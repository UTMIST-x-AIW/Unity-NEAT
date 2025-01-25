using System;
using System.Collections.Generic;
using RTNEAT_offline.NEAT.Attributes;
using RTNEAT_offline.NEAT.Configuration;

namespace RTNEAT_offline.NEAT.Genes
{
    public class DefaultNodeGene : BaseGene
    {
        private new static readonly Dictionary<string, GeneAttribute> _geneAttributes = new()
        {
            { "bias", new FloatAttribute("bias", 0.0f) },
            { "response", new FloatAttribute("response", 1.0f) },
            { "activation", new StringAttribute("activation", "sigmoid") },
            { "aggregation", new StringAttribute("aggregation", "sum") }
        };

        public float Bias
        {
            get => (float)GetAttributeValue("bias");
            set => SetAttributeValue("bias", value);
        }

        public float Response
        {
            get => (float)GetAttributeValue("response");
            set => SetAttributeValue("response", value);
        }

        public string Activation
        {
            get => (string)GetAttributeValue("activation");
            set => SetAttributeValue("activation", value);
        }

        public string Aggregation
        {
            get => (string)GetAttributeValue("aggregation");
            set => SetAttributeValue("aggregation", value);
        }

        public NodeType Type { get; private set; }

        public DefaultNodeGene(object key) : base(key)
        {
            foreach (var (name, attr) in _geneAttributes)
            {
                SetAttribute(name, attr.Clone());
            }

            if (key.GetType() != typeof(int))
                throw new ArgumentException($"DefaultNodeGene key must be an int, not {key}");
            
            // Determine node type based on key
            if (Convert.ToInt32(key) < 0)
                Type = NodeType.Input;
            else if (Convert.ToInt32(key) == 0)
                Type = NodeType.Bias;
            else
                Type = NodeType.Hidden;
        }

        public float Distance(DefaultNodeGene other, Config config)
        {
            float d = Math.Abs(Bias - other.Bias) + Math.Abs(Response - other.Response);
            if (Activation != other.Activation) d += 1.0f;
            if (Aggregation != other.Aggregation) d += 1.0f;
            return d * config.CompatibilityWeightCoefficient;
        }

        public override BaseGene Crossover(BaseGene other)
        {
            if (!(other is DefaultNodeGene otherGene))
                throw new ArgumentException("Cannot crossover with a different type of gene");

            var child = new DefaultNodeGene(Key);
            foreach (var (name, attr) in _geneAttributes)
            {
                var childAttr = attr.Clone();
                childAttr.Value = Random.Shared.NextDouble() < 0.5 ? GetAttributeValue(name) : otherGene.GetAttributeValue(name);
                child.SetAttribute(name, childAttr);
            }
            child.Type = Type; // Node type is determined by key, so we keep the same type
            return child;
        }
    }
}
