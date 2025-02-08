using System;
using System.IO;
using NEAT;
using NEAT.Config;
using NEAT.NN;
using NEAT.Genome;

namespace Sine;

public class Program
{
    // Generate test points for sine function between -π and π
    private static readonly int NumTestPoints = 20;
    private static readonly double[] TestInputs;
    private static readonly double[] TestOutputs;
    private static readonly Random Random = new Random();

    static Program()
    {
        TestInputs = new double[NumTestPoints];
        TestOutputs = new double[NumTestPoints];
        
        for (int i = 0; i < NumTestPoints; i++)
        {
            double x = Random.NextDouble() * 2 * Math.PI - Math.PI;
            TestInputs[i] = x;
            TestOutputs[i] = Math.Sin(x);
        }
    }

    public static void Main(string[] args)
    {
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");
        var config = new Config();
        config.LoadConfig(configPath);

        var pop = new Population(config);

        Console.WriteLine("\nStarting Sine function evolution:");

        Genome? winner = null;
        double bestFitness = double.MinValue;
        int generationsWithoutImprovement = 0;
        const int stagnationLimit = 15;

        for (int generation = 0; generation < 200 && generationsWithoutImprovement < stagnationLimit; generation++)
        {
            Console.WriteLine($"\nGeneration: {generation}");
            
            pop.Evolve(EvaluateGenome);
            var best = pop.GetBestGenome();
            
            Console.WriteLine($"Best fitness: {best.Fitness:F4}");

            if (best.Fitness > bestFitness)
            {
                bestFitness = best.Fitness ?? double.MinValue;
                winner = best;
                generationsWithoutImprovement = 0;
            }
            else
            {
                generationsWithoutImprovement++;
            }

            if (best.Fitness > 18) // Very close to perfect score of 20
            {
                winner = best;
                break;
            }
        }

        if (winner != null)
        {
            Console.WriteLine("\nFound a solution!\n");
            Console.WriteLine("Final output (sample points):");
            var winnerNet = FeedForwardNetwork.Create(winner);
            
            // Test more points for visualization
            const int visualPoints = 50;
            for (int i = 0; i < visualPoints; i++)
            {
                double x = -Math.PI + (2 * Math.PI * i / (visualPoints - 1));
                var input = new[] { x };
                var output = winnerNet.Activate(input);
                Console.WriteLine($"x: {x:F4}, Expected sin(x): {Math.Sin(x):F4}, Got: {output[0]:F4}");
            }
        }
        else
        {
            Console.WriteLine("No satisfactory solution found");
        }
    }

    private static void EvaluateGenome(Genome genome)
    {
        var net = FeedForwardNetwork.Create(genome);
        double fitness = NumTestPoints; // Start with maximum possible fitness

        for (int i = 0; i < NumTestPoints; i++)
        {
            var input = new[] { TestInputs[i] };
            var output = net.Activate(input);
            fitness -= Math.Abs(output[0] - TestOutputs[i]); // Subtract absolute error
        }

        genome.Fitness = fitness;
    }
} 