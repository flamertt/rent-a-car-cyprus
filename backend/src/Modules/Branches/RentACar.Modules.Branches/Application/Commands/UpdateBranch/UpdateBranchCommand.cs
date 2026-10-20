using RentACar.Modules.Branches.Contracts.Dtos;
using RentACar.Modules.Branches.Contracts.Enums;
using RentACar.Shared.Abstractions.Geo;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Branches.Application.Commands.UpdateBranch;

internal sealed record UpdateBranchCommand(
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
    decimal DeliveryFee) : ICommand<BranchDto>;
