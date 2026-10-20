namespace RentACar.Modules.Simulation.Domain.Enums;

internal enum TripState
{
    /// <summary>Bir sonraki bacağın kalkış saatini bekliyor (park hâlinde).</summary>
    Waiting,

    Driving,

    Finished,
}
