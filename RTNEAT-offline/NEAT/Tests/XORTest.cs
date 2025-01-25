using System;
using System.Collections.Generic;
using System.Linq;
using RTNEAT_offline.NEAT.Genome;
using RTNEAT_offline.NEAT.Genes;

namespace RTNEAT_offline.NEAT.Tests
{
    public class XORTest
    {
        private static readonly List<(float[] inputs, float expected)> XORDataset = new()
        {
            (new float[] { 0, 0 }, 0),
            (new float[] { 0, 1 }, 1),
            (new float[] { 1, 0 }, 1),
            (new float[] { 1, 1 }, 0)
        };

        public static void RunTest()
        {
            // Configure NEAT parameters
            var config = new DefaultGenomeConfig(new Dictionary<string, object>
            {
                { "num_inputs", 2 },
                { "num_outputs", 1 },
                { "num_hidden", 3 },
                { "feed_forward", true },
                { "compatibility_disjoint_coefficient", 1.0 },
                { "compatibility_weight_coefficient", 0.5 },
                { "conn_add_prob", 0.1 },
                { "conn_delete_prob", 0.05 },
                { "node_add_prob", 0.05 },
                { "node_delete_prob", 0.03 },
                { "weight_mutate_rate", 0.9 },
                { "weight_replace_rate", 0.1 },
                { "weight_mutate_power", 0.5 },
                { "enabled_mutate_rate", 0.05 },
                { "bias_mutate_rate", 0.8 },
                { "initial_connection", "full" }
            });

            // Create initial population
            var population = new List<DefaultGenome>();
            for (int i = 0; i < 200; i++)
            {
                var genome = new DefaultGenome(i);
                DefaultGenome.ConfigureNew(genome, config);
                population.Add(genome);
            }

            // Evolution loop
            int generation = 0;
            float bestFitness = 0;
            DefaultGenome bestGenome = null;

            while (generation < 500 && bestFitness < 3.9f)
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

                // Elitism - keep best 2 performers
                nextGeneration.Add(bestGenome.Clone());
                var secondBest = population.OrderByDescending(g => g.Fitness).Skip(1).First();
                nextGeneration.Add(secondBest.Clone());

                // Create offspring through mutation and crossover
                while (nextGeneration.Count < population.Count)
                {
                    // Tournament selection
                    var parent1 = SelectParent(population);
                    var parent2 = SelectParent(population);

                    // Crossover
                    DefaultGenome child;
                    if (Random.Shared.NextDouble() < 0.8)
                    {
                        child = parent1.Clone();
                        // Perform crossover by copying random connections from parent2
                        foreach (var conn in parent2.Connections)
                        {
                            if (Random.Shared.NextDouble() < 0.5)
                            {
                                if (child.Connections.ContainsKey(conn.Key))
                                {
                                    child.Connections[conn.Key] = (DefaultConnectionGene)conn.Value.Clone();
                                }
                                else
                                {
                                    child.Connections[conn.Key] = (DefaultConnectionGene)conn.Value.Clone();
                                }
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
                foreach (var (inputs, expected) in XORDataset)
                {
                    var output = ActivateNetwork(bestGenome, inputs, config);
                    Console.WriteLine($"Input: {inputs[0]}, {inputs[1]} -> Output: {output[0]:F4} (Expected: {expected})");
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
                nodeValues[node.Key] = (float)Math.Tanh(sum); // Using tanh as activation function
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
            foreach (var (inputs, expected) in XORDataset)
            {
                var output = ActivateNetwork(genome, inputs, config);
                float error = Math.Abs(expected - output[0]);
                if (error > 0.5f)
                {
                    fitness -= 1.0f;
                }
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