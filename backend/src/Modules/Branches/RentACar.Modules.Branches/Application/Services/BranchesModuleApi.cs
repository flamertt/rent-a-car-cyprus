using RentACar.Modules.Branches.Application.Queries.GetBranch;
using RentACar.Modules.Branches.Application.Queries.ListBranches;
using RentACar.Modules.Branches.Contracts;
using RentACar.Modules.Branches.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Branches.Application.Services;

internal sealed class BranchesModuleApi(IDispatcher dispatcher) : IBranchesApi
{
    public Task<BranchDto?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        dispatcher.QueryAsync(new GetBranchQuery(id), cancellationToken);

    public Task<IReadOnlyList<BranchDto>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        dispatcher.QueryAsync(new ListBranchesQuery(OnlyActive: true), cancellationToken);
}
