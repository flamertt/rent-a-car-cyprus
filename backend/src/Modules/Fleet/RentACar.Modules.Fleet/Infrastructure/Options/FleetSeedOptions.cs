namespace RentACar.Modules.Fleet.Infrastructure.Options;

/// <summary>appsettings: <c>Fleet</c> bölümü.</summary>
internal sealed class FleetSeedOptions
{
    public const string SectionName = "Fleet";

    /// <summary>
    /// Filo büyüklüğü (emekliler hariç). Açılışta filo bunun altındaysa model karışımı ve şube payları korunarak
    /// tamamlanır; üstündeyse araç silinmez.
    /// </summary>
    public int TargetSize { get; set; } = 300;
}
