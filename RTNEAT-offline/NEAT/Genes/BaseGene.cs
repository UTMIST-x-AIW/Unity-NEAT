using System;
using System.Collections.Generic;
using System.Linq;
using RTNEAT_offline.NEAT.Configuration;
using RTNEAT_offline.NEAT.Attributes;

namespace RTNEAT_offline.NEAT.Genes
{
    public abstract class BaseGene
    {
        protected Dictionary<string, GeneAttribute> _geneAttributes;
        public object Key { get; private set; }

        protected BaseGene(object key)
        {
            Key = key;
            _geneAttributes = new Dictionary<string, GeneAttribute>();
        }

        public override string ToString()
        {
            var attribs = new[] { "Key" }.Concat(_geneAttributes.Keys);
            var attribValues = attribs.Select(a => $"{a}={GetType().GetProperty(a)?.GetValue(this)}");
            return $"{GetType().Name}({string.Join(", ", attribValues)})";
        }

        public static bool operator <(BaseGene left, BaseGene right)
        {
            if (left.Key.GetType() != right.Key.GetType())
                throw new ArgumentException($"Cannot compare keys {left.Key} and {right.Key}");
            return Comparer<object>.Default.Compare(left.Key, right.Key) < 0;
        }

        public static bool operator >(BaseGene left, BaseGene right)
        {
            return !(left < right);
        }

        public static void ParseConfig(Config config, Dictionary<string, object> paramDict)
        {
            // This method should be implemented by derived classes if needed
        }

        public List<object> GetConfigParams()
        {
            var paramsList = new List<object>();
            foreach (var attribute in _geneAttributes.Values)
            {
                paramsList.Add(attribute);
            }
            return paramsList;
        }

        public void ValidateAttributes(Config config)
        {
            foreach (var attribute in _geneAttributes.Values)
            {
                attribute.Validate();
            }
        }

        public void ConfigureAttributes(Dictionary<string, object>? defaultValues = null)
        {
            foreach (var attr in _geneAttributes.Values)
            {
                if (defaultValues != null && defaultValues.TryGetValue(attr.Name, out var value))
                {
                    attr.InitValue(value);
                }
                else
                {
                    attr.InitValue(null);
                }
            }
        }

        public void MutateAttributes(Config config)
        {
            foreach (var attr in _geneAttributes.Values)
            {
                attr.MutateValue(config);
            }
        }

        public void ValidateAttributes()
        {
            foreach (var attr in _geneAttributes.Values)
            {
                attr.Validate();
            }
        }

        public Dictionary<string, GeneAttribute> GetAttributes()
        {
            return new Dictionary<string, GeneAttribute>(_geneAttributes);
        }

        public GeneAttribute GetAttribute(string name)
        {
            if (!_geneAttributes.TryGetValue(name, out var attribute))
            {
                throw new KeyNotFoundException($"Attribute {name} not found");
            }
            return attribute;
        }

        public void SetAttribute(string name, GeneAttribute attribute)
        {
            _geneAttributes[name] = attribute;
        }

        public object GetAttributeValue(string name)
        {
            return GetAttribute(name).Value;
        }

        public void SetAttributeValue(string name, object value)
        {
            GetAttribute(name).Value = value;
        }

        public abstract BaseGene Crossover(BaseGene other);

        public virtual BaseGene Clone()
        {
            var clone = (BaseGene)MemberwiseClone();
            clone._geneAttributes = new Dictionary<string, GeneAttribute>();
            foreach (var (name, attr) in _geneAttributes)
            {
                clone.SetAttribute(name, attr.Clone());
            }
            return clone;
        }
    }
}
