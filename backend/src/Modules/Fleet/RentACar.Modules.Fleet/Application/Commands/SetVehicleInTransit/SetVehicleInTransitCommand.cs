using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Fleet.Application.Commands.SetVehicleInTransit;

/// <summary><paramref name="InTransit"/> false ise araç <paramref name="ArrivedBranchId"/> şubesine park edilir.</summary>
internal sealed record SetVehicleInTransitCommand(Guid VehicleId, bool InTransit, Guid? ArrivedBranchId) : ICommand<Unit>;
