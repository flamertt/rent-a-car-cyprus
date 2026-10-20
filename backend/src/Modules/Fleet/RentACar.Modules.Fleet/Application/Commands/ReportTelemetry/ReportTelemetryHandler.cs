using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Fleet.Application.Abstractions;
using RentACar.Modules.Fleet.Contracts.Enums;
using RentACar.Shared.Abstractions.Geo;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Fleet.Application.Commands.ReportTelemetry;

/// <summary>
/// Yüksek frekanslı yazım: varlık izlemeden, tek UPDATE ile. Yalnız yoldaki (kirada/transferde)
/// araçların konumu güncellenir; şubede park etmiş aracı telemetri yerinden oynatamaz.
/// </summary>
internal sealed class ReportTelemetryHandler(IFleetDbContext db, IFleetLiveFeed live) : ICommandHandler<ReportTelemetryCommand, Unit>
{
    public async Task<Unit> HandleAsync(ReportTelemetryCommand command, CancellationToken cancellationToken)
    {
        foreach (var t in command.Telemetry)
        {
            var point = t.Position.ToPoint();
            var fuel = Math.Clamp(t.FuelLevel, 0, 1);
            var updated = await db.Vehicles
                .Where(v => v.Id == t.VehicleId && (v.Status == VehicleStatus.OnRent || v.Status == VehicleStatus.InTransit))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(v => v.Position, point)
                    .SetProperty(v => v.HeadingDeg, t.HeadingDeg)
                    .SetProperty(v => v.SpeedKmh, t.SpeedKmh)
                    .SetProperty(v => v.OdometerKm, v => Math.Max(v.OdometerKm, t.OdometerKm))
                    .SetProperty(v => v.FuelLevel, fuel)
                    .SetProperty(v => v.LastSeenAt, t.At), cancellationToken);

            if (updated > 0 && live.TryGet(t.VehicleId, out var current))
            {
                live.Upsert(current with
                {
                    Lon = t.Position.Lon, Lat = t.Position.Lat, HeadingDeg = t.HeadingDeg,
                    SpeedKmh = t.SpeedKmh, FuelLevel = fuel, At = t.At,
                });
            }
        }

        return Unit.Value;
    }
}
