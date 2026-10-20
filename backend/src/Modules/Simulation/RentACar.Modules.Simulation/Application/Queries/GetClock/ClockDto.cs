namespace RentACar.Modules.Simulation.Application.Queries.GetClock;

/// <summary>Uygulama saati ve hızı (simüle saniye ÷ gerçek saniye); istemci aradaki zamanı kendisi ilerletir.</summary>
internal sealed record ClockDto(DateTimeOffset Now, double Speed);
