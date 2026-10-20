using RentACar.Modules.Branches.Contracts.Enums;
using RentACar.Modules.Branches.Domain.Entities;
using RentACar.Shared.Abstractions.Geo;

namespace RentACar.Modules.Branches.Infrastructure.Persistence.Seed;

/// <summary>
/// Mert Peker Rent a Car şube ağı. Koordinatlar OpenStreetMap'ten (Overpass, 2026-07): Ercan yolcu terminali
/// way/624460625, Girne Limanı node/2012091240, Metehan node/12186123820, Beyarmudu node/873868562,
/// Akyar node/21762864. Ofis noktaları ilçe merkezlerinin (place=city/town) yakınında seçildi.
/// </summary>
internal static class BranchSeedData
{
    /// <summary>Şirketin tek iletişim numarası (personelli noktalar).</summary>
    public const string CompanyPhone = "0542 878 8672";

    public static IEnumerable<Branch> Create()
    {
        var allDay = (Open: new TimeOnly(0, 0), Close: new TimeOnly(23, 59));
        var office = (Open: new TimeOnly(8, 30), Close: new TimeOnly(19, 0));

        yield return Branch.Create("GRN-OFC", "Girne Merkez Ofis", BranchType.Office, District.Girne,
            "Ziya Rızkı Caddesi, Girne", new GeoPoint(33.31890, 35.33720), CompanyPhone, office.Open, office.Close, 0m);
        yield return Branch.Create("LEF-OFC", "Lefkoşa Dereboyu Ofis", BranchType.Office, District.Lefkosa,
            "Mehmet Akif Caddesi (Dereboyu), Lefkoşa", new GeoPoint(33.35450, 35.19300), CompanyPhone, office.Open, office.Close, 0m);
        yield return Branch.Create("MAG-OFC", "Gazimağusa Ofis", BranchType.Office, District.Gazimagusa,
            "Salamis Yolu, Gazimağusa", new GeoPoint(33.93000, 35.12900), CompanyPhone, office.Open, office.Close, 0m);
        yield return Branch.Create("ECN-DSK", "Ercan Havalimanı Karşılama", BranchType.AirportDesk, District.Lefkosa,
            "Ercan Uluslararası Havalimanı, Yolcu Terminali", new GeoPoint(33.50372, 35.14835), CompanyPhone, allDay.Open, allDay.Close, 0m);
        yield return Branch.Create("GRN-PRT", "Girne Turizm Limanı", BranchType.PortDesk, District.Girne,
            "Girne Turizm Limanı yolcu salonu", new GeoPoint(33.33214, 35.34315), null, new TimeOnly(7, 0), new TimeOnly(22, 0), 350m);
        yield return Branch.Create("MTH-BRD", "Metehan Sınır Kapısı", BranchType.BorderCrossing, District.Lefkosa,
            "Metehan (Kermiya) geçiş noktası, kuzey otoparkı", new GeoPoint(33.32300, 35.18300), null, allDay.Open, allDay.Close, 600m);
        yield return Branch.Create("BYM-BRD", "Beyarmudu Sınır Kapısı", BranchType.BorderCrossing, District.Gazimagusa,
            "Beyarmudu (Pergamos) geçiş noktası", new GeoPoint(33.70650, 35.03950), null, allDay.Open, allDay.Close, 900m);
        yield return Branch.Create("AKY-BRD", "Akyar Sınır Kapısı", BranchType.BorderCrossing, District.Gazimagusa,
            "Akyar (Strovilia) geçiş noktası", new GeoPoint(33.90149, 35.09618), null, allDay.Open, allDay.Close, 900m);
    }
}
