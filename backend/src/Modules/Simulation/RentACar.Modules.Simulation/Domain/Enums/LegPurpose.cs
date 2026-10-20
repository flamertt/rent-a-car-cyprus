namespace RentACar.Modules.Simulation.Domain.Enums;

internal enum LegPurpose
{
    ToStay,
    Excursion,
    BackToStay,
    ToReturnBranch,
    ToPickupBranch,
    ToOffice,

    /// <summary>Boştaki aracın servis turu (yıkama/bakım) noktasına gidişi; varışta bekleyip <see cref="FromService"/>.</summary>
    ToService,

    /// <summary>Servis turundan çıktığı şubeye dönüş.</summary>
    FromService,
}
