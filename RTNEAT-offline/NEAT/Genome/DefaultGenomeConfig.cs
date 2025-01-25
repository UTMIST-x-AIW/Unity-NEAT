using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RTNEAT_offline.NEAT.Configuration;
using RTNEAT_offline.NEAT.Genes;
using RTNEAT_offline.NEAT.Activation;
using RTNEAT_offline.NEAT.Aggregation;

namespace RTNEAT_offline.NEAT.Genome
{
    // Configuration class for the Genome
    public class DefaultGenomeConfig
    {
        // Allowed connection types
        public static readonly List<string> AllowedConnectivity = new()
        {
            "unconnected", "fs_neat_nohidden", "fs_neat", "fs_neat_hidden",
            "full_nodirect", "full", "full_direct",
            "partial_nodirect", "partial", "partial_direct"
        };

        // Configuration parameters
        private readonly List<ConfigParameter> _params;
        private IEnumerator<int>? _nodeIndexer;

        // Public properties
        public ActivationFunctionSet ActivationDefs { get; private set; }
        public AggregationFunctionSet AggregationDefs { get; private set; }
        public List<int> InputKeys { get; private set; }
        public List<int> OutputKeys { get; private set; }
        public double? ConnectionFraction { get; private set; }
        public string InitialConnection { get; private set; }
        public string StructuralMutationSurer { get; private set; }
        public bool SingleStructuralMutation { get; private set; }
        public Type NodeGeneType { get; private set; }
        public Type ConnectionGeneType { get; private set; }

        // Additional properties needed by DefaultGenome
        public int NumInputs { get; private set; }
        public int NumOutputs { get; private set; }
        public int NumHidden { get; private set; }
        public bool FeedForward { get; private set; }
        public double CompatibilityDisjointCoefficient { get; private set; }
        public double CompatibilityWeightCoefficient { get; private set; }
        public double ConnAddProb { get; private set; }
        public double ConnDeleteProb { get; private set; }
        public double NodeAddProb { get; private set; }
        public double NodeDeleteProb { get; private set; }
        public double WeightMutateRate { get; private set; }
        public double WeightReplaceRate { get; private set; }
        public double WeightMutatePower { get; private set; }
        public double EnabledMutateRate { get; private set; }
        public double BiasMutateRate { get; private set; }
        public string InitialConnectivity => InitialConnection;

        // Constructor for configuring the genome
        public DefaultGenomeConfig(Dictionary<string, object> parameters)
        {
            // Initialize function sets
            ActivationDefs = new ActivationFunctionSet();
            AggregationDefs = new AggregationFunctionSet();
            InputKeys = new List<int>();
            OutputKeys = new List<int>();
            InitialConnection = "unconnected";
            StructuralMutationSurer = "default";
            NodeGeneType = typeof(DefaultNodeGene);
            ConnectionGeneType = typeof(DefaultConnectionGene);

            // Define default parameters
            _params = new List<ConfigParameter>
            {
                new("num_inputs", typeof(int)),
                new("num_outputs", typeof(int)),
                new("num_hidden", typeof(int)),
                new("feed_forward", typeof(bool)),
                new("compatibility_disjoint_coefficient", typeof(double)),
                new("compatibility_weight_coefficient", typeof(double)),
                new("conn_add_prob", typeof(double)),
                new("conn_delete_prob", typeof(double)),
                new("node_add_prob", typeof(double)),
                new("node_delete_prob", typeof(double)),
                new("weight_mutate_rate", typeof(double), 0.8),
                new("weight_replace_rate", typeof(double), 0.1),
                new("weight_mutate_power", typeof(double), 0.5),
                new("enabled_mutate_rate", typeof(double), 0.01),
                new("bias_mutate_rate", typeof(double), 0.7),
                new("single_structural_mutation", typeof(bool), "false"),
                new("structural_mutation_surer", typeof(string), "default"),
                new("initial_connection", typeof(string), "unconnected")
            };

            // Get node and connection types
            if (parameters.ContainsKey("node_gene_type"))
                NodeGeneType = (Type)parameters["node_gene_type"];
            if (parameters.ContainsKey("connection_gene_type"))
                ConnectionGeneType = (Type)parameters["connection_gene_type"];

            // Assign parameters
            foreach (var param in _params)
            {
                if (parameters.ContainsKey(param.getname()))
                {
                    var value = param.Interpret(parameters);
                    switch (param.getname())
                    {
                        case "num_inputs":
                            NumInputs = Convert.ToInt32(value);
                            break;
                        case "num_outputs":
                            NumOutputs = Convert.ToInt32(value);
                            break;
                        case "num_hidden":
                            NumHidden = Convert.ToInt32(value);
                            break;
                        case "feed_forward":
                            FeedForward = Convert.ToBoolean(value);
                            break;
                        case "compatibility_disjoint_coefficient":
                            CompatibilityDisjointCoefficient = Convert.ToDouble(value);
                            break;
                        case "compatibility_weight_coefficient":
                            CompatibilityWeightCoefficient = Convert.ToDouble(value);
                            break;
                        case "conn_add_prob":
                            ConnAddProb = Convert.ToDouble(value);
                            break;
                        case "conn_delete_prob":
                            ConnDeleteProb = Convert.ToDouble(value);
                            break;
                        case "node_add_prob":
                            NodeAddProb = Convert.ToDouble(value);
                            break;
                        case "node_delete_prob":
                            NodeDeleteProb = Convert.ToDouble(value);
                            break;
                        case "weight_mutate_rate":
                            WeightMutateRate = Convert.ToDouble(value);
                            break;
                        case "weight_replace_rate":
                            WeightReplaceRate = Convert.ToDouble(value);
                            break;
                        case "weight_mutate_power":
                            WeightMutatePower = Convert.ToDouble(value);
                            break;
                        case "enabled_mutate_rate":
                            EnabledMutateRate = Convert.ToDouble(value);
                            break;
                        case "bias_mutate_rate":
                            BiasMutateRate = Convert.ToDouble(value);
                            break;
                        case "single_structural_mutation":
                            SingleStructuralMutation = Convert.ToBoolean(value);
                            break;
                        case "structural_mutation_surer":
                            StructuralMutationSurer = value.ToString();
                            break;
                        case "initial_connection":
                            InitialConnection = value.ToString();
                            break;
                    }
                }
            }

            // Setup input and output keys
            if (parameters.ContainsKey("num_inputs") && parameters.ContainsKey("num_outputs"))
            {
                int numInputs = Convert.ToInt32(parameters["num_inputs"]);
                int numOutputs = Convert.ToInt32(parameters["num_outputs"]);
                InputKeys = Enumerable.Range(0, numInputs).Select(i => -i - 1).ToList();
                OutputKeys = Enumerable.Range(0, numOutputs).ToList();
            }

            // Parse initial connection
            ConnectionFraction = null;
            if (InitialConnection.StartsWith("partial"))
            {
                var parts = InitialConnection.Split(' ');
                InitialConnection = parts[0];
                ConnectionFraction = double.Parse(parts[1]);
                if (ConnectionFraction < 0 || ConnectionFraction > 1)
                {
                    throw new Exception("'partial' connection value must be between 0.0 and 1.0, inclusive.");
                }
            }

            if (!AllowedConnectivity.Contains(InitialConnection))
            {
                throw new Exception($"Invalid initial_connection: {InitialConnection}");
            }

            // Normalize structural mutation surer
            StructuralMutationSurer = StructuralMutationSurer.ToLower() switch
            {
                "1" or "yes" or "true" or "on" => "true",
                "0" or "no" or "false" or "off" => "false",
                "default" => "default",
                _ => throw new Exception($"Invalid structural_mutation_surer: {StructuralMutationSurer}")
            };
        }

        // Add an activation function
        public void AddActivation(string name, Func<double, double> func)
        {
            ActivationDefs.Add(name, func);
        }

        // Add an aggregation function
        public void AddAggregation(string name, Func<IEnumerable<double>, double> func)
        {
            AggregationDefs.Add(name, func);
        }

        // Save configuration to a writer
        public void Save(TextWriter writer)
        {
            if (InitialConnection.StartsWith("partial"))
            {
                if (ConnectionFraction < 0 || ConnectionFraction > 1)
                {
                    throw new Exception("'partial' connection value must be between 0.0 and 1.0, inclusive.");
                }
                writer.WriteLine($"initial_connection = {InitialConnection} {ConnectionFraction}");
            }
            else
            {
                writer.WriteLine($"initial_connection = {InitialConnection}");
            }

            foreach (var param in _params.Where(p => p.getname() != "initial_connection"))
            {
                writer.WriteLine($"{param.getname()} = {param.getDefault()}");
            }
        }

        // Get a new unique node key
        public int GetNewNodeKey(Dictionary<int, DefaultNodeGene> nodeDict)
        {
            if (_nodeIndexer == null)
            {
                int start = nodeDict.Any() ? nodeDict.Keys.Max() + 1 : 0;
                _nodeIndexer = InfiniteSequence(start).GetEnumerator();
            }

            _nodeIndexer.MoveNext();
            return _nodeIndexer.Current;
        }

        // Generate an infinite sequence of integers
        private static IEnumerable<int> InfiniteSequence(int start)
        {
            var current = start;
            while (true)
            {
                yield return current++;
            }
        }

        // Check the value of StructuralMutationSurer
        public bool CheckStructuralMutationSurer()
        {
            return StructuralMutationSurer switch
            {
                "true" => true,
                "false" => false,
                "default" => SingleStructuralMutation,
                _ => throw new Exception($"Invalid structural_mutation_surer: {StructuralMutationSurer}")
            };
        }

        // Helper method to check if a connection creates a cycle in a feed-forward network
        public bool CreatesCycle(int inputNode, int outputNode, Dictionary<int, HashSet<int>> connections)
        {
            if (!FeedForward)
                return false;

            if (connections.ContainsKey(outputNode))
            {
                var visited = new HashSet<int>();
                var stack = new Stack<int>();
                stack.Push(outputNode);

                while (stack.Count > 0)
                {
                    var node = stack.Pop();
                    if (node == inputNode)
                        return true;

                    if (!visited.Contains(node) && connections.ContainsKey(node))
                    {
                        visited.Add(node);
                        foreach (var nextNode in connections[node])
                            stack.Push(nextNode);
                    }
                }
            }

            return false;
        }
    }
}
