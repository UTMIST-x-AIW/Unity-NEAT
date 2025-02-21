using NEAT;
using NEAT.Config;
using NEAT.NN;
using NEAT.Visualization;

namespace NEAT.Sine;

public class Program
{
    // Generate test points for sine function between -π and π
    private static readonly int NumTestPoints = 100;
    private static (double input, double output)[] TestPoints;
    private static readonly Random Random = new Random();

    public static void Initialize()
    {
        TestPoints = new (double input, double output)[NumTestPoints];
        for (int i = 0; i < NumTestPoints; i++)
        {
            // three sine cycles (6 pi)
            double x = Random.NextDouble() * 5 * Math.PI - 2 * Math.PI;
            TestPoints[i] = (x, Math.Sin(x));
        }
    }

    public static void Main(string[] args)
    {
        Initialize();
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");
        var config = new NEAT.Config.Config();
        config.LoadConfig(configPath);

        // Create a population and run sine evolution
        var pop = new Population(config);
        NEAT.Genome.Genome? winner = null;

        Console.WriteLine("\nStarting Sine function evolution:");

        int generation = 0;
        double bestFitness = double.MinValue;
        int generationsWithoutImprovement = 0;
        const int maxGenerations = 2000;
        const int stagnationLimit = 300;

        while (generation < maxGenerations && generationsWithoutImprovement < stagnationLimit)
        {
            Console.WriteLine($"\nGeneration: {generation}");
            pop.Evolve(EvaluateGenome);
            var best = pop.GetBestGenome();
            Console.WriteLine($"Best fitness: {best.Fitness:F4}");

            if (best.Fitness > bestFitness)
            {
                bestFitness = best.Fitness ?? double.MinValue;
                generationsWithoutImprovement = 0;
                
                // Generate DOT file for improved network
                var dot = NetworkVisualizer.GenerateDotGraph(best);
                var dotPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"sine_gen_{generation}.dot");
                NetworkVisualizer.SaveDotToFile(dot, dotPath);
            }
            else
            {
                generationsWithoutImprovement++;
            }

            if (best.Fitness > 0.95) // Very close to perfect
            {
                winner = best;
                Console.WriteLine("Solution found!");
                break;
            }


            generation++;
            if (generationsWithoutImprovement > stagnationLimit-1){
                Console.WriteLine("Stagnation limit reached, stopping evolution.");
            }
        }

        // Get the best genome (winner or best attempt)
        var bestGenome = winner ?? pop.GetBestGenome();

        // Generate final DOT file
        var finalDot = NetworkVisualizer.GenerateDotGraph(bestGenome);
        var finalDotPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sine_final.dot");
        NetworkVisualizer.SaveDotToFile(finalDot, finalDotPath);

        // Print final results
        Console.WriteLine($"\nEvolution completed after {generation} generations");
        Console.WriteLine($"Best fitness achieved: {bestGenome.Fitness:F4}");
        
        // Test the best network on some sample points
        Console.WriteLine("\nTesting best network on sample points:");
        var network = FeedForwardNetwork.Create(bestGenome);
        for (int i = 0; i < Math.Min(10, TestPoints.Length); i++)
        {
            var (input, expected) = TestPoints[i];
            var output = network.Activate(new[] { input })[0];
            Console.WriteLine($"Input: {input:F3} | Expected: {expected:F3} | Output: {output:F3}");
        }

        Console.WriteLine($"\nNetwork visualization saved to: {finalDotPath}");
        Console.WriteLine("To create an SVG, run: dot -Tsvg sine_final.dot -o sine_final.svg");
    }

    private static void EvaluateGenome(NEAT.Genome.Genome genome)
    {
        var net = FeedForwardNetwork.Create(genome);
        double totalError = 0.0; // total error is 0
        //double fitness = 50.0;
        
        foreach (var (input, expected) in TestPoints)
        {
            var output = net.Activate(new[] { input })[0]; //if the activation function ranges from [0 to 1]
            //fitness += 0.9 - Math.Abs(expected - output);
            totalError += Math.Abs(expected - output); // Subtract absolute error
        }
        // inverted error
        genome.Fitness = (1.0)/(1.0+totalError);
        //genome.Fitness = fitness;
    }
} 