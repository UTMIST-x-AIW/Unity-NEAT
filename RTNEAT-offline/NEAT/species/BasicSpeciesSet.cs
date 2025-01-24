using System;
using System.Collections.Generic;
using RTNEAT_offline.NEAT.Reporting;

namespace RTNEAT_offline.NEAT
{
    public class DefaultSpeciesSet
    {
        private readonly Config _config;
        public Dictionary<int, Species> Species { get; private set; }
        public Dictionary<int, int> GenomeToSpecies { get; private set; }

        public DefaultSpeciesSet(Config config, List<IReporter> reporters = null)
        {
            _config = config;
            Species = new Dictionary<int, Species>();
            GenomeToSpecies = new Dictionary<int, int>();
        }

        public void Speciate(Config config, Dictionary<int, Genome> population, int generation)
        {
            // For now, put all genomes in one species
            if (!Species.ContainsKey(1))
            {
                Species[1] = new Species(1, generation);
            }

            foreach (var genome in population)
            {
                GenomeToSpecies[genome.Key] = 1;
            }

            // Update the species with all members
            Species[1].Update(population.Values.First(), population.ToDictionary(
                kvp => kvp.Key, 
                kvp => (object)kvp.Value));
        }
    }
} 