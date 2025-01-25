using System;
using System.IO;
using NEAT;
using NEAT.Config;
using NEAT.NN;
using NEAT.Genome;

namespace XOR;

public class Program
{
    private static readonly double[][] XorInputs = new[]
    {
        new[] { 0.0, 0.0 },
        new[] { 0.0, 1.0 },
        new[] { 1.0, 0.0 },
        new[] { 1.0, 1.0 }
    };

    private static readonly double[][] XorOutputs = new[]
    {
        new[] { 0.0 },
        new[] { 1.0 },
        new[] { 1.0 },
        new[] { 0.0 }
    };

    public static void Main(string[] args)
    {
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");
        var config = new Config();
        config.LoadConfig(configPath);

        var pop = new Population(config);

        Console.WriteLine("\nStarting XOR evolution:");

        Genome? winner = null;
        for (int generation = 0; generation < 300; generation++)
        {
            Console.WriteLine($"\nGeneration: {generation}");
            
            pop.Evolve(EvaluateGenome);
            var best = pop.GetBestGenome();
            
            Console.WriteLine($"Best fitness: {best.Fitness:F4}");

            if (best.Fitness > 3.9)
            {
                winner = best;
                break;
            }
        }

        if (winner != null)
        {
            Console.WriteLine("\nFound a solution!\n");
            Console.WriteLine("Final output:");
            var winnerNet = FeedForwardNetwork.Create(winner);
            
            for (int i = 0; i < XorInputs.Length; i++)
            {
                var output = winnerNet.Activate(XorInputs[i]);
                Console.WriteLine($"input: [{string.Join(", ", XorInputs[i])}], expected: [{string.Join(", ", XorOutputs[i])}], got: [{string.Join(", ", output)}]");
            }
        }
        else
        {
            Console.WriteLine("No solution found");
        }
    }

    private static void EvaluateGenome(Genome genome)
    {
        var net = FeedForwardNetwork.Create(genome);
        double fitness = 4.0;

        for (int i = 0; i < XorInputs.Length; i++)
        {
            var output = net.Activate(XorInputs[i]);
            fitness -= Math.Pow(output[0] - XorOutputs[i][0], 2);
        }

        genome.Fitness = fitness;
    }
} 