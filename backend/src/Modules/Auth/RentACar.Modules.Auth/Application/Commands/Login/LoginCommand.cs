using RentACar.Modules.Auth.Application.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Auth.Application.Commands.Login;

internal sealed record LoginCommand(string? Email, string? Password) : ICommand<AuthResult>;
