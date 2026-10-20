using RentACar.Modules.Simulation.Application.Abstractions;
using RentACar.Modules.Simulation.Domain.Entities;

namespace RentACar.Modules.Simulation.Application.Engine;

/// <summary>Olay günlüğü (admin panelindeki akış). Kare sonunda yolculuklarla birlikte kaydedilir.</summary>
internal sealed class EventLog(ISimulationDbContext db)
{
    public void Write(DateTimeOffset at, string kind, string message, Guid? vehicleId = null, string? reservationCode = null) =>
        db.Events.Add(new SimEvent { At = at, Kind = kind, Message = message, VehicleId = vehicleId, ReservationCode = reservationCode });
}
