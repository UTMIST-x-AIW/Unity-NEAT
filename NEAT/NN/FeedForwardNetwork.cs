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
        private readonly HashSet<int> _activating;

        public FeedForwardNetwork(Dictionary<int, NodeGene> nodes, Dictionary<int, ConnectionGene> connections)
        {
            _nodes = nodes;
            _connections = connections;
            _nodeValues = new Dictionary<int, double>();
            _activating = new HashSet<int>();

            // Categorize nodes
            _inputNodes = nodes.Values.Where(n => n.Type == NodeType.Input).Select(n => n.Key).ToList();
            _outputNodes = nodes.Values.Where(n => n.Type == NodeType.Output).Select(n => n.Key).ToList();
            _hiddenNodes = nodes.Values.Where(n => n.Type == NodeType.Hidden).Select(n => n.Key).ToList();
        }

        public double[] Activate(double[] inputs)
        {
            if (inputs.Length != _inputNodes.Count)
            {
                throw new ArgumentException($"Expected {_inputNodes.Count} inputs, got {inputs.Length}");
            }

            // Reset node values and activation tracking
            _nodeValues.Clear();
            _activating.Clear();

            // Set input values
            for (int i = 0; i < inputs.Length; i++)
            {
                _nodeValues[_inputNodes[i]] = inputs[i];
            }

            // Activate hidden nodes
            foreach (var nodeKey in _hiddenNodes)
            {
                ActivateNode(nodeKey);
            }

            // Activate output nodes
            foreach (var nodeKey in _outputNodes)
            {
                ActivateNode(nodeKey);
            }

            // Collect outputs
            return _outputNodes.Select(key => _nodeValues[key]).ToArray();
        }

        private void ActivateNode(int nodeKey)
        {
            // Return if node is already activated or is currently being activated (cycle detection)
            if (_nodeValues.ContainsKey(nodeKey) || _activating.Contains(nodeKey))
                return;

            // Skip activation for input nodes (they should already have values)
            if (_inputNodes.Contains(nodeKey))
                return;

            // Mark node as being activated
            _activating.Add(nodeKey);

            double sum = 0.0;
            foreach (var conn in _connections.Values)
            {
                if (conn.OutputKey == nodeKey && conn.Enabled)
                {
                    // Ensure input node is activated
                    ActivateNode(conn.InputKey);

                    // Skip if input node wasn't activated (could be due to cycle)
                    if (!_nodeValues.ContainsKey(conn.InputKey))
                        continue;

                    sum += conn.Weight * _nodeValues[conn.InputKey];
                }
            }

            _nodeValues[nodeKey] = Sigmoid(sum);
            _activating.Remove(nodeKey);
        }

        private static double Sigmoid(double x)
        {
            return 1.0 / (1.0 + Math.Exp(-x));
        }

        public static FeedForwardNetwork Create(Genome.Genome genome)
        {
            return new FeedForwardNetwork(genome.Nodes, genome.Connections);
        }
    }
} 