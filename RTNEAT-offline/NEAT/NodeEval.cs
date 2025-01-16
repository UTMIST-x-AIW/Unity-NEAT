

public class NodeEval
{
    public int Node { get; }
    public Func<double, double> ActivationFunction { get; }
    public Func<List<double>, double> AggregationFunction { get; }
    public double Bias { get; }
    public double Response { get; }
    public List<(int InputNode, double Weight)> Links { get; }

    public NodeEval(int node, Func<double, double> activationFunction, Func<List<double>, double> aggregationFunction, double bias, double response, List<(int, double)> links)
    {
        Node = node;
        ActivationFunction = activationFunction;
        AggregationFunction = aggregationFunction;
        Bias = bias;
        Response = response;
        Links = links;
    }
}