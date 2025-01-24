using System;
using System.Collections.Generic;
using RTNEAT_offline.NEAT;
using RTNEAT_offline.NEAT.Reporting;

class Program
{
    static void Main(string[] args)
    {
        // Create config with necessary parameters
        var config = new Config
        {
            PopulationSize = 10,
            FitnessThreshold = 0.9,
            NoFitnessTermination = false,
            ResetOnExtinction = true
        };

        // Create population
        var population = new Population(config);
        
        // Add console reporter
        var consoleReporter = new ConsoleReporter(showSpeciesDetail: true);
        population.AddReporter(consoleReporter);

        // Define a simple fitness function
        bool FitnessFunction(Dictionary<int, Genome> pop, Config cfg)
        {
            foreach (var genome in pop.Values)
            {
                // Simple fitness calculation - just for demonstration
                genome.Fitness = Math.Random.Shared.NextDouble();
            }
            return true; // Return false if extinction occurs
        }

        // Run the evolution
        try
        {
            var best = population.Run(FitnessFunction, maxGenerations: 3);
            Console.WriteLine($"Evolution completed. Best fitness: {best.Fitness}");
        }
        catch (CompleteExtinctionException)
        {
            Console.WriteLine("Evolution failed due to complete extinction");
        }
    }
}