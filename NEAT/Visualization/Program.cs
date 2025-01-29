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
        double lastBestFitness = double.MinValue;
        int generationsWithNoImprovement = 0;
        string timestamp = DateTime.Now.ToString("yyyy_MM_dd__h_mm_ss_tt").ToLower();
        
        // Create visualizations directory if it doesn't exist
        var visualizationsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "visualizations");
        Directory.CreateDirectory(visualizationsDir);

        Console.WriteLine("\nStarting XOR evolution:");
        
        int generationsWithoutImprovement = 0;
        const int stagnationThreshold = 15;
        const int targetSpeciesCount = 8;
        double compatibilityThreshold = config.CompatibilityThreshold;
        const double thresholdAdjustmentRate = 0.1;
        int consecutiveAdjustments = 0;
        const int maxConsecutiveAdjustments = 3;

        for (int generation = 0; generation < 500; generation++)
        {
            Console.WriteLine($"\nGeneration: {generation}");
            
            // Adjust speciation threshold dynamically
            int currentSpeciesCount = pop.GetSpeciesCount();
            if (currentSpeciesCount < targetSpeciesCount && consecutiveAdjustments < maxConsecutiveAdjustments)
            {
                compatibilityThreshold *= (1.0 - thresholdAdjustmentRate);
                pop.UpdateCompatibilityThreshold(compatibilityThreshold);
                consecutiveAdjustments++;
                Console.WriteLine($"Decreased compatibility threshold to {compatibilityThreshold:F2}");
            }
            else if (currentSpeciesCount > targetSpeciesCount && consecutiveAdjustments < maxConsecutiveAdjustments)
            {
                compatibilityThreshold *= (1.0 + thresholdAdjustmentRate);
                pop.UpdateCompatibilityThreshold(compatibilityThreshold);
                consecutiveAdjustments++;
                Console.WriteLine($"Increased compatibility threshold to {compatibilityThreshold:F2}");
            }
            else
            {
                consecutiveAdjustments = 0;
            }

            // Evolve the population
            pop.Evolve(EvaluateGenome);
            var best = pop.GetBestGenome();
            Console.WriteLine($"Best fitness: {best.Fitness:F4}");
            Console.WriteLine($"Number of species: {currentSpeciesCount}");

            // Check for improvement
            if (best.Fitness.HasValue && best.Fitness.Value > bestFitnessSoFar)
            {
                if (best.Fitness.Value > lastBestFitness + 0.1) // Significant improvement
                {
                    generationsWithNoImprovement = 0;
                }
                bestFitnessSoFar = best.Fitness.Value;
                generationsWithoutImprovement = 0;
                
                var dot = NetworkVisualizer.GenerateDotGraph(best);
                var dotPath = Path.Combine(visualizationsDir, $"{timestamp}_gen{generation}.dot");
                NetworkVisualizer.SaveDotToFile(dot, dotPath);
            }
            else
            {
                generationsWithoutImprovement++;
                generationsWithNoImprovement++;
                
                // If stuck for too long, try adjusting species parameters
                if (generationsWithNoImprovement >= 10)
                {
                    compatibilityThreshold *= 0.9; // Make speciation more fine-grained
                    pop.UpdateCompatibilityThreshold(compatibilityThreshold);
                    Console.WriteLine($"Adjusting compatibility threshold to {compatibilityThreshold:F2} due to stagnation");
                    generationsWithNoImprovement = 0;
                }

                if (generationsWithoutImprovement >= stagnationThreshold)
                {
                    Console.WriteLine($"\nEvolution stagnated for {stagnationThreshold} generations. Restarting population...");
                    // Save some of the best performers before reset
                    var topPerformers = pop.GetTopGenomes(5);
                    pop = new Population(config);
                    // Inject some of the best performers into new population
                    pop.InjectGenomes(topPerformers);
                    generationsWithoutImprovement = 0;
                    compatibilityThreshold = config.CompatibilityThreshold; // Reset threshold
                }
            }

            lastBestFitness = best.Fitness ?? 0.0;

            if (best.Fitness.HasValue && best.Fitness.Value > 3.9)
            {
                winner = best;
                break;
            }
        }

        // Get the best genome (winner or best attempt)
        var bestGenome = winner ?? pop.GetBestGenome();

        // Generate final DOT file
        var finalDot = NetworkVisualizer.GenerateDotGraph(bestGenome);
        var finalDotPath = Path.Combine(visualizationsDir, $"{timestamp}_final.dot");
        NetworkVisualizer.SaveDotToFile(finalDot, finalDotPath);

        Console.WriteLine($"\nNetwork visualizations saved in: {visualizationsDir}");
    }

    private static void EvaluateGenome(Genome genome)
    {
        var net = FeedForwardNetwork.Create(genome);
        double fitness = 0.0;
        
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
            
            // Base fitness from error (inverted so higher is better)
            double caseFitness = 1.0 - error;
            
            // Bonus for being very close to correct answer
            if (error < 0.05) caseFitness += 0.5;
            
            // Extra bonus for perfect classification (using threshold of 0.5)
            bool correctClassification = (output[0] >= 0.5) == (expectedOutputs[i] >= 0.5);
            if (correctClassification) caseFitness += 0.5;
            
            // Additional bonus for very accurate outputs
            if (error < 0.01) caseFitness += 0.25;
            
            fitness += caseFitness;
        }

        // Scale the final fitness
        fitness = fitness * (4.0 / 8.0);
        
        // Smaller complexity penalty
        int numGenes = genome.Connections.Count + genome.Nodes.Count;
        double complexityPenalty = numGenes * 0.0005; // Reduced from 0.001
        fitness -= complexityPenalty;

        genome.Fitness = fitness;
    }
}
