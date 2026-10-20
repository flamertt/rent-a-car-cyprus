using RentACar.Shared.Abstractions.Geo;

namespace RentACar.Modules.Simulation.Application.Abstractions;

/// <summary>Yönlendirme sonucu; <see cref="Provider"/> "osrm" ya da "line" (yedek).</summary>
internal sealed record RouteResult(string Provider, GeoPoint[] Points, double[] SegmentDurations, double[] SegmentDistances);
