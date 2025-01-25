using System;
using RTNEAT_offline.NEAT.Configuration;

namespace RTNEAT_offline.NEAT.Attributes
{
    public abstract class GeneAttribute
    {
        public string Name { get; }
        public object Value { get; set; }
        public object DefaultValue { get; }

        protected GeneAttribute(string name, object defaultValue)
        {
            Name = name;
            DefaultValue = defaultValue;
            Value = defaultValue;
        }

        public virtual void InitValue(object value)
        {
            Value = value ?? DefaultValue;
        }

        public abstract void MutateValue(Config config);
        public abstract void Validate();

        public virtual GeneAttribute Clone()
        {
            var clone = (GeneAttribute)MemberwiseClone();
            clone.Value = Value;
            return clone;
        }
    }
} 