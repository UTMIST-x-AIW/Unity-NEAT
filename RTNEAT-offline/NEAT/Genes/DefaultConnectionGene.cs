using System;
using System.Collections.Generic;
using RTNEAT_offline.NEAT.Attributes;
using RTNEAT_offline.NEAT.Configuration;

namespace RTNEAT_offline.NEAT.Genes
{
    public class DefaultConnectionGene : BaseGene
    {
        private new static readonly Dictionary<string, GeneAttribute> _geneAttributes = new()
        {
            { "weight", new FloatAttribute("weight", 0.0f) },
            { "enabled", new BoolAttribute("enabled", true) }
        };

        public float Weight
        {
            get => (float)GetAttributeValue("weight");
            set => SetAttributeValue("weight", value);
        }

        public bool Enabled
        {
            get => (bool)GetAttributeValue("enabled");
            set => SetAttributeValue("enabled", value);
        }

        public DefaultConnectionGene(object key) : base(key)
        {
            foreach (var (name, attr) in _geneAttributes)
            {
                SetAttribute(name, attr.Clone());
            }
        }

        public override BaseGene Crossover(BaseGene other)
        {
            if (!(other is DefaultConnectionGene otherGene))
                throw new ArgumentException("Cannot crossover with a different type of gene");

            var child = new DefaultConnectionGene(Key);
            foreach (var (name, attr) in _geneAttributes)
            {
                var childAttr = attr.Clone();
                childAttr.Value = Random.Shared.NextDouble() < 0.5 ? GetAttributeValue(name) : otherGene.GetAttributeValue(name);
                child.SetAttribute(name, childAttr);
            }
            return child;
        }

        public float Distance(DefaultConnectionGene other, Config config)
        {
            float d = Math.Abs(Weight - other.Weight);
            if (Enabled != other.Enabled) d += 1.0f;
            return d * config.CompatibilityWeightCoefficient;
        }
    }
}
