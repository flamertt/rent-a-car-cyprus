namespace RentACar.Modules.Simulation.Application.Engine;

internal static class SimRandom
{
    /// <summary>Kimliğe bağlı tekrarlanabilir rastgelelik: aynı rezervasyon her karede aynı kararı verir (gelmedi, gecikme…).</summary>
    public static Random For(Guid id) => new(BitConverter.ToInt32(id.ToByteArray(), 4));

    public static T Pick<T>(this Random rng, IReadOnlyList<T> items, Func<T, double> weight)
    {
        var total = items.Sum(weight);
        var roll = rng.NextDouble() * total;
        foreach (var item in items)
        {
            roll -= weight(item);
            if (roll <= 0)
            {
                return item;
            }
        }

        return items[^1];
    }

    public static bool Chance(this Random rng, double probability) => rng.NextDouble() < probability;

    /// <summary>Knuth: küçük λ için Poisson örneği (karede beklenen talep).</summary>
    public static int Poisson(this Random rng, double lambda)
    {
        if (lambda <= 0)
        {
            return 0;
        }

        var limit = Math.Exp(-Math.Min(lambda, 30));
        var k = 0;
        var p = 1.0;
        do
        {
            k++;
            p *= rng.NextDouble();
        }
        while (p > limit);

        return k - 1;
    }
}
