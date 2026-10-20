using RentACar.Shared.Abstractions.Geo;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Fleet.Application.Queries.GetFleetGeoJson;

internal sealed record GetFleetGeoJsonQuery : IQuery<GeoJsonFeatureCollection>;
