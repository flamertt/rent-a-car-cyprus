using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Reservations.Application.Commands.MarkNoShows;

/// <summary>Teslim saatinden NoShowAfterMinutes geçmiş, alınmamış rezervasyonları kapatır; kapatılan sayısını döner.</summary>
internal sealed record MarkNoShowsCommand : ICommand<int>;
