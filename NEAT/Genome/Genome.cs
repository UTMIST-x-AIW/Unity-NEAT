using System;
using System.Collections.Generic;
using System.Linq;
using NEAT.Genes;

namespace NEAT.Genome
{
    public class Genome
    {
        public int Key { get; private set; }
        public Dictionary<int, NodeGene> Nodes { get; private set; }
        public Dictionary<int, ConnectionGene> Connections { get; private set; }
        public double? Fitness { get; set; }

        public Genome(int key)
        {
            Key = key;
            Nodes = new Dictionary<int, NodeGene>();
            Connections = new Dictionary<int, ConnectionGene>();
            Fitness = null;
        }

        public void AddNode(NodeGene node)
        {
            Nodes[node.Key] = node;
        }

        public void AddConnection(ConnectionGene connection)
        {
            Connections[connection.Key] = connection;
        }

        public Genome Clone(int newKey)
        {
            var clone = new Genome(newKey);

            foreach (var node in Nodes.Values)
            {
                clone.AddNode((NodeGene)node.Clone());
            }

            foreach (var conn in Connections.Values)
            {
                clone.AddConnection((ConnectionGene)conn.Clone());
            }

            clone.Fitness = Fitness;
            return clone;
        }

        public double CalculateGenomeDistance(Genome other, double disjointCoefficient, double weightCoefficient)
        {
            var nodeGeneSet = new HashSet<int>(Nodes.Keys.Concat(other.Nodes.Keys));
            var connectionGeneSet = new HashSet<int>(Connections.Keys.Concat(other.Connections.Keys));

            double disjointNodes = nodeGeneSet.Count - Math.Min(Nodes.Count, other.Nodes.Count);
            double disjointConnections = connectionGeneSet.Count - Math.Min(Connections.Count, other.Connections.Count);

            // Calculate average weight differences of matching connections
            double weightDiff = 0.0;
            int matchingConnections = 0;

            foreach (var key in Connections.Keys.Intersect(other.Connections.Keys))
            {
                weightDiff += Math.Abs(Connections[key].Weight - other.Connections[key].Weight);
                matchingConnections++;
            }

            double averageWeightDiff = matchingConnections > 0 ? weightDiff / matchingConnections : 0;

            return disjointCoefficient * (disjointNodes + disjointConnections) + weightCoefficient * averageWeightDiff;
        }

        public Genome Crossover(Genome other, int childKey)
        {
            Console.WriteLine($"\nStarting crossover between Genome {Key} (fitness: {Fitness}) and Genome {other.Key} (fitness: {other.Fitness})");
            
            // Determine which parent is more fit
            Genome morefit = (Fitness >= other.Fitness) ? this : other;
            Genome lessfit = (Fitness >= other.Fitness) ? other : this;
            
            Console.WriteLine($"More fit parent: Genome {morefit.Key}, Less fit parent: Genome {lessfit.Key}");

            var child = new Genome(childKey);

            // Handle nodes first
            Console.WriteLine("\nInheriting nodes:");
            Console.WriteLine($"More fit parent nodes: {string.Join(", ", morefit.Nodes.Keys)}");
            
            // Add all nodes from the more fit parent
            foreach (var node in morefit.Nodes.Values)
            {
                child.AddNode((NodeGene)node.Clone());
            }
            Console.WriteLine($"Child inherited nodes: {string.Join(", ", child.Nodes.Keys)}");

            // Handle connections
            Console.WriteLine("\nInheriting connections:");
            Console.WriteLine($"More fit parent connections: {string.Join(", ", morefit.Connections.Keys)}");
            Console.WriteLine($"Less fit parent connections: {string.Join(", ", lessfit.Connections.Keys)}");

            foreach (var conn in morefit.Connections)
            {
                // If both parents have this connection, randomly choose which one to inherit from
                if (lessfit.Connections.ContainsKey(conn.Key))
                {
                    // Randomly choose which parent's connection to inherit
                    bool chooseMoreFit = Random.Shared.NextDouble() < 0.5;
                    var selectedConn = chooseMoreFit ? conn.Value : lessfit.Connections[conn.Key];
                    
                    Console.WriteLine($"Matching connection {conn.Key}: Chose {(chooseMoreFit ? "more" : "less")} fit parent's connection with weight {selectedConn.Weight}");
                    child.AddConnection((ConnectionGene)selectedConn.Clone());
                }
                else
                {
                    // Disjoint or excess gene - inherit from the more fit parent
                    Console.WriteLine($"Disjoint/excess connection {conn.Key}: Inherited from more fit parent with weight {conn.Value.Weight}");
                    child.AddConnection((ConnectionGene)conn.Value.Clone());
                }
            }

            Console.WriteLine($"\nCrossover complete. Child Genome {childKey} created with:");
            Console.WriteLine($"Nodes: {string.Join(", ", child.Nodes.Keys)}");
            Console.WriteLine($"Connections: {string.Join(", ", child.Connections.Keys)}");

            return child;
        }

        public override string ToString()
        {
            return $"Genome(key={Key}, nodes={Nodes.Count}, connections={Connections.Count}, fitness={Fitness})";
        }
    }
}
