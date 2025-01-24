using System;
using System.Collections.Generic;
using System.Linq;
using RTNEAT_offline.NEAT;
using RTNEAT_offline.NEAT.Reporting;

namespace RTNEAT_offline.NEAT
{
    public class DefaultSpeciesSet
    {
        private readonly Config _config;
        private readonly List<IReporter> _reporters;
        private readonly IEnumerator<int> _indexer;
        public Dictionary<int, Species> Species { get; private set; }
        public Dictionary<int, int> GenomeToSpecies { get; private set; }

        public DefaultSpeciesSet(Config config, List<IReporter> reporters = null)
        {
            _config = config;
            _reporters = reporters ?? new List<IReporter>();
            _indexer = Enumerable.Range(1, int.MaxValue).GetEnumerator();
            Species = new Dictionary<int, Species>();
            GenomeToSpecies = new Dictionary<int, int>();
        }

        public void Speciate(Config config, Dictionary<int, Genome> population, int generation) // Replace `Genome`.
        {
            var compatibilityThreshold = config.CompatibilityThreshold; // Implement this in `Config`.

            var unspeciated = new HashSet<int>(population.Keys);
            var distances = new GenomeDistanceCache(config);
            var newRepresentatives = new Dictionary<int, int>();
            var newMembers = new Dictionary<int, List<int>>();

            foreach (var (sid, species) in Species)
            {
                var candidates = unspeciated
                    .Select(gid => (distances.GetDistance(species.Representative as Genome, population[gid]), gid)) // Replace `Genome`.
                    .ToList();

                var (ignored, newRep) = candidates.MinBy(c => c.Item1); // Requires System.Linq.
                newRepresentatives[sid] = newRep;
                newMembers[sid] = new List<int> { newRep };
                unspeciated.Remove(newRep);
            }

            while (unspeciated.Count > 0)
            {
                var gid = unspeciated.First();
                unspeciated.Remove(gid);

                var candidates = newRepresentatives
                    .Select(pair =>
                    {
                        var (sid, rid) = pair;
                        var rep = population[rid];
                        var dist = distances.GetDistance(rep, population[gid]);
                        return (dist, sid);
                    })
                    .Where(c => c.dist < compatibilityThreshold)
                    .ToList();

                if (candidates.Any())
                {
                    var (_, sid) = candidates.MinBy(c => c.dist); // Requires System.Linq.
                    newMembers[sid].Add(gid);
                }
                else
                {
                    _indexer.MoveNext();
                    var newSid = _indexer.Current;
                    newRepresentatives[newSid] = gid;
                    newMembers[newSid] = new List<int> { gid };
                }
            }

            UpdateSpeciesCollection(population, newRepresentatives, newMembers, generation);
            ReportDistanceStatistics(population.Count, distances);
        }

        private void UpdateSpeciesCollection(
            Dictionary<int, Genome> population,
            Dictionary<int, int> newRepresentatives,
            Dictionary<int, List<int>> newMembers,
            int generation)
        {
            GenomeToSpecies = new Dictionary<int, int>();

            foreach (var (sid, rid) in newRepresentatives)
            {
                if (!Species.TryGetValue(sid, out var species))
                {
                    species = new Species(sid, generation);
                    Species[sid] = species;
                }

                var members = newMembers[sid];
                foreach (var gid in members)
                {
                    GenomeToSpecies[gid] = sid;
                }

                var memberDict = members.ToDictionary(gid => gid, gid => population[gid]);
                species.Update(population[rid], memberDict);
            }
        }

        private void ReportDistanceStatistics(int populationCount, GenomeDistanceCache distances)
        {
            if (populationCount > 1)
            {
                var meanDistance = distances.Mean(); // Implement this method.
                var stddevDistance = distances.StandardDeviation(); // Implement this method.
                _reporters.ForEach(r => r.Info($"Mean genetic distance {meanDistance:F3}, standard deviation {stddevDistance:F3}"));
            }
        }
    }
}
