using RentACar.Modules.Branches.Contracts.Enums;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Branches.Application.Queries.FindNearestBranches;

internal sealed record FindNearestBranchesQuery(double Lon, double Lat, BranchType? Type, int Limit)
    : IQuery<IReadOnlyList<NearestBranchDto>>;
