using RentACar.Modules.Branches.Contracts.Enums;
using RentACar.Shared.Abstractions.Geo;

namespace RentACar.Modules.Branches.Endpoints.Requests;

internal sealed record UpsertBranchRequest(
    string Code,
    string Name,
    BranchType Type,
    District District,
    string Address,
    GeoPoint Location,
    string? Phone,
    TimeOnly OpensAt,
    TimeOnly ClosesAt,
    decimal DeliveryFee);
