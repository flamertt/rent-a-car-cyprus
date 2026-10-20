namespace RentACar.Modules.Simulation.Endpoints.Requests;

internal sealed record ControlSimulationRequest(bool? Running, double? Speed);
