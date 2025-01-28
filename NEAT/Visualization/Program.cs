using NEAT;
using NEAT.Config;
using NEAT.Genome;
using NEAT.NN;

namespace Visualization;

public class Program
{
    public static void Main(string[] args)
    {
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");
        var config = new Config();
        config.LoadConfig(configPath);

        // Create a population and run XOR evolution
        var pop = new Population(config);
        Genome? winner = null;
        double bestFitnessSoFar = double.MinValue;
        string timestamp = DateTime.Now.ToString("yyyy_MM_dd__h_mm_ss_tt").ToLower();

        Console.WriteLine("\nStarting XOR evolution:");

        for (int generation = 0; generation < 300; generation++)
        {
            Console.WriteLine($"\nGeneration: {generation}");
            pop.Evolve(EvaluateGenome);
            var best = pop.GetBestGenome();
            Console.WriteLine($"Best fitness: {best.Fitness:F4}");

            // If this generation has a better fitness, save its visualization
            if (best.Fitness.HasValue && best.Fitness.Value > bestFitnessSoFar)
            {
                bestFitnessSoFar = best.Fitness.Value;
                var dot = NetworkVisualizer.GenerateDotGraph(best);
                var visualizationsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "visualizations");
                var dotPath = Path.Combine(visualizationsDir, $"{timestamp}_gen{generation}.dot");
                NetworkVisualizer.SaveDotToFile(dot, dotPath);
            }

            if (best.Fitness.HasValue && best.Fitness.Value > 3.5)
            {
                winner = best;
                break;
            }
        }

        // Get the best genome (winner or best attempt)
        var bestGenome = winner ?? pop.GetBestGenome();

        // Generate final DOT file
        var finalDot = NetworkVisualizer.GenerateDotGraph(bestGenome);
        var finalDotPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "visualizations", $"{timestamp}_final.dot");
        NetworkVisualizer.SaveDotToFile(finalDot, finalDotPath);

        Console.WriteLine($"\nNetwork visualizations saved in: {Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "visualizations")}");
    }

    private static void EvaluateGenome(Genome genome)
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
            var output = net.Activate(inputs[i]);
            double error = Math.Abs(expectedOutputs[i] - output[0]);
            fitness -= error * error;  // Subtract squared error
        }

        genome.Fitness = fitness;
    }
}
