using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RTNEAT_offline.NEAT.Genes;
using RTNEAT_offline.NEAT.Configuration;
using RTNEAT_offline.NEAT.Activation;
using RTNEAT_offline.NEAT.Aggregation;
using RTNEAT_offline.NEAT.Species;
using RTNEAT_offline.NEAT.Stagnation;
using RTNEAT_offline.NEAT.Reproduction;

namespace RTNEAT_offline.NEAT.Genome
{
    public class DefaultGenome
    {
        private static readonly Random random = new Random();
        
        public int Id { get; private set; }
        public Dictionary<(int, int), DefaultConnectionGene> Connections { get; private set; }
        public Dictionary<int, DefaultNodeGene> Nodes { get; private set; }
        public float? Fitness { get; set; }
        public DefaultGenomeConfig Config { get; private set; }

        public DefaultGenome(DefaultGenomeConfig config)
        {
            Config = config;
            Nodes = new Dictionary<int, DefaultNodeGene>();
            Connections = new Dictionary<(int, int), DefaultConnectionGene>();
            Fitness = null;
        }

        public DefaultGenome(DefaultGenomeConfig config, Dictionary<int, DefaultNodeGene> nodes, Dictionary<(int, int), DefaultConnectionGene> connections)
        {
            Config = config;
            Nodes = nodes;
            Connections = connections;
            Fitness = null;
        }

        public DefaultGenome Clone()
        {
            var clone = new DefaultGenome(Config);
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
            return new DefaultGenomeConfig(
                typeof(DefaultGenome),
                typeof(DefaultNodeGene),
                typeof(DefaultConnectionGene),
                typeof(DefaultReproduction),
                typeof(DefaultSpeciesSet),
                typeof(DefaultStagnation),
                "neat-config.txt"
            );
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
                var node = new DefaultNodeGene(-i - 1);  // Negative IDs for input nodes
                node.ConfigureAttributes(null);
                node.Bias = 0;  // Input nodes have no bias
                genome.Nodes[-i - 1] = node;
            }

            // Create output nodes
            for (int i = 0; i < config.NumOutputs; i++)
            {
                var node = new DefaultNodeGene(i);  // Start at 0 for output nodes
                node.ConfigureAttributes(null);
                // Initialize bias using normal distribution
                node.Bias = (float)(NextGaussian() * config.BiasInitStdev + config.BiasInitMean);
                node.Bias = Math.Clamp(node.Bias, (float)config.BiasMinValue, (float)config.BiasMaxValue);
                genome.Nodes[i] = node;
            }

            // Create hidden nodes if specified
            if (config.NumHidden > 0)
            {
                for (int i = 0; i < config.NumHidden; i++)
                {
                    var node = new DefaultNodeGene(i + config.NumOutputs);  // Continue from output nodes
                    node.ConfigureAttributes(null);
                    // Initialize bias using normal distribution
                    node.Bias = (float)(NextGaussian() * config.BiasInitStdev + config.BiasInitMean);
                    node.Bias = Math.Clamp(node.Bias, (float)config.BiasMinValue, (float)config.BiasMaxValue);
                    genome.Nodes[i + config.NumOutputs] = node;
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
                            var conn = new DefaultConnectionGene((-i - 1, j));  // From input to output
                            conn.ConfigureAttributes(null);
                            // Initialize weight using normal distribution
                            conn.Weight = (float)(NextGaussian() * config.WeightInitStdev + config.WeightInitMean);
                            conn.Weight = Math.Clamp(conn.Weight, (float)config.WeightMinValue, (float)config.WeightMaxValue);
                            conn.Enabled = true;
                            genome.Connections[(-i - 1, j)] = conn;
                        }
                    }
                    break;
                case "partial":
                    // Connect a random subset of possible connections
                    var possibleConnections = new List<(int, int)>();
                    for (int i = 0; i < config.NumInputs; i++)
                    {
                        for (int j = 0; j < config.NumOutputs; j++)
                        {
                            possibleConnections.Add((-i - 1, j));  // From input to output
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
                        // Initialize weight using normal distribution
                        conn.Weight = (float)(NextGaussian() * config.WeightInitStdev + config.WeightInitMean);
                        conn.Weight = Math.Clamp(conn.Weight, (float)config.WeightMinValue, (float)config.WeightMaxValue);
                        conn.Enabled = true;
                        genome.Connections[(fromNode, toNode)] = conn;
                    }
                    break;
                default:
                    throw new ArgumentException($"Unknown initial connectivity type: {config.InitialConnectivity}");
            }
        }

        // Box-Muller transform for generating normally distributed random numbers
        private static double NextGaussian()
        {
            double u1 = 1.0 - random.NextDouble();
            double u2 = 1.0 - random.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
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
                        // Replace with a new random weight using normal distribution
                        conn.Weight = (float)(NextGaussian() * config.WeightInitStdev + config.WeightInitMean);
                }
                    else
                {
                        // Perturb the weight
                        conn.Weight += (float)(NextGaussian() * config.WeightMutatePower);
                    }
                    conn.Weight = Math.Clamp(conn.Weight, (float)config.WeightMinValue, (float)config.WeightMaxValue);
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
                    if (random.NextDouble() < config.BiasReplaceRate)
                    {
                        // Replace with a new random bias using normal distribution
                        node.Bias = (float)(NextGaussian() * config.BiasInitStdev + config.BiasInitMean);
                    }
                    else
                    {
                        // Perturb the bias
                        node.Bias += (float)(NextGaussian() * config.BiasMutatePower);
                    }
                    node.Bias = Math.Clamp(node.Bias, (float)config.BiasMinValue, (float)config.BiasMaxValue);
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

            // Choose a random enabled connection to split
            var enabledConns = Connections.Values.Where(c => c.Enabled).ToList();
            if (!enabledConns.Any())
                return;

            var conn = enabledConns[random.Next(enabledConns.Count)];
            var connKey = conn.Key;
            var (fromNode, toNode) = ((ValueTuple<int, int>)connKey);

            // Disable the old connection
            conn.Enabled = false;

            // Add the new node
            int newNodeId = Nodes.Keys.Max() + 1;
            var newNode = new DefaultNodeGene(newNodeId);
            newNode.ConfigureAttributes(null);
            newNode.Bias = (float)(random.NextDouble() * 2 - 1);  // Random bias between -1 and 1
            Nodes[newNodeId] = newNode;

            // Add two new connections
            var conn1 = new DefaultConnectionGene((fromNode, newNodeId));
            conn1.ConfigureAttributes(null);
            conn1.Weight = 1.0f;  // Keep the path strong
            conn1.Enabled = true;
            Connections[(fromNode, newNodeId)] = conn1;

            var conn2 = new DefaultConnectionGene((newNodeId, toNode));
            conn2.ConfigureAttributes(null);
            conn2.Weight = conn.Weight;  // Preserve the old connection's weight
            conn2.Enabled = true;
            Connections[(newNodeId, toNode)] = conn2;
        }

        private void AddConnection(DefaultGenomeConfig config)
        {
            // Get all possible connections that don't exist and won't create cycles
            var possibleConnections = new List<(int, int)>();
            foreach (var fromNode in Nodes.Keys)
            {
                foreach (var toNode in Nodes.Keys)
                {
                    // Skip if connection already exists
                    if (Connections.ContainsKey((fromNode, toNode)))
                        continue;

                    // Skip if it would connect input to input or output to output
                    if ((fromNode < 0 && toNode < 0) || (fromNode >= 0 && toNode >= 0 && fromNode < config.NumOutputs && toNode < config.NumOutputs))
                        continue;

                    // Skip if it would create a cycle
                    if (!CreatesCycle(fromNode, toNode))
                    {
                        possibleConnections.Add((fromNode, toNode));
                    }
                }
            }

            if (possibleConnections.Count > 0)
            {
                var (fromNode, toNode) = possibleConnections[random.Next(possibleConnections.Count)];
                var newConn = new DefaultConnectionGene((fromNode, toNode));
                newConn.ConfigureAttributes(null);
                newConn.Weight = (float)(random.NextDouble() * 2 - 1);  // Random weight between -1 and 1
                newConn.Enabled = true;
                Connections[(fromNode, toNode)] = newConn;
            }
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

        private bool CreatesCycle(int fromNode, int toNode)
        {
            // Add the potential new connection temporarily
            var tempConnections = new Dictionary<(int, int), DefaultConnectionGene>(Connections);
            var tempConn = new DefaultConnectionGene((fromNode, toNode));
            tempConn.Enabled = true;
            tempConnections[(fromNode, toNode)] = tempConn;

            // Check for cycles using depth-first search
            var visited = new HashSet<int>();
            var recursionStack = new HashSet<int>();

            bool HasCycle(int node)
            {
                if (!visited.Contains(node))
                {
                    visited.Add(node);
                    recursionStack.Add(node);

                    foreach (var conn in tempConnections.Values.Where(c => c.Enabled && ((ValueTuple<int, int>)c.Key).Item1 == node))
                    {
                        var nextNode = ((ValueTuple<int, int>)conn.Key).Item2;
                        if (!visited.Contains(nextNode) && HasCycle(nextNode))
                            return true;
                        else if (recursionStack.Contains(nextNode))
                            return true;
                    }
                }
                recursionStack.Remove(node);
                return false;
            }

            // Start DFS from all input nodes
            foreach (var node in Nodes.Keys.Where(k => k < 0))
            {
                if (HasCycle(node))
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

        public DefaultGenome Crossover(DefaultGenome other)
        {
            // Create a new genome with the same configuration
            var child = new DefaultGenome(Config);

            // Inherit connection genes
            foreach (var (key, gene1) in Connections)
            {
                if (other.Connections.TryGetValue(key, out var gene2))
                {
                    // Inherit randomly from either parent
                    var selectedGene = random.NextDouble() < 0.5 ? gene1 : gene2;
                    child.Connections[key] = (DefaultConnectionGene)selectedGene.Clone();
                }
                else
                {
                    // Inherit disjoint/excess gene from more fit parent
                    if (Fitness > other.Fitness)
                    {
                        child.Connections[key] = (DefaultConnectionGene)gene1.Clone();
                    }
                }
            }

            // Inherit node genes
            foreach (var (key, gene1) in Nodes)
            {
                if (other.Nodes.TryGetValue(key, out var gene2))
                {
                    // Inherit randomly from either parent
                    var selectedGene = random.NextDouble() < 0.5 ? gene1 : gene2;
                    child.Nodes[key] = (DefaultNodeGene)selectedGene.Clone();
                }
                else
                {
                    // Inherit disjoint/excess gene from more fit parent
                    if (Fitness > other.Fitness)
                    {
                        child.Nodes[key] = (DefaultNodeGene)gene1.Clone();
                    }
                }
            }

            return child;
        }
    }
}



