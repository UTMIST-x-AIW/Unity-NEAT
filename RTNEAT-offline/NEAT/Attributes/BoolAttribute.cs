using System;
using RTNEAT_offline.NEAT.Configuration;

namespace RTNEAT_offline.NEAT.Attributes
{
    public class BoolAttribute : GeneAttribute
    {
        private readonly float mutateRate;
        private readonly Random random;

        public BoolAttribute(string name, bool defaultValue, float mutateRate = 0.1f) 
            : base(name, defaultValue)
        {
            this.mutateRate = mutateRate;
            this.random = new Random();
        }

        public override void MutateValue(Config config)
        {
            if (random.NextDouble() < mutateRate)
            {
                Value = !(bool)Value;
            }
        }

        public override void Validate()
        {
            if (Value is not bool)
            {
                throw new ArgumentException($"Value {Value} is not a boolean for {Name}");
            }
        }
    }
}
