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

        public FeedForwardNetwork(Dictionary<int, NodeGene> nodes, Dictionary<int, ConnectionGene> connections)
        {
            _nodes = nodes;
            _connections = connections;
            _nodeValues = new Dictionary<int, double>();

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

            // Reset node values
            _nodeValues.Clear();

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
            if (_nodeValues.ContainsKey(nodeKey))
                return;

            double sum = 0.0;
            foreach (var conn in _connections.Values)
            {
                if (conn.OutputKey == nodeKey && conn.Enabled)
                {
                    // Ensure input node is activated
                    if (!_nodeValues.ContainsKey(conn.InputKey))
                    {
                        ActivateNode(conn.InputKey);
                    }

                    sum += conn.Weight * _nodeValues[conn.InputKey];
                }
            }

            _nodeValues[nodeKey] = Sigmoid(sum);
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