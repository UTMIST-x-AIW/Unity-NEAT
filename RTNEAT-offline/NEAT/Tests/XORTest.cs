using System;
using System.Collections.Generic;
using System.Linq;
using RTNEAT_offline.NEAT.Configuration;
using RTNEAT_offline.NEAT.Genome;
using RTNEAT_offline.NEAT.Genes;
using RTNEAT_offline.NEAT.Species;
using RTNEAT_offline.NEAT.Stagnation;
using RTNEAT_offline.NEAT.Reproduction;

namespace RTNEAT_offline.NEAT.Tests
{
    public class XORTest
    {
        private static readonly (float[], float[])[] Dataset = new[]
        {
            (new[] { 0f, 0f }, new[] { 0f }),
            (new[] { 0f, 1f }, new[] { 1f }),
            (new[] { 1f, 0f }, new[] { 1f }),
            (new[] { 1f, 1f }, new[] { 0f })
        };

        public static void RunTest()
        {
            var config = new DefaultGenomeConfig(
                typeof(DefaultGenome),
                typeof(DefaultNodeGene),
                typeof(DefaultConnectionGene),
                typeof(DefaultReproduction),
                typeof(DefaultSpeciesSet),
                typeof(DefaultStagnation),
                "neat-config.txt"
            );

            // Set configuration parameters
            config.LoadFromParameters(new Dictionary<string, string>
            {
                { "num_inputs", "2" },
                { "num_outputs", "1" },
                { "num_hidden", "0" },  // Start with no hidden nodes
                { "feed_forward", "true" },
                { "compatibility_disjoint_coefficient", "1.0" },
                { "compatibility_weight_coefficient", "0.5" },
                { "conn_add_prob", "0.5" },
                { "conn_delete_prob", "0.5" },
                { "node_add_prob", "0.2" },
                { "node_delete_prob", "0.2" },
                { "weight_mutate_rate", "0.8" },
                { "weight_replace_rate", "0.1" },
                { "weight_mutate_power", "0.5" },
                { "enabled_mutate_rate", "0.01" },
                { "bias_mutate_rate", "0.7" },
                { "bias_init_mean", "0.0" },
                { "bias_init_stdev", "1.0" },
                { "bias_min_value", "-30.0" },
                { "bias_max_value", "30.0" },
                { "weight_init_mean", "0.0" },
                { "weight_init_stdev", "1.0" },
                { "weight_min_value", "-30.0" },
                { "weight_max_value", "30.0" },
                { "initial_connection", "full" }
            });

            // Create initial population
            var population = new List<DefaultGenome>();
            for (int i = 0; i < 150; i++)  // Match Python's pop_size
            {
                var genome = new DefaultGenome(config);
                DefaultGenome.ConfigureNew(genome, config);
                population.Add(genome);
            }

            // Evolution loop
            int generation = 0;
            float bestFitness = 0;
            DefaultGenome bestGenome = null;

            while (generation < 300 && bestFitness < 3.9f)  // Match Python's parameters
            {
                // Evaluate fitness for each genome
                foreach (var genome in population)
                {
                    genome.Fitness = EvaluateGenome(genome, config);
                    if (genome.Fitness > bestFitness)
                    {
                        bestFitness = genome.Fitness.Value;
                        bestGenome = genome;
                        Console.WriteLine($"Generation {generation}: New best fitness = {bestFitness}");
                    }
                }

                // Create next generation
                var nextGeneration = new List<DefaultGenome>();

                // Elitism - keep best 2 performers (matching Python's elitism parameter)
                var elites = population.OrderByDescending(g => g.Fitness).Take(2);
                foreach (var elite in elites)
                {
                    nextGeneration.Add(elite.Clone());
                }

                // Create offspring through mutation and crossover
                while (nextGeneration.Count < population.Count)
                {
                    // Tournament selection
                    var parent1 = SelectParent(population);
                    var parent2 = SelectParent(population);

                    // Crossover
                    DefaultGenome child;
                    if (Random.Shared.NextDouble() < 0.8)  // Higher crossover rate
                    {
                        child = parent1.Clone();
                        foreach (var conn in parent2.Connections)
                        {
                            if (Random.Shared.NextDouble() < 0.5)
                            {
                                child.Connections[conn.Key] = (DefaultConnectionGene)conn.Value.Clone();
                            }
                        }
                    }
                    else
                    {
                        child = parent1.Clone();
                    }

                    // Mutation
                    child.Mutate(config);
                    nextGeneration.Add(child);
                }

                population = nextGeneration;
                generation++;

                if (generation % 10 == 0)
                {
                    Console.WriteLine($"Generation {generation}: Best Fitness = {bestFitness}");
                }
            }

            Console.WriteLine($"\nEvolution completed in {generation} generations");
            Console.WriteLine($"Best fitness achieved: {bestFitness}");
            
            if (bestGenome != null)
            {
                Console.WriteLine("\nTesting best genome on XOR:");
                foreach (var (inputs, expected) in Dataset)
                {
                    var output = ActivateNetwork(bestGenome, inputs, config);
                    Console.WriteLine($"Input: {inputs[0]}, {inputs[1]} -> Output: {output[0]:F4} (Expected: {expected[0]})");
                }
            }
        }

        private static float[] ActivateNetwork(DefaultGenome genome, float[] inputs, DefaultGenomeConfig config)
        {
            // Simple feedforward activation
            var nodeValues = new Dictionary<int, float>();
            
            // Set input values
            for (int i = 0; i < inputs.Length; i++)
            {
                nodeValues[-i - 1] = inputs[i];
            }

            // Activate nodes in order (first hidden nodes, then output nodes)
            var orderedNodes = genome.Nodes.OrderBy(n => n.Key).Where(n => n.Key >= 0);
            foreach (var node in orderedNodes)
            {
                float sum = node.Value.Bias;
                foreach (var conn in genome.Connections.Values.Where(c => c.Enabled && ((ValueTuple<int, int>)c.Key).Item2 == node.Key))
                {
                    var fromNode = ((ValueTuple<int, int>)conn.Key).Item1;
                    if (nodeValues.ContainsKey(fromNode))
                    {
                        sum += nodeValues[fromNode] * conn.Weight;
                    }
                }
                nodeValues[node.Key] = 1.0f / (1.0f + (float)Math.Exp(-sum)); // Sigmoid activation
            }

            // Return output values (first N nodes are output nodes)
            return genome.Nodes.Keys.Where(k => k >= 0 && k < config.NumOutputs)
                                  .OrderBy(k => k)
                                  .Select(k => nodeValues.ContainsKey(k) ? nodeValues[k] : 0.0f)
                                  .ToArray();
        }

        private static float EvaluateGenome(DefaultGenome genome, DefaultGenomeConfig config)
        {
            float fitness = 4.0f;
            foreach (var (inputs, expected) in Dataset)
            {
                var output = ActivateNetwork(genome, inputs, config);
                fitness -= (float)Math.Pow(expected[0] - output[0], 2);  // Use squared error like Python
            }
            return fitness;
        }

        private static DefaultGenome SelectParent(List<DefaultGenome> population)
        {
            // Tournament selection
            var tournament = population.OrderBy(x => Random.Shared.Next()).Take(3).ToList();
            return tournament.MaxBy(g => g.Fitness ?? float.MinValue);
        }
    }
} 