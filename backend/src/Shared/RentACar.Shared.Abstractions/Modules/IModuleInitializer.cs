namespace RentACar.Shared.Abstractions.Modules;

/// <summary>
/// Açılışta sırayla çalışır: migration + tohum veri. <see cref="Order"/> küçük olan önce
/// (örn. Filo tohumu şubelere ihtiyaç duyar → Branches önce).
/// </summary>
public interface IModuleInitializer
{
    int Order { get; }

    Task InitializeAsync(CancellationToken cancellationToken);
}
