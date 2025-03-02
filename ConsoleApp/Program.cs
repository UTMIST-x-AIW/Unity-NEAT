using System;
using System.IO;
using System.Linq;
using System.Threading;
using NEAT;
using NEAT.Config;
using NEAT.Genome;

namespace NEAT.Examples
{
    public class Program
    {
        private static float bestFitness = 0f;

        static void Main(string[] args)
        {
            // Increase population size for greater diversity.
            var config = new NEAT.Config.Config
            {
                InputNodes = 1,
                OutputNodes = 1,
                BiasNodes = 1, // Added to ensure a bias node is created.
                PopulationSize = 500,
                MutationRate = 0.3f,          // Consider increasing if needed (e.g. 0.5f)
                WeightMutationRange = 0.5f
            };

            var population = new Population(config);

            // Improved fitness function: square the error to amplify differences.
            Action<Genome.Genome> fitnessFunction = genome =>
            {
                var network = new NeuralNetwork(genome);
                float totalError = 0;
                for (float x = 0; x < 2 * Math.PI; x += 0.1f)
                {
                    var inputs = new float[] { x };
                    var outputs = network.ProcessInputs(inputs);
                    if (outputs.Length == 0)
                    {
                        throw new InvalidOperationException("Network produced no outputs. Check genome configuration.");
                    }
                    float expected = (float)Math.Sin(x);
                    float error = Math.Abs(expected - outputs[0]);
                    totalError += error * error; // square the error
                }
                genome.Fitness = 1f / (1f + totalError);
            };

            Console.WriteLine("Press Ctrl+C to stop");
            Console.WriteLine("Starting evolution...");

            while (true)
            {
                population.Evolve(fitnessFunction);
                LogGenerationStats(population);
                SaveBestNetwork(population);
                Thread.Sleep(100);
            }
        }

        static void LogGenerationStats(Population population)
        {
            // Sort genomes by fitness (if not done inside Evolve)
            var sorted = population.Individuals.OrderByDescending(g => g.Fitness ?? 0).ToList();

            float genBest = (float)(sorted.First().Fitness ?? 0);
            float genAverage = (float)sorted.Average(g => g.Fitness ?? 0);
            float genMin = (float)(sorted.Last().Fitness ?? 0);
            float genVariance = (float)sorted.Average(g => Math.Pow((g.Fitness ?? 0 - genAverage), 2));

            Console.Clear();
            Console.WriteLine($"Best Fitness: {genBest:F4}");
            Console.WriteLine($"Average Fitness: {genAverage:F4}");
            Console.WriteLine($"Worst Fitness: {genMin:F4}");
            Console.WriteLine($"Variance:      {genVariance:F4}");
            Console.WriteLine("Fitness values per genome:");
            foreach (var genome in sorted)
            {
                Console.WriteLine($"{genome.Fitness:F4}");
            }
            Console.Out.Flush();
        }

        static void SaveBestNetwork(Population population)
        {
            // Compute current best fitness (after sorting).
            var currentBestFitness = (float)population.Individuals.Max(g => g.Fitness ?? 0);
            if (currentBestFitness > bestFitness)
            {
                bestFitness = currentBestFitness;
                var bestGenome = population.Individuals.OrderByDescending(g => g.Fitness).First();
                SaveGenome(bestGenome, "best_genome.txt");
                Console.WriteLine($"New best fitness: {bestFitness:F4}. Saved best genome.");
            }
        }

        static void SaveGenome(Genome.Genome genome, string filename)
        {
            // Save the best genome in a file inside the Visualization folder.
            string folder = Path.Combine("Visualization");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using (var writer = new StreamWriter(Path.Combine(folder, filename)))
            {
                writer.WriteLine($"Fitness: {genome.Fitness:F4}");
                writer.WriteLine("Nodes:");
                foreach (var node in genome.Nodes.Values)
                {
                    writer.WriteLine($"{node.Key}: {node.Type}");
                }
                writer.WriteLine("Connections:");
                foreach (var conn in genome.Connections.Values)
                {
                    writer.WriteLine($"{conn.Key}: {conn.InputKey} -> {conn.OutputKey} | Weight: {conn.Weight:F4} | Enabled: {conn.Enabled}");
                }
            }
        }
    }
}
