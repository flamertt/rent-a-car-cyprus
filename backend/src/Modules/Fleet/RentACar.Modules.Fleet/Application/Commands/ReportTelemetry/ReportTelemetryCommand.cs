using RentACar.Modules.Fleet.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Fleet.Application.Commands.ReportTelemetry;

internal sealed record ReportTelemetryCommand(IReadOnlyList<VehicleTelemetryDto> Telemetry) : ICommand<Unit>;
