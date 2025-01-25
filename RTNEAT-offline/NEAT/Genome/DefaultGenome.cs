using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RTNEAT_offline.NEAT.Genes;
using RTNEAT_offline.NEAT.Configuration;

namespace RTNEAT_offline.NEAT.Genome
{
    public class DefaultGenome
    {
        private static readonly Random random = new Random();
        
        public int Id { get; private set; }
        public Dictionary<(int, int), DefaultConnectionGene> Connections { get; private set; }
        public Dictionary<int, DefaultNodeGene> Nodes { get; private set; }
        public float? Fitness { get; set; }

        public DefaultGenome(int id)
        {
            Id = id;
            Connections = new Dictionary<(int, int), DefaultConnectionGene>();
            Nodes = new Dictionary<int, DefaultNodeGene>();
            Fitness = null;
        }

        public DefaultGenome Clone()
        {
            var clone = new DefaultGenome(Id);
            foreach (var node in Nodes)
            {
                clone.Nodes[node.Key] = (DefaultNodeGene)node.Value.Clone();
            }
            foreach (var conn in Connections)
            {
                clone.Connections[conn.Key] = (DefaultConnectionGene)conn.Value.Clone();
            }
            clone.Fitness = Fitness;
            return clone;
        }

        public static DefaultGenomeConfig ParseConfig(Dictionary<string, object> paramDict)
        {
            paramDict["node_gene_type"] = typeof(DefaultNodeGene);
            paramDict["connection_gene_type"] = typeof(DefaultConnectionGene);
            return new DefaultGenomeConfig(paramDict);
        }

        public static void WriteConfig(TextWriter writer, DefaultGenomeConfig config)
        {
            config.Save(writer);
        }

        public static void ConfigureNew(DefaultGenome genome, DefaultGenomeConfig config)
        {
            // Create input nodes
            for (int i = 0; i < config.NumInputs; i++)
            {
                var node = new DefaultNodeGene(i);
                node.ConfigureAttributes(null);
                genome.Nodes[i] = node;
            }

            // Create output nodes
            for (int i = 0; i < config.NumOutputs; i++)
            {
                var node = new DefaultNodeGene(i + config.NumInputs);
                node.ConfigureAttributes(null);
                genome.Nodes[i + config.NumInputs] = node;
            }

            // Create hidden nodes if specified
            if (config.NumHidden > 0)
            {
                for (int i = 0; i < config.NumHidden; i++)
                {
                    var node = new DefaultNodeGene(i + config.NumInputs + config.NumOutputs);
                    node.ConfigureAttributes(null);
                    genome.Nodes[i + config.NumInputs + config.NumOutputs] = node;
                }
            }

            // Add connections based on initial connectivity type
            switch (config.InitialConnectivity?.ToLower())
            {
                case "fs_neat":
                    // No initial connections
                    break;
                case "full":
                    // Connect all input nodes to all output nodes
                    for (int i = 0; i < config.NumInputs; i++)
                    {
                        for (int j = 0; j < config.NumOutputs; j++)
                        {
                            var conn = new DefaultConnectionGene((i, j + config.NumInputs));
                            conn.ConfigureAttributes(null);
                            conn.Weight = (float)(random.NextDouble() * 4 - 2); // Random weight between -2 and 2
                            conn.Enabled = true;
                            genome.Connections[(i, j + config.NumInputs)] = conn;
                        }
                    }
                    break;
                case "partial":
                    // Connect a random subset of possible connections
                    var possibleConnections = new List<(int, int)>();
                    for (int fromNode = 0; fromNode < config.NumInputs; fromNode++)
                    {
                        for (int toNode = 0; toNode < config.NumOutputs; toNode++)
                        {
                            possibleConnections.Add((fromNode, toNode + config.NumInputs));
                        }
                    }
                    int numConnections = (int)(possibleConnections.Count * (config.ConnectionFraction ?? 0.5));
                    for (int i = 0; i < numConnections; i++)
                    {
                        int index = random.Next(possibleConnections.Count);
                        var (fromNode, toNode) = possibleConnections[index];
                        possibleConnections.RemoveAt(index);
                        
                        var conn = new DefaultConnectionGene((fromNode, toNode));
                        conn.ConfigureAttributes(null);
                        conn.Weight = (float)(random.NextDouble() * 4 - 2);
                        conn.Enabled = true;
                        genome.Connections[(fromNode, toNode)] = conn;
                    }
                    break;
                default:
                    throw new ArgumentException($"Unknown initial connectivity type: {config.InitialConnectivity}");
            }
        }

        public void Mutate(DefaultGenomeConfig config)
        {
            // Mutate connection genes
            foreach (var conn in Connections.Values)
            {
                if (random.NextDouble() < config.WeightMutateRate)
                {
                    if (random.NextDouble() < config.WeightReplaceRate)
                    {
                        conn.Weight = (float)(random.NextDouble() * 4 - 2);
                    }
                    else
                    {
                        conn.Weight += (float)(random.NextDouble() * 2 - 1) * (float)config.WeightMutatePower;
                    }
                }
                
                if (random.NextDouble() < config.EnabledMutateRate)
                {
                    conn.Enabled = !conn.Enabled;
                }
            }

            // Mutate node genes
            foreach (var node in Nodes.Values)
            {
                if (random.NextDouble() < config.BiasMutateRate)
                {
                    node.MutateAttributes(new Config
                    {
                        CompatibilityWeightCoefficient = (float)config.CompatibilityWeightCoefficient
                    });
                }
            }

            // Add node
            if (random.NextDouble() < config.NodeAddProb)
            {
                AddNode(config);
            }

            // Add connection
            if (random.NextDouble() < config.ConnAddProb)
            {
                AddConnection(config);
            }

            // Delete node
            if (random.NextDouble() < config.NodeDeleteProb)
            {
                DeleteNode();
            }

            // Delete connection
            if (random.NextDouble() < config.ConnDeleteProb)
            {
                DeleteConnection();
            }
        }

        private void AddNode(DefaultGenomeConfig config)
        {
            if (!Connections.Any())
                return;

            // Choose a random connection to split
            var conn = Connections.Values.ElementAt(random.Next(Connections.Count));
            var connKey = conn.Key;
            var (inNode, outNode) = ((ValueTuple<int, int>)connKey);
            var newNodeId = Nodes.Keys.Max() + 1;

            // Create new node
            var newNode = new DefaultNodeGene(newNodeId);
            newNode.ConfigureAttributes(null);
            Nodes[newNodeId] = newNode;

            // Create new connections
            var inToNew = new DefaultConnectionGene((inNode, newNodeId));
            inToNew.ConfigureAttributes(null);
            inToNew.Weight = 1.0f;
            inToNew.Enabled = true;

            var newToOut = new DefaultConnectionGene((newNodeId, outNode));
            newToOut.ConfigureAttributes(null);
            newToOut.Weight = conn.Weight;
            newToOut.Enabled = true;

            // Disable old connection
            conn.Enabled = false;

            // Add new connections
            Connections[(inNode, newNodeId)] = inToNew;
            Connections[(newNodeId, outNode)] = newToOut;
        }

        private void AddConnection(DefaultGenomeConfig config)
        {
            if (config.FeedForward && CreatesCycle())
                return;

            // Get list of possible connections
            var possibleConnections = new List<(int, int)>();
            foreach (var fromNode in Nodes.Keys)
            {
                foreach (var toNode in Nodes.Keys)
                {
                    if (fromNode != toNode && 
                        !Connections.ContainsKey((fromNode, toNode)) &&
                        (!config.FeedForward || toNode > fromNode))
                    {
                        possibleConnections.Add((fromNode, toNode));
                    }
                }
            }

            if (!possibleConnections.Any())
                return;

            // Choose a random possible connection
            var (selectedFrom, selectedTo) = possibleConnections[random.Next(possibleConnections.Count)];
            var newConn = new DefaultConnectionGene((selectedFrom, selectedTo));
            newConn.ConfigureAttributes(null);
            newConn.Weight = (float)(random.NextDouble() * 4 - 2);
            newConn.Enabled = true;

            Connections[(selectedFrom, selectedTo)] = newConn;
        }

        private void DeleteNode()
        {
            if (Nodes.Count <= 2)
                return;

            // Don't delete input or output nodes
            var deletableNodes = Nodes.Keys.Where(k => k >= Nodes.Count - 1).ToList();
            if (!deletableNodes.Any())
                return;

            var nodeToDelete = deletableNodes[random.Next(deletableNodes.Count)];

            // Remove all connections to/from this node
            var connectionsToRemove = Connections.Keys
                .Where(k => k.Item1 == nodeToDelete || k.Item2 == nodeToDelete)
                .ToList();

            foreach (var key in connectionsToRemove)
            {
                Connections.Remove(key);
            }

            Nodes.Remove(nodeToDelete);
        }

        private void DeleteConnection()
        {
            if (!Connections.Any())
                return;

            var connToDelete = Connections.Keys.ElementAt(random.Next(Connections.Count));
            Connections.Remove(connToDelete);
        }

        private bool CreatesCycle()
        {
            var visited = new HashSet<int>();
            var stack = new HashSet<int>();

            bool HasCycle(int node)
            {
                if (stack.Contains(node))
                    return true;
                if (visited.Contains(node))
                    return false;

                visited.Add(node);
                stack.Add(node);

                foreach (var conn in Connections.Where(c => c.Key.Item1 == node))
                {
                    if (HasCycle(conn.Key.Item2))
                        return true;
                }

                stack.Remove(node);
                return false;
            }

            foreach (var node in Nodes.Keys)
            {
                if (!visited.Contains(node) && HasCycle(node))
                    return true;
            }

            return false;
        }

        public float Distance(DefaultGenome other, DefaultGenomeConfig config)
        {
            float disjointDiff = 0;
            float weightDiff = 0;
            int matchingGenes = 0;

            // Calculate connection gene differences
            var allConnKeys = new HashSet<(int, int)>(Connections.Keys.Concat(other.Connections.Keys));
            var allNodeKeys = new HashSet<int>(Nodes.Keys.Concat(other.Nodes.Keys));

            foreach (var key in allConnKeys)
            {
                bool inThis = Connections.ContainsKey(key);
                bool inOther = other.Connections.ContainsKey(key);

                if (inThis && inOther)
                {
                    weightDiff += Math.Abs(Connections[key].Weight - other.Connections[key].Weight);
                    matchingGenes++;
                }
                else
                {
                    disjointDiff++;
                }
            }

            // Calculate node gene differences
            foreach (var key in allNodeKeys)
            {
                if (Nodes.ContainsKey(key) != other.Nodes.ContainsKey(key))
                {
                    disjointDiff++;
                }
            }

            float weightAvg = matchingGenes > 0 ? weightDiff / matchingGenes : 0;
            int numGenes = Math.Max(allConnKeys.Count + allNodeKeys.Count, 1);

            return (float)(config.CompatibilityDisjointCoefficient * disjointDiff / numGenes +
                   config.CompatibilityWeightCoefficient * weightAvg);
        }

        public (int, int) Size()
        {
            var numEnabledConnections = Connections.Count(c => c.Value.Enabled);
            return (Nodes.Count, numEnabledConnections);
        }

        public override string ToString()
        {
            var s = $"Key: {Id}\nFitness: {Fitness}\nNodes:";
            foreach (var (k, ng) in Nodes)
            {
                s += $"\n\t{k} {ng}";
            }
            s += "\nConnections:";
            var connections = Connections.Values.OrderBy(c => c.Key).ToList();
            foreach (var c in connections)
            {
                s += "\n\t" + c.ToString();
            }
            return s;
        }
    }
}



