using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RTNEATOffline.Genome
{
    public class DefaultGenome
    {
        // Properties matching Python implementation
        public int Key { get; private set; }
        public Dictionary<(int, int), ConnectionGene> Connections { get; private set; }
        public Dictionary<int, NodeGene> Nodes { get; private set; }
        public double? Fitness { get; set; }

        public DefaultGenome(int? key)
        {
            Key = key ?? 0;
            Connections = new Dictionary<(int, int), ConnectionGene>();
            Nodes = new Dictionary<int, NodeGene>();
            Fitness = null;
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


        public void ConfigureNew(DefaultGenomeConfig config)
        {
            // Create node genes for the output pins
            foreach (var nodeKey in config.OutputKeys)
            {
                Nodes[nodeKey] = CreateNode(config, nodeKey);
            }

            // Add hidden nodes if requested
            if (config.NumHidden > 0)
            {
                for (int i = 0; i < config.NumHidden; i++)
                {
                    var nodeKey = config.GetNewNodeKey(Nodes);
                    if (Nodes.ContainsKey(nodeKey))
                    {
                        throw new InvalidOperationException($"Node key {nodeKey} already exists.");
                    }
                    var node = CreateNode(config, nodeKey);
                    Nodes[nodeKey] = node;
                }
            }

            // Add connections based on initial connectivity type
            if (config.InitialConnection.Contains("fs_neat"))
            {
                if (config.InitialConnection == "fs_neat_nohidden")
                {
                    ConnectFsNeatNoHidden(config);
                }
                else if (config.InitialConnection == "fs_neat_hidden")
                {
                    ConnectFsNeatHidden(config);
                }
                else
                {
                    if (config.NumHidden > 0)
                    {
                        Console.Error.WriteLine(
                            "Warning: initial_connection = fs_neat will not connect to hidden nodes;\n" +
                            "\tif this is desired, set initial_connection = fs_neat_nohidden;\n" +
                            "\tif not, set initial_connection = fs_neat_hidden"
                        );
                    }
                    ConnectFsNeatNoHidden(config);
                }
            }
            else if (config.InitialConnection.Contains("full"))
            {
                if (config.InitialConnection == "full_nodirect")
                {
                    ConnectFullNoDirect(config);
                }
                else if (config.InitialConnection == "full_direct")
                {
                    ConnectFullDirect(config);
                }
                else
                {
                    if (config.NumHidden > 0)
                    {
                        Console.Error.WriteLine(
                            "Warning: initial_connection = full with hidden nodes will not do direct input-output connections;\n" +
                            "\tif this is desired, set initial_connection = full_nodirect;\n" +
                            "\tif not, set initial_connection = full_direct"
                        );
                    }
                    ConnectFullNoDirect(config);
                }
            }
            else if (config.InitialConnection.Contains("partial"))
            {
                if (config.InitialConnection == "partial_nodirect")
                {
                    ConnectPartialNoDirect(config);
                }
                else if (config.InitialConnection == "partial_direct")
                {
                    ConnectPartialDirect(config);
                }
                else
                {
                    if (config.NumHidden > 0)
                    {
                        Console.Error.WriteLine(
                            $"Warning: initial_connection = partial with hidden nodes will not do direct input-output connections;\n" +
                            $"\tif this is desired, set initial_connection = partial_nodirect {config.ConnectionFraction};\n" +
                            $"\tif not, set initial_connection = partial_direct {config.ConnectionFraction}"
                        );
                    }
                    ConnectPartialNoDirect(config);
                }
            }
        }




        public void ConfigureCrossover(DefaultGenome genome1, DefaultGenome genome2, DefaultGenomeConfig config)
        {
            var parent1 = genome1.Fitness > genome2.Fitness ? genome1 : genome2;
            var parent2 = genome1.Fitness > genome2.Fitness ? genome2 : genome1;

            // Inherit connection genes
            foreach (var (key, cg1) in parent1.Connections)
            {
                if (parent2.Connections.TryGetValue(key, out var cg2))
                {
                    // Homologous gene: combine genes from both parents
                    Connections[key] = cg1.Crossover(cg2);
                }
                else
                {
                    // Excess or disjoint gene: copy from the fittest parent
                    Connections[key] = cg1.Copy();
                }
            }

            // Inherit node genes
            foreach (var (key, ng1) in parent1.Nodes)
            {
                if (parent2.Nodes.TryGetValue(key, out var ng2))
                {
                    // Homologous gene: combine genes from both parents
                    Nodes[key] = ng1.Crossover(ng2);
                }
                else
                {
                    // Excess or disjoint gene: copy from the fittest parent
                    Nodes[key] = ng1.Copy();
                }
            }
        }

        public void Mutate(DefaultGenomeConfig config)
        {
            if (config.SingleStructuralMutation)
            {
                var div = Math.Max(1, (config.NodeAddProb + config.NodeDeleteProb +
                                     config.ConnAddProb + config.ConnDeleteProb));
                var r = Random.Shared.NextDouble();
                if (r < (config.NodeAddProb / div))
                {
                    MutateAddNode(config);
                }
                else if (r < ((config.NodeAddProb + config.NodeDeleteProb) / div))
                {
                    MutateDeleteNode(config);
                }
                else if (r < ((config.NodeAddProb + config.NodeDeleteProb +
                              config.ConnAddProb) / div))
                {
                    MutateAddConnection(config);
                }
                else if (r < ((config.NodeAddProb + config.NodeDeleteProb +
                              config.ConnAddProb + config.ConnDeleteProb) / div))
                {
                    MutateDeleteConnection();
                }
            }
            else
            {
                if (Random.Shared.NextDouble() < config.NodeAddProb)
                {
                    MutateAddNode(config);
                }
                if (Random.Shared.NextDouble() < config.NodeDeleteProb)
                {
                    MutateDeleteNode(config);
                }
                if (Random.Shared.NextDouble() < config.ConnAddProb)
                {
                    MutateAddConnection(config);
                }
                if (Random.Shared.NextDouble() < config.ConnDeleteProb)
                {
                    MutateDeleteConnection();
                }
            }

            // Mutate connection genes
            foreach (var cg in Connections.Values)
            {
                cg.Mutate(config);
            }

            // Mutate node genes (bias, response, etc.)
            foreach (var ng in Nodes.Values)
            {
                ng.Mutate(config);
            }
        }

        public void MutateAddNode(DefaultGenomeConfig config)
        {
            if (!Connections.Any())
            {
                if (config.CheckStructuralMutationSure())
                {
                    MutateAddConnection(config);
                }
                return;
            }

            // Choose a random connection to split
            var connToSplit = Connections.Values.ElementAt(Random.Shared.Next(Connections.Count));
            var newNodeId = config.GetNewNodeKey(Nodes);
            var ng = CreateNode(config, newNodeId);
            Nodes[newNodeId] = ng;

            // Disable this connection and create two new connections
            connToSplit.Enabled = false;

            var (i, o) = connToSplit.Key;
            AddConnection(config, i, newNodeId, 1.0, true);
            AddConnection(config, newNodeId, o, connToSplit.Weight, true);
        }

        public void AddConnection(DefaultGenomeConfig config, int inputKey, int outputKey, double weight, bool enabled)
        {
            var key = (inputKey, outputKey);
            var connection = CreateConnection(config, inputKey, outputKey);
            connection.InitAttributes(config);
            connection.Weight = weight;
            connection.Enabled = enabled;
            Connections[key] = connection;
        }

        public void MutateAddConnection(DefaultGenomeConfig config)
        {
            var possibleOutputs = Nodes.Keys.ToList();
            var outNode = possibleOutputs[Random.Shared.Next(possibleOutputs.Count)];

            var possibleInputs = possibleOutputs.Concat(config.InputKeys).ToList();
            var inNode = possibleInputs[Random.Shared.Next(possibleInputs.Count)];

            // Don't duplicate connections
            var key = (inNode, outNode);
            if (Connections.ContainsKey(key))
            {
                if (config.CheckStructuralMutationSure())
                {
                    Connections[key].Enabled = true;
                }
                return;
            }

            // Don't allow connections between two output nodes
            if (config.OutputKeys.Contains(inNode) && config.OutputKeys.Contains(outNode))
            {
                return;
            }

            // For feed-forward networks, avoid creating cycles
            if (config.FeedForward && CreatesCycle(Connections.Keys.ToList(), key))
            {
                return;
            }

            var cg = CreateConnection(config, inNode, outNode);
            Connections[cg.Key] = cg;
        }

        public int MutateDeleteNode(DefaultGenomeConfig config)
        {
            // Do nothing if there are no non-output nodes
            var availableNodes = Nodes.Keys.Where(k => !config.OutputKeys.Contains(k)).ToList();
            if (!availableNodes.Any())
            {
                return -1;
            }

            var delKey = availableNodes[Random.Shared.Next(availableNodes.Count)];

            // Remove connections that use this node
            var connectionsToDelete = Connections.Keys
                .Where(k => k.Item1 == delKey || k.Item2 == delKey)
                .ToList();

            foreach (var key in connectionsToDelete)
            {
                Connections.Remove(key);
            }

            Nodes.Remove(delKey);
            return delKey;
        }

        public void MutateDeleteConnection()
        {
            if (Connections.Any())
            {
                var key = Connections.Keys.ElementAt(Random.Shared.Next(Connections.Count));
                Connections.Remove(key);
            }
        }

        public double Distance(DefaultGenome other, DefaultGenomeConfig config)
        {
            // Compute node gene distance component
            double nodeDistance = 0.0;
            if (Nodes.Any() || other.Nodes.Any())
            {
                int disjointNodes = 0;
                foreach (var k2 in other.Nodes.Keys)
                {
                    if (!Nodes.ContainsKey(k2))
                    {
                        disjointNodes++;
                    }
                }

                foreach (var (k1, n1) in Nodes)
                {
                    if (other.Nodes.TryGetValue(k1, out var n2))
                    {
                        nodeDistance += n1.Distance(n2, config);
                    }
                    else
                    {
                        disjointNodes++;
                    }
                }

                var maxNodes = Math.Max(Nodes.Count, other.Nodes.Count);
                nodeDistance = (nodeDistance +
                              (config.CompatibilityDisjointCoefficient *
                               disjointNodes)) / maxNodes;
            }

            // Compute connection gene differences
            double connectionDistance = 0.0;
            if (Connections.Any() || other.Connections.Any())
            {
                int disjointConnections = 0;
                foreach (var k2 in other.Connections.Keys)
                {
                    if (!Connections.ContainsKey(k2))
                    {
                        disjointConnections++;
                    }
                }

                foreach (var (k1, c1) in Connections)
                {
                    if (other.Connections.TryGetValue(k1, out var c2))
                    {
                        connectionDistance += c1.Distance(c2, config);
                    }
                    else
                    {
                        disjointConnections++;
                    }
                }

                var maxConn = Math.Max(Connections.Count, other.Connections.Count);
                connectionDistance = (connectionDistance +
                                    (config.CompatibilityDisjointCoefficient *
                                     disjointConnections)) / maxConn;
            }

            return nodeDistance + connectionDistance;
        }

        public (int, int) Size()
        {
            var numEnabledConnections = Connections.Count(c => c.Value.Enabled);
            return (Nodes.Count, numEnabledConnections);
        }

        public override string ToString()
        {
            var s = $"Key: {Key}\nFitness: {Fitness}\nNodes:";
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

        private static NodeGene CreateNode(DefaultGenomeConfig config, int nodeId)
        {
            var node = new DefaultNodeGene(nodeId);
            node.InitAttributes(config);
            return node;
        }

        private static ConnectionGene CreateConnection(DefaultGenomeConfig config, int inputId, int outputId)
        {
            var connection = new DefaultConnectionGene((inputId, outputId));
            connection.InitAttributes(config);
            return connection;
        }

        private void ConnectFsNeatNoHidden(DefaultGenomeConfig config)
        {
            var inputId = config.InputKeys[Random.Shared.Next(config.InputKeys.Count)];
            foreach (var outputId in config.OutputKeys)
            {
                var connection = CreateConnection(config, inputId, outputId);
                Connections[connection.Key] = connection;
            }
        }

        private void ConnectFsNeatHidden(DefaultGenomeConfig config)
        {
            var inputId = config.InputKeys[Random.Shared.Next(config.InputKeys.Count)];
            foreach (var hiddenId in Nodes.Keys.Where(n => n != config.InputKeys[0]))
            {
                var connection = CreateConnection(config, inputId, hiddenId);
                Connections[connection.Key] = connection;
            }

            foreach (var hiddenId in Nodes.Keys.Where(n => n != config.OutputKeys[0]))
            {
                foreach (var outputId in config.OutputKeys)
                {
                    var connection = CreateConnection(config, hiddenId, outputId);
                    Connections[connection.Key] = connection;
                }
            }
        }

        public List<(int, int)> ComputeFullConnections(DefaultGenomeConfig config, bool direct)
        {
            // Identify hidden and output nodes
            var hidden = Nodes.Keys.Where(i => !config.OutputKeys.Contains(i)).ToList();
            var output = Nodes.Keys.Where(i => config.OutputKeys.Contains(i)).ToList();

            List<(int, int)> connections = new List<(int, int)>();

            if (hidden.Any())
            {
                // Connect each input to all hidden nodes
                foreach (var inputId in config.InputKeys)
                {
                    foreach (var h in hidden)
                    {
                        connections.Add((inputId, h));
                    }
                }

                // Connect each hidden node to all output nodes
                foreach (var h in hidden)
                {
                    foreach (var outputId in output)
                    {
                        connections.Add((h, outputId));
                    }
                }
            }

            // If direct is true or there are no hidden nodes, connect each input to all output nodes
            if (direct || !hidden.Any())
            {
                foreach (var inputId in config.InputKeys)
                {
                    foreach (var outputId in output)
                    {
                        connections.Add((inputId, outputId));
                    }
                }
            }

            // For recurrent genomes, include node self-connections
            if (!config.FeedForward)
            {
                foreach (var i in Nodes.Keys)
                {
                    connections.Add((i, i));
                }
            }

            return connections;
        }



        private void ConnectFullNoDirect(DefaultGenomeConfig config)
        {
            foreach (var inputId in config.InputKeys)
            {
                foreach (var outputId in config.OutputKeys)
                {
                    var connection = CreateConnection(config, inputId, outputId);
                    Connections[connection.Key] = connection;
                }
            }
        }


        public void ConnectFullDirect(DefaultGenomeConfig config)
        {
            // Create a fully-connected genome, including direct input-output connections.
            foreach (var (inputId, outputId) in ComputeFullConnections(config, true))
            {
                var connection = CreateConnection(config, inputId, outputId);
                Connections[connection.Key] = connection;
            }
        }

        public void ConnectPartialNoDirect(DefaultGenomeConfig config)
        {
            // Create a partially-connected genome, with (unless no hidden nodes) no direct input-output connections.
            if (config.ConnectionFraction < 0 || config.ConnectionFraction > 1)
            {
                throw new ArgumentException("Connection fraction must be between 0 and 1");
            }

            var allConnections = ComputeFullConnections(config, false).ToList();
            Shuffle(allConnections);
            int numToAdd = (int)Math.Round(allConnections.Count * config.ConnectionFraction);
            foreach (var (inputId, outputId) in allConnections.Take(numToAdd))
            {
                var connection = CreateConnection(config, inputId, outputId);
                Connections[connection.Key] = connection;
            }
        }

        public void ConnectPartialDirect(DefaultGenomeConfig config)
        {
            // Create a partially-connected genome, including (possibly) direct input-output connections.
            if (config.ConnectionFraction < 0 || config.ConnectionFraction > 1)
            {
                throw new ArgumentException("Connection fraction must be between 0 and 1");
            }

            var allConnections = ComputeFullConnections(config, true).ToList();
            Shuffle(allConnections);
            int numToAdd = (int)Math.Round(allConnections.Count * config.ConnectionFraction);
            foreach (var (inputId, outputId) in allConnections.Take(numToAdd))
            {
                var connection = CreateConnection(config, inputId, outputId);
                Connections[connection.Key] = connection;
            }
        }

        public DefaultGenome GetPrunedCopy(DefaultGenomeConfig genomeConfig)
        {
            // Get a pruned copy of the genome, removing unused node and connection genes.
            var (usedNodeGenes, usedConnectionGenes) = GetPrunedGenes(Nodes, Connections, genomeConfig.InputKeys, genomeConfig.OutputKeys);
            var newGenome = new DefaultGenome(null);
            newGenome.Nodes = usedNodeGenes;
            newGenome.Connections = usedConnectionGenes;
            return newGenome;
        }

        public static (Dictionary<int, NodeGene>, Dictionary<(int, int), ConnectionGene>) GetPrunedGenes(
            Dictionary<int, NodeGene> nodeGenes, 
            Dictionary<(int, int), ConnectionGene> connectionGenes, 
            List<int> inputKeys, 
            List<int> outputKeys)
        {
            var usedNodes = RequiredForOutput(inputKeys, outputKeys, connectionGenes);
            var usedPins = new HashSet<int>(usedNodes);
            usedPins.UnionWith(inputKeys);

            // Copy used nodes into a new genome.
            var usedNodeGenes = new Dictionary<int, NodeGene>();
            foreach (var nodeId in usedNodes)
            {
                usedNodeGenes[nodeId] = nodeGenes[nodeId].Clone();
            }

            // Copy enabled and used connections into the new genome.
            var usedConnectionGenes = new Dictionary<(int, int), ConnectionGene>();
            foreach (var (key, connectionGene) in connectionGenes)
            {
                var (inNodeId, outNodeId) = key;
                if (connectionGene.Enabled && usedPins.Contains(inNodeId) && usedPins.Contains(outNodeId))
                {
                    usedConnectionGenes[key] = connectionGene.Clone();
                }
            }

            return (usedNodeGenes, usedConnectionGenes);
        }



    }
}



