using RentACar.Modules.Fleet.Contracts.Enums;
using RentACar.Modules.Fleet.Domain.Entities;

namespace RentACar.Modules.Fleet.Infrastructure.Persistence.Seed;

/// <summary>
/// Mert Peker Rent a Car filosunun model karışımı (40'lık şablon; <c>Fleet:TargetSize</c> büyüklüğüne aynı oranlarla
/// ölçeklenir). KKTC trafiği soldan akar ve filolar ağırlıkla sağdan direksiyonlu
/// otomatik araçlardır (Vitz, Note, Aqua, Noah…). "Z" kiralık izni yeni araç ister ve 10 yaşını dolduran
/// araçta yenilenmez (2012 tasarısı) → model yılları 2021–2026. Şube payı Girne &gt; Ercan &gt; Lefkoşa &gt;
/// Gazimağusa: yatak kapasitesi ve OSM car_rental kümeleriyle uyumlu (docs/KKTC-VERI-KAYNAKLARI.md §5.2, §10).
/// </summary>
internal static class FleetSeedData
{
    public static readonly string[] Colors = ["Beyaz", "Gümüş", "Siyah", "Gri", "Kırmızı", "Lacivert", "İnci Beyazı"];

    /// <summary>Stok tutan şubelerin kodu ve araç payı.</summary>
    public static readonly (string BranchCode, int Share)[] Distribution =
        [("GRN-OFC", 15), ("ECN-DSK", 12), ("LEF-OFC", 8), ("MAG-OFC", 5)];

    public static readonly (string Category, string Make, string Model, int Count)[] Fleet =
    [
        ("MCAR", "Kia", "Picanto", 3), ("MCAR", "Hyundai", "i10", 2),
        ("ECAR", "Toyota", "Vitz", 4), ("ECAR", "Nissan", "Note", 3), ("ECAR", "Honda", "Fit", 3),
        ("CDAR", "Toyota", "Corolla Axio", 3), ("CDAR", "Nissan", "Sylphy", 2),
        ("CDAH", "Toyota", "Aqua", 3), ("CDAH", "Toyota", "Prius", 2),
        ("CFAR", "Honda", "Vezel", 2), ("CFAR", "Nissan", "Juke", 2),
        ("IFAR", "Nissan", "X-Trail", 2), ("IFAR", "Toyota", "RAV4", 1),
        ("IVAR", "Toyota", "Noah", 2), ("IVAR", "Nissan", "Serena", 1),
        ("PDAR", "Mercedes-Benz", "C 200", 1), ("PDAR", "BMW", "320i", 1),
        ("STAR", "Mini", "Cooper Cabrio", 2), ("STAR", "Mazda", "MX-5", 1),
    ];

    public static IReadOnlyDictionary<string, VehicleCategory> Categories() => new Dictionary<string, VehicleCategory>
    {
        ["MCAR"] = VehicleCategory.Create("MCAR", "Mini", "Kia Picanto, Hyundai i10", BodyType.Hatchback, Transmission.Automatic, FuelType.Petrol, 4, 5, 1, 21, 5_000m, 10),
        ["ECAR"] = VehicleCategory.Create("ECAR", "Ekonomi", "Toyota Vitz, Nissan Note, Honda Fit", BodyType.Hatchback, Transmission.Automatic, FuelType.Petrol, 5, 5, 2, 21, 5_000m, 20),
        ["CDAR"] = VehicleCategory.Create("CDAR", "Kompakt Sedan", "Toyota Corolla Axio, Nissan Sylphy", BodyType.Sedan, Transmission.Automatic, FuelType.Petrol, 5, 4, 3, 21, 7_500m, 30),
        ["CDAH"] = VehicleCategory.Create("CDAH", "Kompakt Hibrit", "Toyota Aqua, Toyota Prius", BodyType.Hatchback, Transmission.Automatic, FuelType.Hybrid, 5, 5, 2, 21, 7_500m, 40),
        ["CFAR"] = VehicleCategory.Create("CFAR", "Kompakt SUV", "Honda Vezel, Nissan Juke", BodyType.Suv, Transmission.Automatic, FuelType.Petrol, 5, 5, 3, 23, 10_000m, 50),
        ["IFAR"] = VehicleCategory.Create("IFAR", "SUV", "Nissan X-Trail, Toyota RAV4", BodyType.Suv, Transmission.Automatic, FuelType.Petrol, 5, 5, 4, 25, 12_500m, 60),
        ["IVAR"] = VehicleCategory.Create("IVAR", "Minivan (8 kişi)", "Toyota Noah, Nissan Serena", BodyType.Minivan, Transmission.Automatic, FuelType.Petrol, 8, 5, 4, 25, 12_500m, 70),
        ["PDAR"] = VehicleCategory.Create("PDAR", "Premium", "Mercedes-Benz C 200, BMW 320i", BodyType.Sedan, Transmission.Automatic, FuelType.Petrol, 5, 4, 3, 25, 20_000m, 80),
        ["STAR"] = VehicleCategory.Create("STAR", "Cabrio", "Mini Cooper Cabrio, Mazda MX-5", BodyType.Convertible, Transmission.Automatic, FuelType.Petrol, 4, 2, 1, 25, 15_000m, 90),
    };
}
