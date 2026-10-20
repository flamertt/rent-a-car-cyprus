using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentACar.Modules.Fleet.Contracts;
using RentACar.Modules.Pricing.Contracts;
using RentACar.Modules.Pricing.Contracts.Requests;
using RentACar.Modules.Reservations.Application.Abstractions;
using RentACar.Modules.Reservations.Application.Mappings;
using RentACar.Modules.Reservations.Application.Services;
using RentACar.Modules.Reservations.Contracts.Dtos;
using RentACar.Modules.Reservations.Contracts.Events;
using RentACar.Modules.Reservations.Domain.Entities;
using RentACar.Modules.Reservations.Domain.Rules;
using RentACar.Shared.Abstractions.Events;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Reservations.Application.Commands.CreateReservation;

internal sealed class CreateReservationHandler(
    IReservationsDbContext db,
    IFleetApi fleet,
    IPricingApi pricing,
    VehicleAvailability availability,
    ActiveBranchGuard branchGuard,
    CustomerResolver customers,
    IEventBus bus,
    IClock clock,
    IOptions<ReservationRules> options,
    ILogger<CreateReservationHandler> logger) : ICommandHandler<CreateReservationCommand, ReservationDto>
{
    private const int MaxAssignAttempts = 3;

    public async Task<ReservationDto> HandleAsync(CreateReservationCommand command, CancellationToken cancellationToken)
    {
        var r = command.Request;
        var rules = options.Value;
        var now = clock.UtcNow;
        if (r.ReturnAt <= r.PickupAt.AddHours(1))
        {
            throw new DomainException("reservation.invalid_period", "İade, teslimden en az 1 saat sonra olmalı.");
        }

        if (r.PickupAt < now.AddMinutes(rules.MinLeadMinutes))
        {
            throw new DomainException("reservation.lead_time", $"Teslim zamanı en az {rules.MinLeadMinutes} dk sonrası olmalı.");
        }

        if (r.PickupAt > now.AddDays(rules.MaxAdvanceDays))
        {
            throw new DomainException("reservation.too_far", $"En fazla {rules.MaxAdvanceDays} gün sonrası için rezervasyon yapılabilir.");
        }

        await branchGuard.EnsureActiveAsync(r.PickupBranchId, r.ReturnBranchId, cancellationToken);
        var category = await fleet.GetCategoryAsync(r.CategoryId, cancellationToken) ?? throw new NotFoundException("Araç sınıfı", r.CategoryId);
        var customer = await customers.ResolveAsync(r, cancellationToken);

        var pickupDate = KktcTime.LocalDate(r.PickupAt);
        var age = customer.AgeOn(pickupDate);
        if (age < category.MinDriverAge)
        {
            throw new DomainException("reservation.driver_age", $"{category.Name} sınıfı için sürücü en az {category.MinDriverAge} yaşında olmalı.");
        }

        if (customer.LicenseYearsOn(pickupDate) < rules.MinLicenseYears)
        {
            throw new DomainException("reservation.license_years", $"Ehliyet en az {rules.MinLicenseYears} yıllık olmalı.");
        }

        var quote = await pricing.QuoteAsync(new QuoteRequest(
            category.Id, r.PickupAt, r.ReturnAt, r.PickupBranchId, r.ReturnBranchId,
            r.Extras?.Select(e => new QuoteExtraRequest(e.Code, e.Quantity)).ToList(), age), cancellationToken);

        var candidates = await availability.FreeVehiclesAsync(category.Id, r.PickupBranchId, r.PickupAt, r.ReturnAt, cancellationToken);
        if (candidates.Count == 0)
        {
            throw new ConflictException("reservation.sold_out", $"{category.Name} sınıfında bu tarihlerde boş araç yok.");
        }

        var first = candidates[0];
        var reservation = Reservation.Create(
            r.Channel,
            (customer.Id, customer.FullName, customer.Email),
            (category.Id, category.Code),
            (first.Id, first.Plate),
            r.PickupBranchId,
            r.ReturnBranchId,
            r.PickupAt,
            r.ReturnAt,
            rules.Turnaround,
            quote.Days,
            quote.Total,
            quote.Currency,
            quote.Lines.Select(l => new PriceLine { Code = l.Code, Description = l.Description, Quantity = l.Quantity, UnitPrice = l.UnitPrice, Amount = l.Amount }),
            now);
        db.Reservations.Add(reservation);

        // Eşzamanlı iki istek aynı aracı seçerse exclusion constraint ikinciyi reddeder → sıradaki aday.
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                await db.SaveChangesAsync(cancellationToken);
                break;
            }
            catch (DbUpdateException ex) when (db.IsVehicleOverlap(ex) && attempt + 1 < Math.Min(MaxAssignAttempts, candidates.Count))
            {
                var next = candidates[attempt + 1];
                logger.LogInformation("{Plate} başka rezervasyona gitti, {Next} deneniyor", reservation.VehiclePlate, next.Plate);
                reservation.ReassignVehicle(next.Id, next.Plate);
            }
            catch (DbUpdateException ex) when (db.IsVehicleOverlap(ex))
            {
                throw new ConflictException("reservation.sold_out", $"{category.Name} sınıfındaki son araç az önce kiralandı.");
            }
        }

        await bus.PublishAsync(new ReservationConfirmed(reservation.Id, reservation.VehicleId, reservation.PickupBranchId, reservation.PickupAt, now), cancellationToken);
        return reservation.ToDto();
    }
}
