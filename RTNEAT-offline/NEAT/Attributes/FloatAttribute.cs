using System;
using RTNEAT_offline.NEAT.Configuration;

namespace RTNEAT_offline.NEAT.Attributes;

public class FloatAttribute : GeneAttribute
{
    private readonly float minValue;
    private readonly float maxValue;
    private readonly float mutationPower;
    private readonly float replaceRate;
    private readonly Random random;

    public FloatAttribute(string name, float defaultValue, float minValue = float.MinValue, float maxValue = float.MaxValue, float mutationPower = 0.1f, float replaceRate = 0.1f) 
        : base(name, defaultValue)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.mutationPower = mutationPower;
        this.replaceRate = replaceRate;
        this.random = new Random();
    }

    public override void MutateValue(Config config)
    {
        var r = random.NextDouble();
        var currentValue = Convert.ToSingle(Value);

        if (r < replaceRate)
        {
            // Replace with a random value
            Value = Convert.ToSingle(random.NextDouble() * (maxValue - minValue) + minValue);
        }
        else
        {
            // Perturb the current value
            var delta = (float)((random.NextDouble() * 2 - 1) * mutationPower);
            Value = Math.Clamp(currentValue + delta, minValue, maxValue);
        }
    }

    public override void Validate()
    {
        var value = (float)Value;
        if (value < minValue || value > maxValue)
        {
            throw new ArgumentException($"Value {value} is outside bounds [{minValue}, {maxValue}] for {Name}");
        }
    }
}