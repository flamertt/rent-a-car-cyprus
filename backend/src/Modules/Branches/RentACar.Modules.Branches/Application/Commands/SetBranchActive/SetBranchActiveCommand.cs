using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Branches.Application.Commands.SetBranchActive;

internal sealed record SetBranchActiveCommand(Guid Id, bool IsActive) : ICommand<Unit>;
