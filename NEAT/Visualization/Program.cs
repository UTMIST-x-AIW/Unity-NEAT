using NEAT;
using NEAT.Config;
using NEAT.Genome;
using NEAT.NN;
using NEAT.Visualization;

namespace NEAT.Visualization;

public class Program
{
    public static void Main(string[] args)
    {
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");
        var config = new NEAT.Config.Config();
        config.LoadConfig(configPath);

        // Create a population and run XOR evolution
        var pop = new Population(config);
        NEAT.Genome.Genome? winner = null;

        Console.WriteLine("\nStarting XOR evolution:");

        for (int generation = 0; generation < 1000; generation++)
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

        // Get the best genome (winner or best attempt)
        var bestGenome = winner ?? pop.GetBestGenome();

        // Generate DOT file
        var dot = NetworkVisualizer.GenerateDotGraph(bestGenome);
        var dotPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "network.dot");
        NetworkVisualizer.SaveDotToFile(dot, dotPath);

        Console.WriteLine($"\nNetwork visualization saved to: {dotPath}");
        Console.WriteLine("To create an SVG, run: dot -Tsvg network.dot -o network.svg");
    }

    private static void EvaluateGenome(NEAT.Genome.Genome genome)
    {
        var net = FeedForwardNetwork.Create(genome);
        double fitness = 4.0;  // Max fitness

        // Test all XOR cases
        var inputs = new[]
        {
            new[] { 0.0, 0.0 },
            new[] { 0.0, 1.0 },
            new[] { 1.0, 0.0 },
            new[] { 1.0, 1.0 }
        };
        var expectedOutputs = new[] { 0.0, 1.0, 1.0, 0.0 };

        for (int i = 0; i < inputs.Length; i++)
        {
            var output = Math.Floor(net.Activate(inputs[i])[0] + 0.5);
            double error = Math.Abs(expectedOutputs[i] - output);
            fitness -= error;
        }

        genome.Fitness = fitness;
    }
}
