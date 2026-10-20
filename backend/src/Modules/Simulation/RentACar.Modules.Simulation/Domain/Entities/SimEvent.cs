namespace RentACar.Modules.Simulation.Domain.Entities;

/// <summary>Admin panelindeki olay akışının bir satırı.</summary>
internal sealed class SimEvent
{
    public long Id { get; set; }

    public DateTimeOffset At { get; set; }

    public string Kind { get; set; } = null!;

    public string Message { get; set; } = null!;

    public Guid? VehicleId { get; set; }

    public string? ReservationCode { get; set; }
}
