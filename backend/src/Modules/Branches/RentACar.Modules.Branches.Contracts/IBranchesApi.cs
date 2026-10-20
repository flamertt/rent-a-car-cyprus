using RentACar.Modules.Branches.Contracts.Dtos;

namespace RentACar.Modules.Branches.Contracts;

/// <summary>Şubeler modülünün diğer modüllere açtığı tek yüz.</summary>
public interface IBranchesApi
{
    Task<BranchDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BranchDto>> ListActiveAsync(CancellationToken cancellationToken = default);
}
