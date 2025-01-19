using System.Collections.Generic;

public class GenomeDistanceCache
{
    private readonly Dictionary<(int, int), double> _distances = new();
    private readonly Config _config; // Replace `Config` with configuration type.
    public int Hits { get; private set; }
    public int Misses { get; private set; }

    public GenomeDistanceCache(Config config)
    {
        _config = config;
        Hits = 0;
        Misses = 0;
    }

    public double GetDistance(Genome genome0, Genome genome1) // Replace with your genome type.
    {
        var key = (genome0.Key, genome1.Key);

        if (_distances.TryGetValue(key, out var distance))
        {
            Hits++;
            return distance;
        }

        distance = genome0.Distance(genome1, _config); // Implement the Distance method.
        _distances[key] = distance;
        _distances[(key.Item2, key.Item1)] = distance; // Store for both orders.
        Misses++;

        return distance;
    }
}
