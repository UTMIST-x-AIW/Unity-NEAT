namespace RTNEAT_offline.NEAT.nn;

public class FeedForwardNetwork
{
    public List<int> inputNodes { get; }
    public List<int> outputNodes { get; }
    public List<NodeEval> nodeEvals { get;  }
    public Dictionary<int, double> values { get; }

    public FeedForwardNetwork(List<int> inputNodes, List<int> outputNodes,
        List<NodeEval> nodeEvals)
    {
        this.inputNodes = inputNodes;
        this.outputNodes = outputNodes;
        this.nodeEvals = nodeEvals;
        values = inputNodes.Concat(outputNodes).ToDictionary(key => key, value => 0.0);
        // Each of the input nodes is a key for each of the output nodes.
    }

    public List<double> Activate(List<double> inputs)
    {
        if (inputNodes.Count != inputs.Count)
        {
            throw new InvalidOperationException
                ($"Expected {inputNodes.Count} inputs, got {inputs.Count}");
        }

        for (int i = 0; i < inputNodes.Count; i++)
        {
            values[inputNodes[i]] = inputs[i];
        }

        foreach (var nodeEval in nodeEvals)
        {
            var nodeInputs = new List<double>();

            foreach (var (inputNode, weight) in nodeEval.Links)
            {
                nodeInputs.Add(values[inputNode] * weight);
            }
            
            // After manipulating the inputs, add them to the same value
            double s = nodeEval.AggregationFunction(nodeInputs);
            
            values[nodeEval.Node] = nodeEval.ActivationFunction(nodeEval.Bias + nodeEval.Response * s);
            // You are giving these values to the aggregation function 
        }

        return outputNodes.Select(node => values[node]).ToList();
    }
}