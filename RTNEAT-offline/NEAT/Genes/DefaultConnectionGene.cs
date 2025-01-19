using RTNEAT_offline.NEAT.Configuration;

namespace RTNEAT_offline.NEAT.Genes
{
    public class DefaultConnectionGene : BaseGene
    {
        private static readonly GeneAttribute[] _geneAttributes = new GeneAttribute[]
        {
            new FloatAttribute("weight"),
            new BoolAttribute("enabled")
        };

        public float Weight { get; set; }
        public bool Enabled { get; set; }

        public DefaultConnectionGene(Tuple<int, int> key) : base(key)
        {
            if (key.GetType() != typeof(Tuple<int, int>))
                throw new ArgumentException($"DefaultConnectionGene key must be a tuple, not {key}");
        }

        public float Distance(DefaultConnectionGene other, Config config)
        {
            float d = Math.Abs(Weight - other.Weight);
            if (Enabled != other.Enabled) d += 1.0f;
            return d * config.CompatibilityWeightCoefficient;
        }
    }
}
