using RTNEAT_offline.NEAT.Configuration;
using RTNEAT_offline.NEAT.Attributes;

namespace RTNEAT_offline.NEAT.Genes
{
    public class DefaultNodeGene : BaseGene
    {
        private static readonly GeneAttribute[] _geneAttributes = new GeneAttribute[]
        {
            new FloatAttribute("bias", null),
            new FloatAttribute("response", null),
            new StringAttribute("activation", options=''),
            new StringAttribute("aggregation", options='')
        };

        public float Bias { get; set; }
        public float Response { get; set; }
        public string Activation { get; set; }
        public string Aggregation { get; set; }

        public DefaultNodeGene(int key) : base(key)
        {
            if (key.GetType() != typeof(int))
                throw new ArgumentException($"DefaultNodeGene key must be an int, not {key}");
        }

        public float Distance(DefaultNodeGene other, Config config)
        {
            float d = Math.Abs(Bias - other.Bias) + Math.Abs(Response - other.Response);
            if (Activation != other.Activation) d += 1.0f;
            if (Aggregation != other.Aggregation) d += 1.0f;
            return d * config.CompatibilityWeightCoefficient;
        }
    }
}
