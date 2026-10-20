using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RentACar.Shared.Abstractions.Modules;

/// <summary>
/// Modular monolith'in tek giriş noktası. Her modül kendi servislerini, DbContext'ini ve
/// uç noktalarını burada kaydeder; host (RentACar.Api) yalnız bu arayüzü tanır.
/// </summary>
public interface IModule
{
    /// <summary>Kısa, küçük harf ad: URL ön eki (/api/{name}) ve PostgreSQL şeması olarak kullanılır.</summary>
    string Name { get; }

    void Register(IServiceCollection services, IConfiguration configuration);

    void MapEndpoints(IEndpointRouteBuilder endpoints);
}

/// <summary>
/// Açılışta sırayla çalışır: migration + tohum veri. <see cref="Order"/> küçük olan önce
/// (örn. Filo tohumu şubelere ihtiyaç duyar → Branches önce).
/// </summary>
public interface IModuleInitializer
{
    int Order { get; }

    Task InitializeAsync(CancellationToken cancellationToken);
}
