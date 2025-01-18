// Imports 
using System;
using System.Collections.Generic;
using System.Linq;
using Systems.Threading;
namespace RTNEAT-offline.NEAT.Reproduction
{
    // Class: DefaultReproduction
    // The class that handles the reproduction of the NEAT algorithm

    public class DefaultReproduction
    {
        private ReproductionConfigs _config; // The configuration of the reproduction
        private List<IReporter> _reporters; // IReporter Interface Type

        private StagnationHandler _stagnation; // Stagnation detection logic 
        
        private int _genomeIndex; // Global genome innovation number !!!NEED CONFIRMATION!!!

        private Dictionary<int, (int, int)> _parents; //Dictionary containing key of child genome ID and parents ID tuple
        // !!! NEED CONFIRM if tuple is a separate type in Genome!!!

        // Initialize Individual Reproduction Params

        // !!! ParseConfig?? !!!
        public InitializeReproduction(
            ReproductionConfigs config, 
            List<IReporter> reporters, 
            //StagnationHandler stagnation, 
            int genomeIndex,
            Dictionary<int, (int, int)> parents)
        {
            _config = config;
            _reporters = reporters;
            _stagnation = stagnation;
            _genomeIndex = genomeIndex; // MIGHT CHANGE TO 0
            _parents = new Dictionary<int, (int, int)>();
        }

        // Function to create one new (child) genome
        public Dictionary<int, Genome> CreateNewGenome(
            Type genomeType,
            GenomeConfig genomeConfig)
        {
            _genomeIndex ++; // !!! REPLACE WITH ITERATOR !!!
            g = genome_type(_genomeIndex);
            g.ConfigureNew(genomeConfig);
            g.parents[_genomeIndex] = tuple();
        }

        public Dictionary<int, Genome> Reproduce()
        {
            // !!! NEED TO IMPLEMENT !!!
        }

        public crossOver()
        {
            // !!! NEED TO IMPLEMENT !!!
        }

        
       
    }

    public class StagnationHandler
    {
        // !!! IMPLEMENT LATER !!!
    }
}