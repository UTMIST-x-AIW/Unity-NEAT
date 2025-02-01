using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NEAT.Genes;

namespace NEAT.Visualization
{
    public static class XORVisualization
    {
        private static readonly (double[] inputs, double output)[] XORTestCases = new[]
        {
            (new[] { 0.0, 0.0 }, 0.0),
            (new[] { 0.0, 1.0 }, 1.0),
            (new[] { 1.0, 0.0 }, 1.0),
            (new[] { 1.0, 1.0 }, 0.0)
        };

        public static void RunTest()
        {
            Console.WriteLine("Running XOR Evolution with Species Visualization...");
            Console.WriteLine("================================================\n");

            // Create configuration
            var config = new Config.Config();
            config.SetParameter("population_size", 150);
            config.SetParameter("num_inputs", 2);
            config.SetParameter("num_outputs", 1);
            config.SetParameter("compatibility_threshold", 20.0);
            config.SetParameter("weight_mutation_rate", 0.8);
            config.SetParameter("node_add_prob", 0.2);
            config.SetParameter("conn_add_prob", 0.5);
            config.SetParameter("conn_delete_prob", 0.2);
            config.SetParameter("disjoint_coefficient", 0.3);
            config.SetParameter("weight_coefficient", 0.3);
            config.SetParameter("species_target_size", 30);

            // Create population and visualization
            var population = new Population(config);
            var visualization = new SpeciesVisualization();

            // Create visualization directory
            Directory.CreateDirectory("visualizations");

            // Evolution loop
            int generation = 0;
            double bestFitness = 0.0;
            int generationsWithoutImprovement = 0;
            const int maxGenerations = 100;
            const int stagnationLimit = 15;
            NEAT.Genome.Genome? lastBestGenome = null;

            while (generation < maxGenerations && generationsWithoutImprovement < stagnationLimit)
            {
                // Record species information
                visualization.RecordGeneration(generation, population.GetSpecies());

                // Evolve population
                population.Evolve(EvaluateGenome);

                // Track progress
                var currentBest = population.GetBestGenome();
                if (currentBest != null && (lastBestGenome == null || currentBest.Fitness > lastBestGenome.Fitness))
                {
                    bestFitness = currentBest.Fitness ?? 0.0;
                    generationsWithoutImprovement = 0;

                    // Save network visualization
                    var dotGraph = NetworkVisualizer.GenerateDotGraph(currentBest);
                    NetworkVisualizer.SaveDotToFile(dotGraph, $"visualizations/xor_gen_{generation}.dot");
                    lastBestGenome = currentBest;

                    // Print progress
                    Console.WriteLine($"Generation {generation}: New best fitness = {bestFitness:F4}");
                    if (bestFitness >= 3.9) // Close enough to perfect
                    {
                        Console.WriteLine("Solution found!");
                        break;
                    }
                }
                else
                {
                    generationsWithoutImprovement++;
                }

                generation++;
            }

            // Print final results
            var bestGenome = population.GetBestGenome();
            Console.WriteLine($"\nEvolution completed after {generation} generations");
            Console.WriteLine($"Best fitness achieved: {bestGenome?.Fitness:F4}");
            Console.WriteLine($"Best genome structure: {bestGenome?.Nodes.Count} nodes, {bestGenome?.Connections.Count} connections");

            // Print XOR results for best genome
            if (bestGenome != null)
            {
                Console.WriteLine("\nXOR Results:");
                Console.WriteLine("------------");
                foreach (var (inputs, expected) in XORTestCases)
                {
                    var output = ActivateGenome(bestGenome, inputs);
                    Console.WriteLine($"Input: {inputs[0]:F1}, {inputs[1]:F1} | Expected: {expected:F1} | Output: {output:F4}");
                }
            }

            // Print species history
            visualization.PrintHistory();

            // Print final network visualization
            if (bestGenome != null)
            {
                Console.WriteLine("\nGenerating final network visualization...");
                var dotGraph = NetworkVisualizer.GenerateDotGraph(bestGenome);
                NetworkVisualizer.SaveDotToFile(dotGraph, "visualizations/xor_final.dot");
            }
        }

        private static void EvaluateGenome(NEAT.Genome.Genome genome)
        {
            double fitness = 0.0;
            
            foreach (var (inputs, expected) in XORTestCases)
            {
                var output = ActivateGenome(genome, inputs);
                var error = Math.Abs(expected - output);
                fitness += 1.0 - error;
            }

            genome.Fitness = fitness;
        }

        private static double ActivateGenome(NEAT.Genome.Genome genome, double[] inputs)
        {
            // Reset all nodes
            foreach (var node in genome.Nodes.Values)
            {
                node.Value = 0.0;
            }

            // Set input values
            for (int i = 0; i < inputs.Length; i++)
            {
                genome.Nodes[i].Value = inputs[i];
            }

            // Activate the network
            var sortedNodes = genome.Nodes.Values
                .OrderBy(n => n.Type == NodeType.Input ? 0 : n.Type == NodeType.Hidden ? 1 : 2)
                .ThenBy(n => n.Key);

            foreach (var node in sortedNodes)
            {
                if (node.Type != NodeType.Input)
                {
                    // Sum incoming connections
                    double sum = genome.Connections.Values
                        .Where(c => c.OutputKey == node.Key && c.Enabled)
                        .Sum(c => genome.Nodes[c.InputKey].Value * c.Weight);

                    // Apply activation function (sigmoid)
                    node.Value = 1.0 / (1.0 + Math.Exp(-4.9 * sum));
                }
            }

            // Return output value
            return genome.Nodes.Values.First(n => n.Type == NodeType.Output).Value;
        }
    }
} 