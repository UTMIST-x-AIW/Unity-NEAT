// Imports 
using System;
using System.Collections.Generic;
using System.Linq;
using Systems.Threading;

namespace RTNEATOffline.NEAT.Reproduction
{
    // Class: DefaultReproduction
    // The class that handles the reproduction of the NEAT algorithm

    public class DefaultReproduction
    {
        private ReproductionConfigs _config; // The configuration of the reproduction
        private ReporterSet _reporters; // IReporter Interface Type

        private StagnationHandler _stagnation; // Stagnation detection logic 
        
        private int _GenomeIndex; // Global genome innovation number !!!NEED CONFIRMATION!!!

        private Dictionary<int, (int, int)> _parents; //Dictionary containing key of child genome ID and parents ID tuple
        // !!! NEED CONFIRM if tuple is a separate type in Genome!!!

        // Initialize Individual Reproduction Params

        // !!! ParseConfig?? !!!

        //Implement fitness calculation & stagnation 
        


        public Reproduction(
            ReproductionConfigs config, 
            ReporterSet reporters, 
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
        public void CreateNewGenome(
            Type GenomeType,
            GenomeConfig GenomeConfig,
            _GenomeIndex/*self added*/)
        {
            _GenomeIndex ++; // !!! REPLACE WITH ITERATOR !!!
            g = GenomeType(_GenomeIndex);
            g.ConfigureNew(GenomeConfig);
            g.parents[_GenomeIndex] = tuple();

            return g;
        }

        public Dictionary<int, Genome> CreateNew(
            Type GenomeType,
            GenomeConfig genomeConfig,
            int numGenomes)
        {
            var newGenomes = new Dictionary<int, Genome>();
            for (var i = 0; i < numGenomes; i++)
            {
                newGenomes[i] = CreateNewGenome(GenomeType, genomeConfig);
            }

            return newGenomes;
        }

        public Dictionary<int, Genome/*Change this*/> reproduce(DefaultGenome parent1, DefaultGenome parent2, DefaultGenome config)
        {    // A function to calculate the energy / whatever reproduction params is for surviving population
            // !!! NEED TO IMPLEMENT !!!

            // create a new child
            var child = CreateNewGenome(DefaultGenome, config);
            child.ConfigureCrossover(parent1, parent2, config);
            child.Mutate(config);
            _parents[childID] = (parent1.ID, parent2.ID);

            // species mapping?

            return child;
        }

        
       
    }

    public class StagnationHandler
    {
        // !!! IMPLEMENT LATER !!!
    }
}