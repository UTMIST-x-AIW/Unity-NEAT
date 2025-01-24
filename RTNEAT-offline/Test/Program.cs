using System;
using System.Threading;

namespace RTNEAT_offline.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a simple reporter
            var reporter = new SimpleReporter();
            
            // Test the reporter
            reporter.Info("Starting test simulation...");
            
            for (int gen = 0; gen < 3; gen++)
            {
                reporter.StartGeneration(gen);
                Thread.Sleep(1000); // Simulate some work
                reporter.Info($"Processing generation {gen}...");
            }
            
            reporter.Info("Test complete!");
        }
    }

    // Simple reporter class for testing
    class SimpleReporter
    {
        public void StartGeneration(int generation)
        {
            Console.WriteLine($"\n ****** Running generation {generation} ****** \n");
        }

        public void Info(string message)
        {
            Console.WriteLine(message);
        }
    }
}
