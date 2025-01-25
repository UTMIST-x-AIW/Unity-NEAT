using System;
using System.Collections.Generic;
using System.Linq;
using RTNEAT_offline.NEAT.Configuration;
using RTNEAT_offline.NEAT.Genome;
using RTNEAT_offline.NEAT.Species;

namespace RTNEAT_offline.NEAT.Reproduction
{
    public class DefaultReproduction
    {
        private readonly DefaultGenomeConfig _config;
        private readonly Random _random;

        public DefaultReproduction(DefaultGenomeConfig config)
        {
            _config = config;
            _random = new Random();
        }

        public List<DefaultGenome> Reproduce(Dictionary<int, List<DefaultGenome>> species, int populationSize)
        {
            var newPopulation = new List<DefaultGenome>();
            var totalAdjustedFitness = 0.0f;
            var speciesAdjustedFitness = new Dictionary<int, float>();

            // Calculate adjusted fitness for each species
            foreach (var (speciesId, members) in species)
            {
                if (!members.Any()) continue;

                var speciesFitness = members.Sum(m => m.Fitness ?? 0.0f);
                var adjustedFitness = speciesFitness / members.Count;
                speciesAdjustedFitness[speciesId] = adjustedFitness;
                totalAdjustedFitness += adjustedFitness;
            }

            // Assign number of offspring to each species
            foreach (var (speciesId, members) in species)
            {
                if (!members.Any()) continue;

                var adjustedFitness = speciesAdjustedFitness[speciesId];
                var speciesOffspring = (int)Math.Round((adjustedFitness / totalAdjustedFitness) * populationSize);

                // Create offspring for this species
                for (int i = 0; i < speciesOffspring; i++)
                {
                    if (_random.NextDouble() < 0.25) // 25% chance of mutation only
                    {
                        // Select parent
                        var parent = SelectParent(members);
                        var child = parent.Clone();
                        child.Mutate(_config);
                        newPopulation.Add(child);
                    }
                    else // 75% chance of crossover
                    {
                        // Select parents
                        var parent1 = SelectParent(members);
                        var parent2 = SelectParent(members);
                        var child = parent1.Crossover(parent2);
                        child.Mutate(_config);
                        newPopulation.Add(child);
                    }
                }
            }

            // If we need more genomes to reach populationSize, clone from the best performing species
            while (newPopulation.Count < populationSize)
            {
                var bestSpeciesId = speciesAdjustedFitness.OrderByDescending(x => x.Value).First().Key;
                var bestMembers = species[bestSpeciesId];
                var parent = SelectParent(bestMembers);
                var child = parent.Clone();
                child.Mutate(_config);
                newPopulation.Add(child);
            }

            return newPopulation;
        }

        private DefaultGenome SelectParent(List<DefaultGenome> members)
        {
            // Tournament selection
            var tournamentSize = Math.Min(3, members.Count);
            var bestGenome = members[_random.Next(members.Count)];
            
            for (int i = 1; i < tournamentSize; i++)
            {
                var competitor = members[_random.Next(members.Count)];
                if ((competitor.Fitness ?? 0) > (bestGenome.Fitness ?? 0))
                {
                    bestGenome = competitor;
                }
            }

            return bestGenome;
        }
    }
} 