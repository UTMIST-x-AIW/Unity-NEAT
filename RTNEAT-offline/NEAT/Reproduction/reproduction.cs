// Imports 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RTNEAT_offline.NEAT
{
    // Class: DefaultReproduction
    // The class that handles the reproduction of the NEAT algorithm

    public class Reproduction
    {
        private readonly Config _config;
        private Dictionary<int, (int, int)> _parents;

        public Reproduction(Config config)
        {
            _config = config;
            _parents = new Dictionary<int, (int, int)>();
        }

        public Dictionary<int, Genome> CreateNew(Type genomeType, object genomeConfig, int numGenomes)
        {
            var newGenomes = new Dictionary<int, Genome>();
            for (var i = 0; i < numGenomes; i++)
            {
                newGenomes[i] = new Genome(i) { Fitness = 0.0 };
            }
            return newGenomes;
        }

        public Dictionary<int, Genome> Reproduce(Config config, Dictionary<int, Genome> population, 
            DefaultSpeciesSet species, int generation)
        {
            // For now, just create new random genomes
            return CreateNew(config.GenomeType, config.GenomeConfig, config.PopulationSize);
        }
    }

    public class StagnationHandler
    {
        // !!! IMPLEMENT LATER !!!
    }
}