using RentACar.Modules.Branches.Contracts.Enums;
using RentACar.Shared.Abstractions.Geo;

namespace RentACar.Modules.Branches.Contracts.Dtos;

public sealed record BranchDto(
    Guid Id,
    string Code,
    string Name,
    BranchType Type,
    District District,
    string Address,
    GeoPoint Location,
    string? Phone,
    TimeOnly OpensAt,
    TimeOnly ClosesAt,
    bool IsActive,
    decimal DeliveryFee);
