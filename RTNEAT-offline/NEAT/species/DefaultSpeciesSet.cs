using System;
using System.Collections.Generic;
using System.Linq;
using RTNEAT_offline.NEAT.Configuration;
using RTNEAT_offline.NEAT.Genome;
using RTNEAT_offline.NEAT.Reporting;

namespace RTNEAT_offline.NEAT.Species
{
    public class DefaultSpeciesSet
    {
        private readonly DefaultGenomeConfig _config;
        private readonly Dictionary<int, List<DefaultGenome>> _species;
        private readonly List<IReporter> reporters;
        private readonly GenomeDistanceCache _distanceCache;
        private int _nextSpeciesId;

        public DefaultSpeciesSet(DefaultGenomeConfig config, List<IReporter> reporters)
        {
            _config = config;
            _species = new Dictionary<int, List<DefaultGenome>>();
            this.reporters = reporters;
            _distanceCache = new GenomeDistanceCache(config);
            _nextSpeciesId = 0;
        }

        public Dictionary<int, List<DefaultGenome>> Speciate(List<DefaultGenome> population)
        {
            reporters.ForEach(r => r.Info("Speciating population"));

            // Reset distance cache for new speciation
            _distanceCache.Reset();

            // Clear existing species
            _species.Clear();

            // Create initial species if none exist
            if (_species.Count == 0 && population.Count > 0)
            {
                var firstSpecies = new List<DefaultGenome> { population[0] };
                _species[_nextSpeciesId++] = firstSpecies;
            }

            // For each genome, find the species it belongs to
            foreach (var genome in population)
            {
                bool foundSpecies = false;
                foreach (var (speciesId, members) in _species)
                {
                    var representative = members[0];
                    var distance = _distanceCache.GetDistance(genome, representative);
                    if (distance < _config.SpeciesDistanceThreshold)
                    {
                        members.Add(genome);
                        foundSpecies = true;
                        break;
                    }
                }

                if (!foundSpecies)
                {
                    var newSpecies = new List<DefaultGenome> { genome };
                    _species[_nextSpeciesId++] = newSpecies;
                }
            }

            // Remove empty species
            var emptySpecies = _species.Where(s => s.Value.Count == 0).Select(s => s.Key).ToList();
            foreach (var speciesId in emptySpecies)
            {
                _species.Remove(speciesId);
            }

            // Report statistics
            var (mean, stdDev) = _distanceCache.GetStatistics();
            reporters.ForEach(r => r.Info($"Mean genetic distance {mean:F3}, standard deviation {stdDev:F3}"));
            reporters.ForEach(r => r.Info($"Species count: {_species.Count}"));

            return _species;
        }

        public Dictionary<int, List<DefaultGenome>> GetSpecies()
        {
            return _species;
        }
    }
} 