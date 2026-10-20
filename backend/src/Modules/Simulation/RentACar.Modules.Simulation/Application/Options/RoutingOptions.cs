namespace RentACar.Modules.Simulation.Application.Options;

internal sealed class RoutingOptions
{
    /// <summary>
    /// OSRM taban adresi. Önerilen: yerelde Geofabrik Cyprus extract'ıyla çalışan OSRM (docker compose --profile routing).
    /// Boş ya da erişilemezse düz çizgi yedeği kullanılır.
    /// </summary>
    public string? OsrmUrl { get; set; } = "http://127.0.0.1:5000";

    /// <summary>Kamu demo sunucusu (router.project-osrm.org) için ≥1000 ms: kullanım politikası saniyede 1 istek.</summary>
    public int MinRequestIntervalMs { get; set; }

    public int TimeoutSeconds { get; set; } = 10;
}
