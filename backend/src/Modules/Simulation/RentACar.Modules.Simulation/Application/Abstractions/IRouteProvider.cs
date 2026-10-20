using RentACar.Shared.Abstractions.Geo;

namespace RentACar.Modules.Simulation.Application.Abstractions;

/// <summary>İki nokta arası sürüş rotası; servis yoksa bile bir rota (yedek) döner.</summary>
internal interface IRouteProvider
{
    Task<RouteResult> RouteAsync(GeoPoint from, GeoPoint to, CancellationToken cancellationToken);
}
