using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RTNEATOffline.Genome
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
        private IEnumerator<int> _nodeIndexer;

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

        // Constructor for configuring the genome
        public DefaultGenomeConfig(Dictionary<string, object> parameters)
        {
            // Initialize function sets
            ActivationDefs = new ActivationFunctionSet();
            AggregationDefs = new AggregationFunctionSet();

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
                new("single_structural_mutation", typeof(bool), "false"),
                new("structural_mutation_surer", typeof(string), "default"),
                new("initial_connection", typeof(string), "unconnected")
            };

            // Get node and connection types
            NodeGeneType = (Type)parameters["node_gene_type"];
            _params.AddRange(NodeGeneType.GetConfigParams());

            ConnectionGeneType = (Type)parameters["connection_gene_type"];
            _params.AddRange(ConnectionGeneType.GetConfigParams());

            // Assign parameters
            foreach (var param in _params)
            {
                var value = param.Interpret(parameters[param.Name]);
                typeof(DefaultGenomeConfig).GetProperty(param.Name)?.SetValue(this, value);
            }

            // Validate attributes
            NodeGeneType.ValidateAttributes(this);
            ConnectionGeneType.ValidateAttributes(this);

            // Setup input and output keys
            int numInputs = (int)parameters["num_inputs"];
            int numOutputs = (int)parameters["num_outputs"];
            InputKeys = Enumerable.Range(0, numInputs).Select(i => -i - 1).ToList();
            OutputKeys = Enumerable.Range(0, numOutputs).ToList();

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

            _nodeIndexer = null;
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
            WritePrettyParams(writer, _params.Where(p => p.Name != "initial_connection"));
        }

        // Get a new unique node key
        public int GetNewNodeKey(Dictionary<int, NodeGene> nodeDict)
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
    }
}
