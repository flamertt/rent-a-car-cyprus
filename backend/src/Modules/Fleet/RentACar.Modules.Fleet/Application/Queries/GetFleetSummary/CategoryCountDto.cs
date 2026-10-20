namespace RentACar.Modules.Fleet.Application.Queries.GetFleetSummary;

internal sealed record CategoryCountDto(string Code, string Name, int Total, int Available, int OnRent);
