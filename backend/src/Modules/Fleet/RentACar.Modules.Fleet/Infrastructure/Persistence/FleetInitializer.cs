using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentACar.Modules.Branches.Contracts;
using RentACar.Modules.Fleet.Application.Abstractions;
using RentACar.Modules.Fleet.Application.Mappings;
using RentACar.Modules.Fleet.Contracts.Enums;
using RentACar.Modules.Fleet.Domain.Entities;
using RentACar.Modules.Fleet.Domain.Services;
using RentACar.Modules.Fleet.Infrastructure.Options;
using RentACar.Modules.Fleet.Infrastructure.Persistence.Seed;
using RentACar.Shared.Abstractions.Modules;

namespace RentACar.Modules.Fleet.Infrastructure.Persistence;

/// <summary>Migration + tohum filo (hedef büyüklüğe tamamlanır); ardından canlı akışın ilk görüntüsü (emekliler hariç).</summary>
internal sealed class FleetInitializer(
    FleetDbContext db, IBranchesApi branches, IFleetLiveFeed live, IOptions<FleetSeedOptions> options, ILogger<FleetInitializer> logger)
    : IModuleInitializer
{
    public int Order => 20;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await db.Database.MigrateAsync(cancellationToken);
        if (!await db.Categories.AnyAsync(cancellationToken))
        {
            db.Categories.AddRange(FleetSeedData.Categories().Values);
            await db.SaveChangesAsync(cancellationToken);
        }

        await TopUpAsync(cancellationToken);

        var vehicles = await db.Vehicles.AsNoTracking().Include(v => v.Category)
            .Where(v => v.Status != VehicleStatus.Retired).ToListAsync(cancellationToken);
        foreach (var vehicle in vehicles)
        {
            live.Upsert(vehicle.ToLive());
        }
    }

    /// <summary>
    /// Filo hedefin altındaysa eksik araçlar eklenir: model karışımı <see cref="FleetSeedData.Fleet"/>, şube payı
    /// <see cref="FleetSeedData.Distribution"/>. Stok yalnız personelli noktalarda durur; sınır kapısı / liman
    /// teslimleri transferle yapılır.
    /// </summary>
    private async Task TopUpAsync(CancellationToken ct)
    {
        var existing = await db.Vehicles.CountAsync(v => v.Status != VehicleStatus.Retired, ct);
        var target = options.Value.TargetSize;
        if (existing >= target)
        {
            return;
        }

        var categories = await db.Categories.ToDictionaryAsync(c => c.Code, ct);
        var stock = (await branches.ListActiveAsync(ct)).ToDictionary(b => b.Code);
        var branchCycle = FleetSeedData.Distribution.SelectMany(d => Enumerable.Repeat(d.BranchCode, d.Share)).ToArray();
        var modelCycle = FleetSeedData.Fleet.SelectMany(f => Enumerable.Repeat((f.Category, f.Make, f.Model), f.Count)).ToArray();
        var plates = (await db.Vehicles.Select(v => v.Plate).ToListAsync(ct)).ToHashSet();
        var parked = (await db.Vehicles.Where(v => v.BranchId != null).Select(v => new { v.BranchId, v.ParkingSlot }).ToListAsync(ct))
            .GroupBy(x => x.BranchId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ParkingSlot).ToHashSet());

        // İlk tohum hep aynı plakalarla başlasın (1974); tamamlamada sıra kaldığı yerden sürer.
        var random = new Random(1974 + existing);
        for (var i = existing; i < target; i++)
        {
            var (category, make, model) = modelCycle[i % modelCycle.Length];
            var branch = stock[branchCycle[i % branchCycle.Length]];
            if (!parked.TryGetValue(branch.Id, out var taken))
            {
                parked[branch.Id] = taken = [];
            }

            var slot = ParkingLot.FirstFree(taken);
            taken.Add(slot);
            db.Vehicles.Add(Vehicle.Create(
                NextPlate(random, plates), make, model, random.Next(2021, 2027),
                FleetSeedData.Colors[random.Next(FleetSeedData.Colors.Length)], categories[category],
                branch.Id, branch.Location, slot, random.Next(8_000, 120_000)));
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Filo {Existing} araçtan {Target} araca tamamlandı", existing, target);
    }

    private static string NextPlate(Random random, HashSet<string> taken)
    {
        string plate;
        do
        {
            plate = KktcPlate.RentalPlate(random);
        }
        while (!taken.Add(plate));

        return plate;
    }
}
