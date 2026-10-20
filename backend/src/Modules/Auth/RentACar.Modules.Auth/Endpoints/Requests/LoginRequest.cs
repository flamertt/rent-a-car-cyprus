namespace RentACar.Modules.Auth.Endpoints.Requests;

internal sealed record LoginRequest(string? Email, string? Password);
