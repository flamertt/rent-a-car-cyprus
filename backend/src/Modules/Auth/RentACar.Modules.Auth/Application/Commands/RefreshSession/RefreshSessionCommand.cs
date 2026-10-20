using RentACar.Modules.Auth.Application.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Auth.Application.Commands.RefreshSession;

/// <summary>Yenileme belirteciyle (çerezden) yeni erişim belirteci; yenileme belirteci döndürülür.</summary>
internal sealed record RefreshSessionCommand(string? RefreshToken) : ICommand<AuthResult>;
