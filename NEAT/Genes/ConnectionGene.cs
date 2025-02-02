using System;

namespace NEAT.Genes
{
    public class ConnectionGene : BaseGene
    {
        public int InputKey { get; private set; }
        public int OutputKey { get; private set; }
        public double Weight { get; set; }

        public ConnectionGene(int key, int inputKey, int outputKey, double weight = 0.0) : base(key)
        {
            InputKey = inputKey;
            OutputKey = outputKey;
            Weight = weight;
        }

        public override BaseGene Clone()
        {
            var clone = new ConnectionGene(Key, InputKey, OutputKey, Weight)
            {
                Enabled = Enabled
            };
            return clone;
        }

        public override double DistanceTo(BaseGene other)
        {
            if (!(other is ConnectionGene otherConnection))
                throw new ArgumentException("Connection distance comparison requires ConnectionGene type");

            // Weight difference is used as distance metric
            return Math.Abs(Weight - otherConnection.Weight);
        }

        public override string ToString()
        {
            return $"ConnectionGene(key={Key}, input={InputKey}, output={OutputKey}, weight={Weight}, enabled={Enabled})";
        }
    }
}
