using System;
using System.Collections.Generic;
using System.Linq;
using RTNEAT_offline.NEAT.Configuration;
using RTNEAT_offline.NEAT.Genome;
using RTNEAT_offline.NEAT.Species;

namespace RTNEAT_offline.NEAT.Stagnation
{
    public class DefaultStagnation
    {
        private readonly Config _config;
        private readonly Dictionary<int, float> _speciesFitness;
        private readonly Dictionary<int, int> _stagnationCount;

        public DefaultStagnation(Config config)
        {
            _config = config;
            _speciesFitness = new Dictionary<int, float>();
            _stagnationCount = new Dictionary<int, int>();
        }

        public List<int> UpdateStagnation(Dictionary<int, List<DefaultGenome>> species)
        {
            var extinctSpecies = new List<int>();

            foreach (var (speciesId, members) in species)
            {
                if (!members.Any()) continue;

                // Get the current species fitness (maximum fitness among its members)
                float currentFitness = members.Max(m => m.Fitness ?? float.MinValue);

                // If we haven't seen this species before, or if fitness has improved
                if (!_speciesFitness.ContainsKey(speciesId) || currentFitness > _speciesFitness[speciesId])
                {
                    _speciesFitness[speciesId] = currentFitness;
                    _stagnationCount[speciesId] = 0;
                }
                else
                {
                    _stagnationCount[speciesId] = _stagnationCount.GetValueOrDefault(speciesId, 0) + 1;
                }

                // Check if species has stagnated
                if (_stagnationCount[speciesId] >= _config.MaxStagnation)
                {
                    extinctSpecies.Add(speciesId);
                }
            }

            return extinctSpecies;
        }
    }
}
