namespace RentACar.Modules.Auth.Endpoints.Requests;

internal sealed record ChangePasswordRequest(string? CurrentPassword, string? NewPassword);
