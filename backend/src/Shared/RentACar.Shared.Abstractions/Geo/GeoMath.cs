namespace RentACar.Shared.Abstractions.Geo;

public static class GeoMath
{
    private const double EarthRadiusMeters = 6_371_008.8;

    public static double DistanceMeters(GeoPoint a, GeoPoint b)
    {
        var dLat = ToRad(b.Lat - a.Lat);
        var dLon = ToRad(b.Lon - a.Lon);
        var h = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
            + Math.Cos(ToRad(a.Lat)) * Math.Cos(ToRad(b.Lat)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return 2 * EarthRadiusMeters * Math.Asin(Math.Min(1, Math.Sqrt(h)));
    }

    /// <summary>Pusula yönü: 0 = kuzey, saat yönünde derece.</summary>
    public static double BearingDeg(GeoPoint from, GeoPoint to)
    {
        var φ1 = ToRad(from.Lat);
        var φ2 = ToRad(to.Lat);
        var Δλ = ToRad(to.Lon - from.Lon);
        var y = Math.Sin(Δλ) * Math.Cos(φ2);
        var x = Math.Cos(φ1) * Math.Sin(φ2) - Math.Sin(φ1) * Math.Cos(φ2) * Math.Cos(Δλ);
        return (Math.Atan2(y, x) * 180 / Math.PI + 360) % 360;
    }

    /// <summary>Şehir ölçeğinde doğrusal ara değer (büyük daire gerekmez).</summary>
    public static GeoPoint Lerp(GeoPoint a, GeoPoint b, double t) =>
        new(a.Lon + (b.Lon - a.Lon) * t, a.Lat + (b.Lat - a.Lat) * t);

    private static double ToRad(double deg) => deg * Math.PI / 180;
}
