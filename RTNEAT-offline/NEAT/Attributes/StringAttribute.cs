namespace RTNEAT_offline.NEAT.Attributes;

using System;
using System.Collections.Generic;
using RTNEAT_offline.NEAT.Configuration;

public class StringAttribute : GeneAttribute
{
    private readonly List<string> options;
    private readonly float mutateRate;
    private readonly Random random;

    public StringAttribute(string name, string defaultValue, List<string>? options = null, float mutateRate = 0.1f) 
        : base(name, defaultValue)
    {
        this.options = options ?? new List<string> { defaultValue };
        this.mutateRate = mutateRate;
        this.random = new Random();

        if (!this.options.Contains(defaultValue))
        {
            throw new ArgumentException($"Default value {defaultValue} not in options list for {Name}");
        }
    }

    public override void MutateValue(Config config)
    {
        if (random.NextDouble() < mutateRate)
        {
            var currentIndex = options.IndexOf((string)Value);
            var newIndex = random.Next(options.Count - 1);
            if (newIndex >= currentIndex)
            {
                newIndex++; // Skip the current value
            }
            Value = options[newIndex];
        }
    }

    public override void Validate()
    {
        if (Value is not string value)
        {
            throw new ArgumentException($"Value {Value} is not a string for {Name}");
        }
        if (!options.Contains(value))
        {
            throw new ArgumentException($"Value {value} not in options list for {Name}");
        }
    }
}
