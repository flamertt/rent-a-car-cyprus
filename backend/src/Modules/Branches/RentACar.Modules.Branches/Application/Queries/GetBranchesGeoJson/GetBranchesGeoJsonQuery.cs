using RentACar.Shared.Abstractions.Geo;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Branches.Application.Queries.GetBranchesGeoJson;

internal sealed record GetBranchesGeoJsonQuery : IQuery<GeoJsonFeatureCollection>;
