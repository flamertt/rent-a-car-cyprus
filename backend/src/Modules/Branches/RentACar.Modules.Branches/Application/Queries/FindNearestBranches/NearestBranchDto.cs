using RentACar.Modules.Branches.Contracts.Dtos;

namespace RentACar.Modules.Branches.Application.Queries.FindNearestBranches;

internal sealed record NearestBranchDto(BranchDto Branch, double DistanceKm);
