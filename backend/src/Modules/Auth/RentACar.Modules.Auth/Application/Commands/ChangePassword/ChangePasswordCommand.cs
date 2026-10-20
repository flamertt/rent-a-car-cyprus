using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Auth.Application.Commands.ChangePassword;

/// <summary>Şifre değişince kullanıcının tüm oturumları kapanır (her cihazda yeniden giriş).</summary>
internal sealed record ChangePasswordCommand(Guid UserId, string? CurrentPassword, string? NewPassword) : ICommand<Unit>;
