using RentACar.Modules.Simulation.Application.Queries.GetSimulationStatus;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Simulation.Application.Commands.ControlSimulation;

/// <summary>Başlat / duraklat / hız; boş alan mevcut değeri korur.</summary>
internal sealed record ControlSimulationCommand(bool? Running, double? Speed) : ICommand<SimulationStatusDto>;
