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
            // Calculate disjoint nodes
            var disjointNodes = Nodes.Keys.Count(k => !other.Nodes.ContainsKey(k)) +
                               other.Nodes.Keys.Count(k => !Nodes.ContainsKey(k));

            // Calculate disjoint connections
            var disjointConnections = Connections.Keys.Count(k => !other.Connections.ContainsKey(k)) +
                                    other.Connections.Keys.Count(k => !Connections.ContainsKey(k));

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

        public override string ToString()
        {
            return $"Genome(key={Key}, nodes={Nodes.Count}, connections={Connections.Count}, fitness={Fitness})";
        }
    }
} 