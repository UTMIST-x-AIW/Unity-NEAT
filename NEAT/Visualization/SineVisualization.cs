using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NEAT.Genes;

namespace NEAT.Visualization
{
    public static class SineVisualization
    {
        private static readonly int NumTestPoints = 40;
        private static readonly (double input, double output)[] TestPoints;

        static SineVisualization()
        {
            TestPoints = new (double input, double output)[NumTestPoints];
            for (int i = 0; i < NumTestPoints; i++)
            {
                double x = -Math.PI + (2 * Math.PI * i / (NumTestPoints - 1));
                TestPoints[i] = (x, Math.Sin(x));
            }
        }

        public static void RunTest()
        {
            Console.WriteLine("Running Sine Function Evolution with Visualization...");
            Console.WriteLine("================================================\n");

            // Create configuration
            var config = new Config.Config();
            config.SetParameter("population_size", 150);
            config.SetParameter("num_inputs", 1);
            config.SetParameter("num_outputs", 1);
            config.SetParameter("compatibility_threshold", 3.0);
            config.SetParameter("weight_mutation_rate", 0.8);
            config.SetParameter("node_add_prob", 0.2);
            config.SetParameter("conn_add_prob", 0.5);
            config.SetParameter("conn_delete_prob", 0.2);
            config.SetParameter("disjoint_coefficient", 2.0);
            config.SetParameter("weight_coefficient", 0.4);
            config.SetParameter("species_target_size", 6);

            // Create population and visualization
            var population = new Population(config);
            var visualization = new SpeciesVisualization();

            // Create visualization directory
            Directory.CreateDirectory("visualizations");

            // Evolution loop
            int generation = 0;
            double bestFitness = double.MinValue;
            int generationsWithoutImprovement = 0;
            const int maxGenerations = 200;
            const int stagnationLimit = 15;
            NEAT.Genome.Genome? lastBestGenome = null;

            while (generation < maxGenerations && generationsWithoutImprovement < stagnationLimit)
            {
                // Record species information
                visualization.RecordGeneration(generation, population.GetSpecies());

                // Visualize species representatives
                var species = population.GetSpecies();
                foreach (var s in species)
                {
                    if (s.Members.Count > 1)
                    {
                        var dotGraph = NetworkVisualizer.GenerateDotGraph(s.Representative);
                        var filename = $"visualizations/sine_species_{s.Key}_gen_{generation}";
                        NetworkVisualizer.SaveDotToFile(dotGraph, $"{filename}.dot");
                        Console.WriteLine($"\nVisualized Species {s.Key}:");
                        Console.WriteLine($"Size: {s.Members.Count} members");
                        Console.WriteLine($"Representative Fitness: {s.Representative.Fitness:F3}");
                        Console.WriteLine($"Structure: {s.Representative.Nodes.Count} nodes, {s.Representative.Connections.Count} connections");
                    }
                }

                // Evolve population
                population.Evolve(EvaluateGenome);

                // Track progress
                var currentBest = population.GetBestGenome();
                if (currentBest != null && (lastBestGenome == null || currentBest.Fitness > lastBestGenome.Fitness))
                {
                    bestFitness = currentBest.Fitness ?? double.MinValue;
                    generationsWithoutImprovement = 0;

                    // Save network visualization
                    var dotGraph = NetworkVisualizer.GenerateDotGraph(currentBest);
                    NetworkVisualizer.SaveDotToFile(dotGraph, $"visualizations/sine_gen_{generation}.dot");
                    lastBestGenome = currentBest;

                    // Print progress
                    Console.WriteLine($"Generation {generation}: New best fitness = {bestFitness:F4}");
                    if (bestFitness > NumTestPoints - 0.5) // Very close to perfect
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

            // Print sine results for best genome
            if (bestGenome != null)
            {
                Console.WriteLine("\nSine Function Results (Sample Points):");
                Console.WriteLine("----------------------------------");
                foreach (var (input, expected) in TestPoints.Take(10)) // Show first 10 points
                {
                    var output = ActivateGenome(bestGenome, new[] { input });
                    Console.WriteLine($"Input: {input:F3} | Expected: {expected:F3} | Output: {output:F3}");
                }
            }

            // Print species history
            visualization.PrintHistory();

            // Print final network visualization
            if (bestGenome != null)
            {
                Console.WriteLine("\nGenerating final network visualization...");
                var dotGraph = NetworkVisualizer.GenerateDotGraph(bestGenome);
                NetworkVisualizer.SaveDotToFile(dotGraph, "visualizations/sine_final.dot");
            }
        }

        private static void EvaluateGenome(NEAT.Genome.Genome genome)
        {
            double fitness = NumTestPoints; // Start with maximum possible score
            
            foreach (var (input, expected) in TestPoints)
            {
                var output = ActivateGenome(genome, new[] { input });
                fitness -= Math.Abs(expected - output); // Subtract absolute error
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

                    // Apply activation function (tanh for better sine approximation)
                    node.Value = Math.Tanh(sum);
                }
            }

            // Return output value
            return genome.Nodes.Values.First(n => n.Type == NodeType.Output).Value;
        }
    }
} 