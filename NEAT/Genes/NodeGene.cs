using System;

namespace NEAT.Genes
{
    public enum NodeType
    {
        Hidden,
        Input,
        Output,
        Bias
    }

    public class NodeGene : BaseGene
    {
        public NodeType Type { get; private set; }
        public string ActivationFunction { get; set; }
        public double Bias { get; set; }
        public double Response { get; set; }
        public double Activation { get; set; }
        public double Aggregation { get; set; }

        public NodeGene(int key, NodeType type) : base(key)
        {
            Type = type;
            ActivationFunction = "sigmoid";
            Bias = 0.0;
            Response = 1.0;
            Activation = 0.0;
            Aggregation = 0.0;
        }

        public override BaseGene Clone()
        {
            var clone = new NodeGene(Key, Type)
            {
                ActivationFunction = ActivationFunction,
                Bias = Bias,
                Response = Response,
                Activation = Activation,
                Aggregation = Aggregation,
                Enabled = Enabled
            };
            return clone;
        }

        public override double DistanceTo(BaseGene other)
        {
            if (!(other is NodeGene otherNode))
                throw new ArgumentException("Node distance comparison requires NodeGene type");

            return base.DistanceTo(other);
        }

        public override string ToString()
        {
            return $"NodeGene(key={Key}, type={Type})";
        }
    }
} 