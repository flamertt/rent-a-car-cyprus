namespace RentACar.Modules.Simulation.Domain.Enums;

internal enum TripKind
{
    /// <summary>Personel aracı teslim noktasına götürüyor.</summary>
    Delivery,

    /// <summary>Müşteri kiralama süresince geziyor.</summary>
    Rental,

    /// <summary>Personel aracı şubeye götürüyor: stoksuz noktadan (liman/sınır kapısı) ofise dönüş ya da şubeler arası filo transferi.</summary>
    Reposition,
}
