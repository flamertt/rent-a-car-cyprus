using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Auth.Application.Commands.Logout;

/// <summary>Çıkış: belirtecin ailesi (bu cihazdaki oturum) kapatılır. Belirteç yoksa/geçersizse sessizce geçer.</summary>
internal sealed record LogoutCommand(string? RefreshToken) : ICommand<Unit>;
