using RentACar.Modules.Branches.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Branches.Application.Queries.GetBranch;

internal sealed record GetBranchQuery(Guid Id) : IQuery<BranchDto?>;
