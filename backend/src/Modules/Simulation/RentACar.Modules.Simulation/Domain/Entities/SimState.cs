namespace RentACar.Modules.Simulation.Domain.Entities;

/// <summary>Tek satır: çalışma durumu, hız, kaldığı simülasyon anı ve sayaçlar (yeniden başlatmada devam için).</summary>
internal sealed class SimState
{
    public static readonly Guid SingletonId = new("00000000-0000-0000-0000-00000000000a");

    public Guid Id { get; set; } = SingletonId;

    public bool Running { get; set; }

    public double Speed { get; set; }

    public DateTimeOffset SimNow { get; set; }

    public DateTimeOffset LastDemandAt { get; set; }

    public int ReservationsCreated { get; set; }

    public int LostDemand { get; set; }

    public int Pickups { get; set; }

    public int Returns { get; set; }

    public string? LastError { get; set; }
}
