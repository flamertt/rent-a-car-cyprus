using RentACar.Modules.Branches.Contracts.Dtos;
using RentACar.Modules.Branches.Contracts.Enums;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Branches.Application.Queries.ListBranches;

/// <summary><paramref name="OnlyActive"/> false: yönetim paneli (pasifler dahil, koda göre).</summary>
internal sealed record ListBranchesQuery(bool OnlyActive, BranchType? Type = null, District? District = null)
    : IQuery<IReadOnlyList<BranchDto>>;
