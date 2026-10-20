using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Commands.SetDailyRate;

internal sealed record SetDailyRateCommand(Guid CategoryId, decimal DailyRate) : ICommand<Unit>;
