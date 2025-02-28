using System;
using NEAT.Genes;
using NEAT.Genome;

namespace NEAT.Examples
{
    public class CrossoverDemo
    {
        public static void Run()
        {
            Console.WriteLine("NEAT Crossover Demonstration\n");

            // Create first parent genome
            var parent1 = new Genome.Genome(1);
            parent1.Fitness = 10.0;
            
            // Add nodes to parent1
            parent1.AddNode(new NodeGene(1, NodeType.Input));
            parent1.AddNode(new NodeGene(2, NodeType.Hidden));
            parent1.AddNode(new NodeGene(3, NodeType.Output));
            
            // Add connections to parent1
            var conn1 = new ConnectionGene(1, 1, 2, 0.5);
            var conn2 = new ConnectionGene(2, 2, 3, 0.7);
            var conn3 = new ConnectionGene(3, 1, 3, 0.3);
            conn3.Enabled = false;  // Disable this connection
            
            parent1.AddConnection(conn1);
            parent1.AddConnection(conn2);
            parent1.AddConnection(conn3);

            // Create second parent genome
            var parent2 = new Genome.Genome(2);
            parent2.Fitness = 5.0;
            
            // Add nodes to parent2
            parent2.AddNode(new NodeGene(1, NodeType.Input));
            parent2.AddNode(new NodeGene(2, NodeType.Hidden));
            parent2.AddNode(new NodeGene(3, NodeType.Output));
            parent2.AddNode(new NodeGene(4, NodeType.Hidden));  // Extra hidden node
            
            // Add connections to parent2
            var conn4 = new ConnectionGene(1, 1, 2, -0.5);
            var conn5 = new ConnectionGene(4, 2, 4, 0.9);
            
            parent2.AddConnection(conn4);
            parent2.AddConnection(conn5);

            Console.WriteLine("Parent 1:");
            Console.WriteLine(parent1);
            Console.WriteLine("\nParent 2:");
            Console.WriteLine(parent2);

            Console.WriteLine("\nPerforming crossover...");
            var child = parent1.Crossover(parent2, 3);

            Console.WriteLine("\nFinal child genome:");
            Console.WriteLine(child);

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
} 