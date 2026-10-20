using System.Security.Cryptography;
using RentACar.Modules.Reservations.Contracts.Enums;
using RentACar.Modules.Reservations.Domain.Rules;
using RentACar.Shared.Abstractions.Exceptions;

namespace RentACar.Modules.Reservations.Domain.Entities;

/// <summary>
/// Rezervasyon = kiralama sözleşmesinin yaşam döngüsü:
/// Confirmed → Active (araç teslim) → Completed (iade); Confirmed → Cancelled | NoShow.
/// Müşteri adı, sınıf kodu ve plaka diğer modüllerden kopyalanır (anlık görüntü; join yok).
/// </summary>
internal sealed class Reservation
{
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private Reservation()
    {
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = null!;

    public ReservationStatus Status { get; private set; }

    public BookingChannel Channel { get; private set; }

    public Guid CustomerId { get; private set; }

    public string CustomerName { get; private set; } = null!;

    public string CustomerEmail { get; private set; } = null!;

    public Guid CategoryId { get; private set; }

    public string CategoryCode { get; private set; } = null!;

    public Guid VehicleId { get; private set; }

    public string VehiclePlate { get; private set; } = null!;

    public Guid PickupBranchId { get; private set; }

    public Guid ReturnBranchId { get; private set; }

    public DateTimeOffset PickupAt { get; private set; }

    public DateTimeOffset ReturnAt { get; private set; }

    /// <summary>İade + hazırlık süresi; araç bu ana kadar başka rezervasyona verilemez (exclusion constraint).</summary>
    public DateTimeOffset BlockedUntil { get; private set; }

    public DateTimeOffset? PickedUpAt { get; private set; }

    public DateTimeOffset? ReturnedAt { get; private set; }

    public Guid? ActualReturnBranchId { get; private set; }

    public int? StartOdometerKm { get; private set; }

    public int? EndOdometerKm { get; private set; }

    public double? StartFuelLevel { get; private set; }

    public double? EndFuelLevel { get; private set; }

    public int Days { get; private set; }

    public decimal Total { get; private set; }

    public string Currency { get; private set; } = null!;

    public List<PriceLine> Lines { get; private set; } = [];

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ClosedAt { get; private set; }

    public static Reservation Create(
        BookingChannel channel,
        (Guid Id, string Name, string Email) customer,
        (Guid Id, string Code) category,
        (Guid Id, string Plate) vehicle,
        Guid pickupBranchId,
        Guid returnBranchId,
        DateTimeOffset pickupAt,
        DateTimeOffset returnAt,
        TimeSpan turnaround,
        int days,
        decimal total,
        string currency,
        IEnumerable<PriceLine> lines,
        DateTimeOffset now) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            Code = NewCode(),
            Status = ReservationStatus.Confirmed,
            Channel = channel,
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            CustomerEmail = customer.Email,
            CategoryId = category.Id,
            CategoryCode = category.Code,
            VehicleId = vehicle.Id,
            VehiclePlate = vehicle.Plate,
            PickupBranchId = pickupBranchId,
            ReturnBranchId = returnBranchId,
            PickupAt = pickupAt.ToUniversalTime(),
            ReturnAt = returnAt.ToUniversalTime(),
            BlockedUntil = (returnAt + turnaround).ToUniversalTime(),
            Days = days,
            Total = total,
            Currency = currency,
            Lines = lines.ToList(),
            CreatedAt = now,
        };

    /// <summary>Çakışma olursa (aynı araç) başka araçla yeniden denemek için.</summary>
    public void ReassignVehicle(Guid vehicleId, string plate)
    {
        EnsureStatus(ReservationStatus.Confirmed, "yeniden atanamaz");
        VehicleId = vehicleId;
        VehiclePlate = plate;
    }

    public void PickUp(DateTimeOffset now, int odometerKm, double fuelLevel, ReservationRules rules)
    {
        EnsureStatus(ReservationStatus.Confirmed, "teslim edilemez");
        if (now < PickupAt - TimeSpan.FromMinutes(rules.EarlyPickupMinutes))
        {
            throw new DomainException("reservation.too_early", $"Araç en erken {rules.EarlyPickupMinutes} dk önce teslim edilebilir.");
        }

        Status = ReservationStatus.Active;
        PickedUpAt = now;
        StartOdometerKm = odometerKm;
        StartFuelLevel = Math.Clamp(fuelLevel, 0, 1);
    }

    /// <summary>İade: geç kalınan her 24 saat (tolerans sonrası) ortalama günlük fiyattan, eksik yakıt depo oranından eklenir.</summary>
    public void Return(DateTimeOffset now, Guid branchId, int odometerKm, double fuelLevel, ReservationRules rules)
    {
        EnsureStatus(ReservationStatus.Active, "iade alınamaz");
        if (odometerKm < (StartOdometerKm ?? 0))
        {
            throw new DomainException("reservation.odometer", "İade kilometresi teslimden küçük olamaz.");
        }

        Status = ReservationStatus.Completed;
        ReturnedAt = now;
        ClosedAt = now;
        ActualReturnBranchId = branchId;
        EndOdometerKm = odometerKm;
        EndFuelLevel = Math.Clamp(fuelLevel, 0, 1);

        var late = now - ReturnAt - TimeSpan.FromMinutes(rules.GraceMinutes);
        if (late > TimeSpan.Zero)
        {
            var lateDays = (int)Math.Ceiling(late.TotalHours / 24);
            var rental = Lines.FirstOrDefault(l => l.Code == "RENTAL");
            var daily = rental?.UnitPrice ?? Math.Round(Total / Math.Max(1, Days));
            AddLine("LATE_RETURN", $"Geç iade · {lateDays} gün", lateDays, daily);
        }

        var missingFuel = (StartFuelLevel ?? 1) - EndFuelLevel.Value;
        if (missingFuel > 0.05)
        {
            var amount = Math.Round(rules.RefuelPricePerTank * (decimal)missingFuel);
            AddLine("REFUEL", $"Eksik yakıt (%{missingFuel * 100:0})", 1, amount);
        }
    }

    public void Cancel(DateTimeOffset now)
    {
        EnsureStatus(ReservationStatus.Confirmed, "iptal edilemez");
        Status = ReservationStatus.Cancelled;
        ClosedAt = now;
    }

    public void MarkNoShow(DateTimeOffset now)
    {
        EnsureStatus(ReservationStatus.Confirmed, "gelmedi olarak işaretlenemez");
        Status = ReservationStatus.NoShow;
        ClosedAt = now;
    }

    private void AddLine(string code, string description, int quantity, decimal unitPrice)
    {
        var amount = Math.Round(unitPrice * quantity);
        Lines.Add(new PriceLine { Code = code, Description = description, Quantity = quantity, UnitPrice = unitPrice, Amount = amount });
        Total += amount;
    }

    private void EnsureStatus(ReservationStatus expected, string action)
    {
        if (Status != expected)
        {
            throw new ConflictException("reservation.invalid_status", $"{Code}: {Status} durumundaki rezervasyon {action}.");
        }
    }

    private static string NewCode() =>
        "KR-" + string.Create(6, 0, static (span, _) =>
        {
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];
            }
        });
}
