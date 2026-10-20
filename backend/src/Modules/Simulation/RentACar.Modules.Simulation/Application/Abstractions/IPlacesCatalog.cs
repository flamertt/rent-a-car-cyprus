using RentACar.Modules.Simulation.Domain.ValueObjects;

namespace RentACar.Modules.Simulation.Application.Abstractions;

/// <summary>Müşterilerin konakladığı bölgeler ve gezdiği yerler.</summary>
internal interface IPlacesCatalog
{
    IReadOnlyList<Place> Stays { get; }

    IReadOnlyList<Place> Attractions { get; }
}
