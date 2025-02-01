using NEAT.Config;
using NEAT.Genome;
using NEAT.Genes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NEAT.Visualization
{
    public static class SpeciationTests
    {
        public static void RunTests()
        {
            Console.WriteLine("Running Speciation Tests...");

            // Create a simple config
            var config = new Config.Config();
            config.SetParameter("num_inputs", 2);
            config.SetParameter("num_outputs", 1);
            config.SetParameter("population_size", 10);
            config.SetParameter("compatibility_threshold", 3.0); // Increased threshold for less strict speciation
            config.SetParameter("disjoint_coefficient", 1.0);
            config.SetParameter("weight_coefficient", 0.3); // Reduced weight coefficient to be less sensitive to weight differences

            var population = new Population(config);

            // Create three distinct genomes
            var genome1 = CreateSimpleGenome(1, new[] { (0, 2, 0.5), (1, 2, 0.5) });  // Simple direct connections
            var genome2 = CreateSimpleGenome(2, new[] { (0, 3, 0.5), (3, 2, 0.5) });  // Has a hidden node
            var genome3 = CreateSimpleGenome(3, new[] { (0, 2, -2.0), (1, 2, -2.0) }); // Similar to genome1 but very different weights

            Console.WriteLine("\nInitial Genomes:");
            PrintGenomeDetails(genome1, "Genome 1 (Direct connections)");
            PrintGenomeDetails(genome2, "Genome 2 (Has hidden node)");
            PrintGenomeDetails(genome3, "Genome 3 (Different weights)");

            // Inject these genomes into the population
            population.InjectGenomes(new List<Genome.Genome> { genome1, genome2, genome3 });

            // Get species information
            var speciesCount = population.GetSpeciesCount();
            Console.WriteLine($"\nNumber of species formed: {speciesCount}");

            // Test expectations
            bool test1 = speciesCount >= 2 && speciesCount <= 3; // We expect 2-3 species due to structural differences
            Console.WriteLine($"\nTest 1 - Multiple Species Formed: {(test1 ? "PASSED" : "FAILED")}");
            Console.WriteLine($"Expected 2-3 species, got {speciesCount}");

            // Create a genome very similar to genome1
            var genome4 = CreateSimpleGenome(4, new[] { (0, 2, 0.51), (1, 2, 0.49) });
            population.InjectGenomes(new List<Genome.Genome> { genome4 });

            // Get updated species count
            var newSpeciesCount = population.GetSpeciesCount();
            bool test2 = newSpeciesCount == speciesCount; // Similar genome shouldn't create new species
            Console.WriteLine($"\nTest 2 - Similar Genome Grouped: {(test2 ? "PASSED" : "FAILED")}");
            Console.WriteLine($"Expected same number of species after adding similar genome");

            Console.WriteLine("\nSpeciation Test Complete!");
        }

        private static Genome.Genome CreateSimpleGenome(int key, (int input, int output, double weight)[] connections)
        {
            var genome = new Genome.Genome(key);
            
            // Add input nodes
            genome.AddNode(new NodeGene(0, NodeType.Input));
            genome.AddNode(new NodeGene(1, NodeType.Input));
            
            // Add output node
            genome.AddNode(new NodeGene(2, NodeType.Output));
            
            // Add hidden node if needed
            if (connections.Any(c => c.input == 3 || c.output == 3))
            {
                genome.AddNode(new NodeGene(3, NodeType.Hidden));
            }

            // Add connections
            int connKey = 0;
            foreach (var (input, output, weight) in connections)
            {
                genome.AddConnection(new ConnectionGene(connKey++, input, output, weight));
            }

            return genome;
        }

        private static void PrintGenomeDetails(Genome.Genome genome, string label)
        {
            Console.WriteLine($"\n{label}:");
            Console.WriteLine($"Nodes: {string.Join(", ", genome.Nodes.Values.Select(n => $"{n.Key}({n.Type})"))}");
            Console.WriteLine($"Connections: {string.Join(", ", genome.Connections.Values.Select(c => $"{c.InputKey}->{c.OutputKey}({c.Weight:F2})"))}");
        }
    }
} 