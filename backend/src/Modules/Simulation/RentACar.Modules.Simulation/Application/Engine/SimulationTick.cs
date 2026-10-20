using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Branches.Contracts;
using RentACar.Modules.Fleet.Contracts;
using RentACar.Modules.Simulation.Application.Abstractions;
using RentACar.Modules.Simulation.Application.Services;
using RentACar.Modules.Simulation.Domain.Entities;
using RentACar.Modules.Simulation.Domain.Enums;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Simulation.Application.Engine;

/// <summary>
/// Bir motor karesi: yolculuklar her karede ilerler; talep üretimi, operasyon (teslimat, teslim, transfer) ve
/// asgari hareket her <see cref="OperationsEveryTicks"/> karede bir çalışır. Hızlı ileri sarmada her karede çalışır: kare
/// orada ≥ 30 simüle dakikadır, seyrek operasyonda teslimat → teslim zinciri "gelmedi" eşiğini aşar. Diğer modüllere yalnız
/// Contracts arayüzleriyle dokunur — bir web kullanıcısıyla aynı kurallar.
/// </summary>
internal sealed class SimulationTick(
    ISimulationDbContext db,
    IFleetApi fleet,
    IBranchesApi branches,
    IPlacesCatalog places,
    SimulationRuntime runtime,
    DemandGenerator demand,
    FleetDispatcher dispatcher,
    ActivityFloor floor,
    TripRunner runner,
    RouteMemory routes,
    IClock clock)
{
    private const int OperationsEveryTicks = 4;
    private const int PruneEveryTicks = 1200;
    private const int KeepEvents = 10_000;
    private const int MaxRoutesInMemory = 4_000;
    private static readonly TimeSpan KeepRoutes = TimeSpan.FromDays(2);

    public async Task RunAsync(CancellationToken ct)
    {
        var now = clock.UtcNow;
        var state = await db.State.SingleAsync(ct);
        var trips = await db.Trips.Where(t => t.State != TripState.Finished).ToListAsync(ct);
        var ctx = new SimContext(now, await fleet.ListCategoriesAsync(ct), await branches.ListActiveAsync(ct), places, runtime.Random);

        if (runtime.Tick++ % OperationsEveryTicks == 0 || runtime.CatchingUp)
        {
            var fleetSize = (await fleet.ListRentableVehiclesAsync(null, ct)).Count;
            await demand.RunAsync(ctx, state, fleetSize, ct);
            void AddTrip(Trip trip)
            {
                db.Trips.Add(trip);
                trips.Add(trip);
            }

            await dispatcher.RunAsync(ctx, state, trips, AddTrip, ct);

            // Hızlı ileri sarmada görüntü yok; yapay hareket yalnız gerçek zamanda.
            if (!runtime.CatchingUp)
            {
                await floor.RunAsync(ctx, trips, AddTrip, ct);
            }
        }

        await runner.RunAsync(ctx, state, trips, ct);

        state.SimNow = now;
        state.Running = runtime.Running;
        state.Speed = runtime.Speed;
        state.LastError = runtime.LastError;
        await db.SaveChangesAsync(ct);

        // Olay akışı yalnız son kayıtları gösterir; tablo sınırsız büyümesin (~10 dakikada bir). Rotalar da: otel ve park
        // noktaları dağıtık olduğu için rota çeşidi çok; yolculukta olmayan eski rotalar silinir, bellek sınırlanır.
        if (runtime.Tick % PruneEveryTicks == 0)
        {
            var newest = await db.Events.MaxAsync(e => (long?)e.Id, ct) ?? 0;
            await db.Events.Where(e => e.Id < newest - KeepEvents).ExecuteDeleteAsync(ct);

            var cutoff = now - KeepRoutes;
            await db.Routes
                .Where(r => r.CreatedAt < cutoff && !db.Trips.Any(t => t.RouteId == r.Id && t.State != TripState.Finished))
                .ExecuteDeleteAsync(ct);
            routes.Trim(trips.Where(t => t.RouteId is not null).Select(t => t.RouteId!.Value).ToHashSet(), MaxRoutesInMemory);
        }
    }
}
