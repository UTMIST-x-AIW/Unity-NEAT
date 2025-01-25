using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RTNEAT_offline.NEAT.Configuration;
using RTNEAT_offline.NEAT.Genes;
using RTNEAT_offline.NEAT.Activation;
using RTNEAT_offline.NEAT.Aggregation;
using RTNEAT_offline.NEAT.Species;
using RTNEAT_offline.NEAT.Stagnation;
using RTNEAT_offline.NEAT.Reproduction;

namespace RTNEAT_offline.NEAT.Genome
{
    // Configuration class for the Genome
    public class DefaultGenomeConfig : Config
    {
        // Allowed connection types
        public static readonly List<string> AllowedConnectivity = new()
        {
            "unconnected", "fs_neat_nohidden", "fs_neat", "fs_neat_hidden",
            "full_nodirect", "full", "full_direct",
            "partial_nodirect", "partial", "partial_direct"
        };

        private readonly Dictionary<string, ConfigParameter> _params;
        private IEnumerator<int>? _nodeIndexer;

        // Public properties
        public ActivationFunctionSet ActivationDefs { get; private set; }
        public AggregationFunctionSet AggregationDefs { get; private set; }
        public List<int> InputKeys { get; private set; }
        public List<int> OutputKeys { get; private set; }
        public float? ConnectionFraction { get; private set; }
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
        public string InitialConnectivity { get; private set; }
        public float WeightReplaceRate { get; private set; }
        public float WeightMutatePower { get; private set; }
        public float WeightInitMean { get; private set; }
        public float WeightInitStdev { get; private set; }
        public float WeightMinValue { get; private set; }
        public float WeightMaxValue { get; private set; }
        public float BiasInitMean { get; private set; }
        public float BiasInitStdev { get; private set; }
        public float BiasMinValue { get; private set; }
        public float BiasMaxValue { get; private set; }
        public float BiasReplaceRate { get; private set; }
        public float BiasMutatePower { get; private set; }
        public float EnabledMutateRate { get; private set; }

        public DefaultGenomeConfig(Type genomeType, Type nodeGeneType, Type connectionGeneType, Type reproductionType, Type speciesSetType, Type stagnationType, string filename)
            : base(genomeType, reproductionType, speciesSetType, stagnationType, filename)
        {
            NodeGeneType = nodeGeneType;
            ConnectionGeneType = connectionGeneType;
            InitialConnectivity = "fs_neat";

            _params = new Dictionary<string, ConfigParameter>
            {
                { "num_inputs", new ConfigParameter("num_inputs", typeof(int), 2) },
                { "num_outputs", new ConfigParameter("num_outputs", typeof(int), 1) },
                { "num_hidden", new ConfigParameter("num_hidden", typeof(int), 0) },
                { "feed_forward", new ConfigParameter("feed_forward", typeof(bool), true) },
                { "compatibility_disjoint_coefficient", new ConfigParameter("compatibility_disjoint_coefficient", typeof(double), 1.0) },
                { "initial_connectivity", new ConfigParameter("initial_connectivity", typeof(string), "fs_neat") },
                { "weight_replace_rate", new ConfigParameter("weight_replace_rate", typeof(float), 0.1f) },
                { "weight_mutate_power", new ConfigParameter("weight_mutate_power", typeof(float), 0.5f) },
                { "weight_init_mean", new ConfigParameter("weight_init_mean", typeof(float), 0.0f) },
                { "weight_init_stdev", new ConfigParameter("weight_init_stdev", typeof(float), 1.0f) },
                { "weight_min_value", new ConfigParameter("weight_min_value", typeof(float), -30.0f) },
                { "weight_max_value", new ConfigParameter("weight_max_value", typeof(float), 30.0f) },
                { "bias_init_mean", new ConfigParameter("bias_init_mean", typeof(float), 0.0f) },
                { "bias_init_stdev", new ConfigParameter("bias_init_stdev", typeof(float), 1.0f) },
                { "bias_min_value", new ConfigParameter("bias_min_value", typeof(float), -30.0f) },
                { "bias_max_value", new ConfigParameter("bias_max_value", typeof(float), 30.0f) },
                { "bias_replace_rate", new ConfigParameter("bias_replace_rate", typeof(float), 0.1f) },
                { "bias_mutate_power", new ConfigParameter("bias_mutate_power", typeof(float), 0.5f) },
                { "enabled_mutate_rate", new ConfigParameter("enabled_mutate_rate", typeof(float), 0.01f) }
            };

            // Initialize function sets
            ActivationDefs = new ActivationFunctionSet();
            AggregationDefs = new AggregationFunctionSet();

            // Add default activation functions
            ActivationDefs.Add("sigmoid", x => 1.0 / (1.0 + Math.Exp(-x)));
            ActivationDefs.Add("tanh", Math.Tanh);
            ActivationDefs.Add("relu", x => Math.Max(0, x));
            ActivationDefs.Add("leaky_relu", x => x > 0 ? x : 0.01 * x);
            ActivationDefs.Add("step", x => x > 0 ? 1.0 : 0.0);

            // Add default aggregation functions
            AggregationDefs.Add("sum", values => values.Sum());
            AggregationDefs.Add("product", values => values.Aggregate(1.0, (acc, val) => acc * val));
            AggregationDefs.Add("max", values => values.Max());
            AggregationDefs.Add("min", values => values.Min());
            AggregationDefs.Add("mean", values => values.Any() ? values.Average() : 0.0);

            InputKeys = new List<int>();
            OutputKeys = new List<int>();
            InitialConnection = "unconnected";
            StructuralMutationSurer = "default";
        }

        public override void LoadFromParameters(Dictionary<string, string> parameters)
        {
            base.LoadFromParameters(parameters);

            foreach (var param in _params)
            {
                if (parameters.TryGetValue(param.Key, out string value))
                {
                    switch (param.Key)
                    {
                        case "num_inputs":
                            NumInputs = int.Parse(value);
                            break;
                        case "num_outputs":
                            NumOutputs = int.Parse(value);
                            break;
                        case "num_hidden":
                            NumHidden = int.Parse(value);
                            break;
                        case "feed_forward":
                            FeedForward = bool.Parse(value);
                            break;
                        case "compatibility_disjoint_coefficient":
                            CompatibilityDisjointCoefficient = double.Parse(value);
                            break;
                        case "initial_connectivity":
                            InitialConnectivity = value;
                            break;
                        case "weight_replace_rate":
                            WeightReplaceRate = float.Parse(value);
                            break;
                        case "weight_mutate_power":
                            WeightMutatePower = float.Parse(value);
                            break;
                        case "weight_init_mean":
                            WeightInitMean = float.Parse(value);
                            break;
                        case "weight_init_stdev":
                            WeightInitStdev = float.Parse(value);
                            break;
                        case "weight_min_value":
                            WeightMinValue = float.Parse(value);
                            break;
                        case "weight_max_value":
                            WeightMaxValue = float.Parse(value);
                            break;
                        case "bias_init_mean":
                            BiasInitMean = float.Parse(value);
                            break;
                        case "bias_init_stdev":
                            BiasInitStdev = float.Parse(value);
                            break;
                        case "bias_min_value":
                            BiasMinValue = float.Parse(value);
                            break;
                        case "bias_max_value":
                            BiasMaxValue = float.Parse(value);
                            break;
                        case "bias_replace_rate":
                            BiasReplaceRate = float.Parse(value);
                            break;
                        case "bias_mutate_power":
                            BiasMutatePower = float.Parse(value);
                            break;
                        case "enabled_mutate_rate":
                            EnabledMutateRate = float.Parse(value);
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
                ConnectionFraction = float.Parse(parts[1]);
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

        public override List<ConfigParameter> GetConfigParameters()
        {
            var parameters = base.GetConfigParameters();
            parameters.AddRange(_params.Values);
            return parameters;
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

            foreach (var param in _params.Where(p => p.Key != "initial_connection"))
            {
                writer.WriteLine($"{param.Key} = {param.Value.getDefault()}");
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
