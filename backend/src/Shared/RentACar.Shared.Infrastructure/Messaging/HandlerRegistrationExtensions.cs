using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using RentACar.Shared.Abstractions.Events;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Shared.Infrastructure.Messaging;

public static class HandlerRegistrationExtensions
{
    private static readonly Type[] HandlerInterfaces =
        [typeof(ICommandHandler<,>), typeof(IQueryHandler<,>), typeof(IIntegrationEventHandler<>)];

    /// <summary>Modül derlemesindeki tüm komut, sorgu ve olay işleyicilerini (scoped) kaydeder.</summary>
    public static IServiceCollection AddModuleHandlers(this IServiceCollection services, Assembly assembly)
    {
        foreach (var type in assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }))
        {
            foreach (var contract in type.GetInterfaces()
                         .Where(i => i.IsGenericType && HandlerInterfaces.Contains(i.GetGenericTypeDefinition())))
            {
                services.AddScoped(contract, type);
            }
        }

        return services;
    }
}
