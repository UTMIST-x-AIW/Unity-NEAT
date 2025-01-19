namespace RTNEAT_offline.NEAT.Attributes;

using System;
using System.Collections.Generic;

public class IntegerAttribute : BaseAttribute
{
    // Config items specific to IntegerAttribute
    private static readonly Dictionary<string, Tuple<Type, object>> _configItems = new()
    {
        { "replace_rate", new Tuple<Type, object>(typeof(float), null) },
        { "mutate_rate", new Tuple<Type, object>(typeof(float), null) },
        { "mutate_power", new Tuple<Type, object>(typeof(float), null) },
        { "max_value", new Tuple<Type, object>(typeof(int), null) },
        { "min_value", new Tuple<Type, object>(typeof(int), null) }
    };

    public IntegerAttribute(string name, Dictionary<string, object> defaultDict) 
        : base(name, defaultDict)
    {
    }

    public int Clamp(int value, dynamic config)
    {
        int minValue = (int)config.GetType().GetProperty(ConfigItemName("min_value")).GetValue(config);
        int maxValue = (int)config.GetType().GetProperty(ConfigItemName("max_value")).GetValue(config);

        return Math.Max(Math.Min(value, maxValue), minValue);
    }

    public int InitValue(dynamic config)
    {
        int minValue = (int)config.GetType().GetProperty(ConfigItemName("min_value")).GetValue(config);
        int maxValue = (int)config.GetType().GetProperty(ConfigItemName("max_value")).GetValue(config);

        return RandomInteger(minValue, maxValue);
    }

    public int MutateValue(int value, dynamic config)
    {
        float mutateRate = (float)config.GetType().GetProperty(ConfigItemName("mutate_rate")).GetValue(config);
        float r = RandomValue();

        if (r < mutateRate)
        {
            float mutatePower = (float)config.GetType().GetProperty(ConfigItemName("mutate_power")).GetValue(config);
            return Clamp(value + (int)Math.Round(RandomGaussian(0.0f, mutatePower)), config);
        }

        float replaceRate = (float)config.GetType().GetProperty(ConfigItemName("replace_rate")).GetValue(config);

        if (r < replaceRate + mutateRate)
        {
            return InitValue(config);
        }

        return value;
    }

    public void Validate(dynamic config)
    {
        int minValue = (int)config.GetType().GetProperty(ConfigItemName("min_value")).GetValue(config);
        int maxValue = (int)config.GetType().GetProperty(ConfigItemName("max_value")).GetValue(config);

        if (maxValue < minValue)
        {
            throw new InvalidOperationException($"Invalid min/max configuration for {Name}");
        }
    }

    // Helper methods for randomness (replace with your preferred random library)
    private static int RandomInteger(int minValue, int maxValue)
    {
        var rand = new Random();
        return rand.Next(minValue, maxValue + 1); // C# upper bound is exclusive
    }

    private static float RandomGaussian(float mean, float stdev)
    {
        var rand = new Random();
        double u1 = 1.0 - rand.NextDouble();
        double u2 = 1.0 - rand.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return (float)(mean + stdev * randStdNormal);
    }

    private static float RandomValue()
    {
        var rand = new Random();
        return (float)rand.NextDouble();
    }
}
