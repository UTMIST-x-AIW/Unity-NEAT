using System;
using System.Collections.Generic;
using System.Linq;
using NEAT.Genes;

namespace NEAT.NN
{
    public class FeedForwardNetwork
    {
        private readonly Dictionary<int, NodeGene> _nodes;
        private readonly Dictionary<int, ConnectionGene> _connections;
        private readonly List<int> _inputNodes;
        private readonly List<int> _outputNodes;
        private readonly List<int> _hiddenNodes;
        private readonly Dictionary<int, double> _nodeValues;
        private readonly Dictionary<int, HashSet<int>> _incomingConnections;

        public FeedForwardNetwork(Dictionary<int, NodeGene> nodes, Dictionary<int, ConnectionGene> connections)
        {
            _nodes = nodes;
            _connections = connections;
            _nodeValues = new Dictionary<int, double>();
            _incomingConnections = new Dictionary<int, HashSet<int>>();

            // Initialize incoming connections
            foreach (var node in nodes.Keys)
            {
                _incomingConnections[node] = new HashSet<int>();
            }

            // Build incoming connections map
            foreach (var conn in connections.Values)
            {
                if (conn.Enabled)
                {
                    if (!_nodes.ContainsKey(conn.InputKey) || !_nodes.ContainsKey(conn.OutputKey))
                    {
                        continue;  // Skip invalid connections
                    }
                    _incomingConnections[conn.OutputKey].Add(conn.InputKey);
                }
            }

            // Ensure proper layer assignments
            bool layersChanged;
            do
            {
                layersChanged = false;
                foreach (var conn in connections.Values)
                {
                    if (conn.Enabled)
                    {
                        var sourceNode = _nodes[conn.InputKey];
                        var targetNode = _nodes[conn.OutputKey];

                        // Ensure target is in a higher layer than source
                        if (sourceNode.Layer >= targetNode.Layer)
                        {
                            targetNode.Layer = sourceNode.Layer + 1;
                            layersChanged = true;
                        }
                    }
                }
            } while (layersChanged);  // Repeat until no more changes are needed

            // Sort nodes by layer
            _inputNodes = nodes.Values.Where(n => n.Type == NodeType.Input)
                              .OrderBy(n => n.Key)
                              .Select(n => n.Key)
                              .ToList();

            _hiddenNodes = nodes.Values.Where(n => n.Type == NodeType.Hidden)
                               .OrderBy(n => n.Layer)
                               .ThenBy(n => n.Key)
                               .Select(n => n.Key)
                               .ToList();

            _outputNodes = nodes.Values.Where(n => n.Type == NodeType.Output)
                               .OrderBy(n => n.Key)
                               .Select(n => n.Key)
                               .ToList();
        }

        public double[] Activate(double[] inputs)
        {
            if (inputs.Length != _inputNodes.Count)
            {
                throw new ArgumentException($"Expected {_inputNodes.Count} inputs, got {inputs.Length}");
            }

            // Reset node values
            _nodeValues.Clear();

            // Set input values
            for (int i = 0; i < inputs.Length; i++)
            {
                _nodeValues[_inputNodes[i]] = inputs[i];
            }

            // Process hidden nodes in layer order
            foreach (var nodeKey in _hiddenNodes)
            {
                ActivateNode(nodeKey);
            }

            // Process output nodes
            foreach (var nodeKey in _outputNodes)
            {
                ActivateNode(nodeKey);
            }

            // Collect outputs
            return _outputNodes.Select(key => _nodeValues[key]).ToArray();
        }

        private void ActivateNode(int nodeKey)
        {
            double sum = 0.0;

            // Sum all incoming connections
            foreach (var inputKey in _incomingConnections[nodeKey])
            {
                var conn = _connections.Values.First(c => c.InputKey == inputKey && c.OutputKey == nodeKey && c.Enabled);

                // Input node must already have a value since we're feedforward
                if (!_nodeValues.ContainsKey(inputKey))
                {
                    throw new InvalidOperationException($"Node {inputKey} should have been activated before node {nodeKey}");
                }

                sum += conn.Weight * _nodeValues[inputKey];
            }

            _nodeValues[nodeKey] = tanh(sum);
        }

        private static double Sigmoid(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }

        private static double tanh(double x)
        {
            return Math.Tanh(x);
        }


        public static FeedForwardNetwork Create(Genome.Genome genome)
        {
            return new FeedForwardNetwork(genome.Nodes, genome.Connections);
        }
    }
}
