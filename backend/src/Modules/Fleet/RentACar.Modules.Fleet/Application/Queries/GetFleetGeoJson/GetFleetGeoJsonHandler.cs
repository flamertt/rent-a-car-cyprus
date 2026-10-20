using RentACar.Modules.Fleet.Application.Abstractions;
using RentACar.Shared.Abstractions.Geo;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Fleet.Application.Queries.GetFleetGeoJson;

/// <summary>Anlık görüntü bellekteki canlı akıştan (veritabanına gidilmez).</summary>
internal sealed class GetFleetGeoJsonHandler(IFleetLiveFeed live) : IQueryHandler<GetFleetGeoJsonQuery, GeoJsonFeatureCollection>
{
    public Task<GeoJsonFeatureCollection> HandleAsync(GetFleetGeoJsonQuery query, CancellationToken cancellationToken) =>
        Task.FromResult(new GeoJsonFeatureCollection(live.ChangedSince(0, out _)
            .Select(v => new GeoJsonFeature(
                v.Id.ToString(),
                new GeoJsonPoint([v.Lon, v.Lat]),
                new Dictionary<string, object?>
                {
                    ["plate"] = v.Plate,
                    ["category"] = v.CategoryCode,
                    ["body"] = v.Body.ToString(),
                    ["color"] = v.Color,
                    ["status"] = v.Status.ToString(),
                    ["heading"] = v.HeadingDeg,
                    ["speed"] = v.SpeedKmh,
                }))
            .ToList()));
}
