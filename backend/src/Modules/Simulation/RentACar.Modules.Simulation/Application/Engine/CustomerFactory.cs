using System.Text;
using RentACar.Modules.Reservations.Contracts.Requests;

namespace RentACar.Modules.Simulation.Application.Engine;

/// <summary>
/// Sahte müşteri. Uyruk dağılımı KKTC giriş istatistiklerine yakın: hava/deniz yolcusunun büyük kısmı TC,
/// ardından İngiltere ve Kuzey/Doğu Avrupa; yerel (KKTC) kiracılar da var. E-postalar example.com (ayrılmış alan adı).
/// </summary>
internal static class CustomerFactory
{
    private sealed record Origin(string Code, double Weight, string[] First, string[] Last);

    private static readonly Origin[] Origins =
    [
        new("TR", 52,
            ["Ahmet", "Mehmet", "Ayşe", "Fatma", "Emre", "Zeynep", "Can", "Elif", "Burak", "Selin", "Mert", "Deniz", "Ece", "Oğuz", "Gizem", "Kerem", "Yasemin", "Hakan", "Derya", "Serkan"],
            ["Yılmaz", "Kaya", "Demir", "Şahin", "Çelik", "Yıldız", "Aydın", "Öztürk", "Arslan", "Doğan", "Kılıç", "Aslan", "Çetin", "Koç", "Kurt", "Özdemir"]),
        new("CY", 8,
            ["Hüseyin", "Mustafa", "Emine", "Hatice", "Erhan", "Gülsen", "Tuncay", "Nesrin", "Ozan", "Pembe"],
            ["Hüseyinoğlu", "Mehmetoğlu", "Ahmetoğlu", "Ersoy", "Özerdağ", "Gürsel", "Talat", "Denktaş"]),
        new("GB", 14,
            ["James", "Oliver", "Emily", "Sophie", "Harry", "Charlotte", "George", "Amelia", "Jack", "Olivia"],
            ["Smith", "Jones", "Taylor", "Brown", "Williams", "Wilson", "Evans", "Thomas", "Roberts", "Walker"]),
        new("DE", 8,
            ["Lukas", "Anna", "Felix", "Lena", "Jonas", "Laura", "Maximilian", "Mia"],
            ["Müller", "Schmidt", "Schneider", "Fischer", "Weber", "Meyer", "Wagner", "Becker"]),
        new("RU", 6,
            ["Ivan", "Olga", "Dmitri", "Natalia", "Sergei", "Anastasia", "Alexei", "Elena"],
            ["Ivanov", "Petrova", "Smirnov", "Kuznetsova", "Popov", "Volkova", "Sokolov"]),
        new("UA", 3, ["Andrii", "Oksana", "Taras", "Iryna"], ["Shevchenko", "Kovalenko", "Bondarenko", "Melnyk"]),
        new("IR", 4, ["Reza", "Maryam", "Ali", "Sara", "Hossein", "Neda"], ["Hosseini", "Ahmadi", "Mohammadi", "Karimi", "Rezaei"]),
        new("NL", 2, ["Daan", "Sanne", "Bram", "Lotte"], ["de Jong", "Jansen", "de Vries", "Bakker"]),
        new("SE", 3, ["Erik", "Elsa", "Lars", "Maja"], ["Johansson", "Andersson", "Karlsson", "Nilsson"]),
    ];

    private static int _sequence;

    public static (CustomerDetailsRequest Details, int Age) Create(Random rng, DateOnly pickupDate)
    {
        var origin = rng.Pick(Origins, o => o.Weight);
        var first = origin.First[rng.Next(origin.First.Length)];
        var last = origin.Last[rng.Next(origin.Last.Length)];

        // Yaş 21–75, çoğunluk 25–55.
        var age = 21 + (int)(Math.Pow(rng.NextDouble(), 1.35) * 54);
        var dob = pickupDate.AddYears(-age).AddDays(-rng.Next(0, 365));
        var licenseAge = 18 + rng.Next(0, Math.Max(1, Math.Min(10, age - 19)));
        var issued = dob.AddYears(licenseAge).AddDays(rng.Next(0, 300));
        var n = Interlocked.Increment(ref _sequence);

        var details = new CustomerDetailsRequest(
            first,
            last,
            $"{Ascii(first)}.{Ascii(last)}.{rng.Next(100, 999)}{n}@example.com",
            null,
            origin.Code,
            dob,
            $"{origin.Code}{rng.NextInt64(10_000_000, 99_999_999)}",
            issued,
            origin.Code);
        return (details, age);
    }

    private static string Ascii(string s)
    {
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s.ToLowerInvariant())
        {
            var mapped = ch switch
            {
                'ç' => 'c', 'ğ' => 'g', 'ı' => 'i', 'ö' => 'o', 'ş' => 's', 'ü' => 'u', 'ä' => 'a', 'é' => 'e',
                ' ' => '-',
                _ => ch,
            };
            if (char.IsAsciiLetterOrDigit(mapped) || mapped == '-')
            {
                sb.Append(mapped);
            }
        }

        return sb.ToString();
    }
}
