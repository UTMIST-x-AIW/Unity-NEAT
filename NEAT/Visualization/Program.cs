using System;

namespace NEAT.Visualization
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please specify a test to run:");
                Console.WriteLine("  --test-speciation : Run speciation tests");
                Console.WriteLine("  --test-xor        : Run XOR evolution with species visualization");
                return;
            }

            switch (args[0].ToLower())
            {
                case "--test-speciation":
                    SpeciationTests.RunTests();
                    break;
                case "--test-xor":
                    XORVisualization.RunTest();
                    break;
                default:
                    Console.WriteLine($"Unknown test: {args[0]}");
                    break;
            }
        }
    }
}
