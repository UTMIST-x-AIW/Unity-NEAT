namespace RTNEAT_offline.NEAT.Attributes;

using System;
using System.Collections.Generic;

public class BoolAttribute : BaseAttribute
{
    // Config items specific to BoolAttribute
    private static readonly Dictionary<string, Tuple<Type, object>> _configItems = new()
    {
        { "default", new Tuple<Type, object>(typeof(string), null) },
        { "mutate_rate", new Tuple<Type, object>(typeof(float), null) },
        { "rate_to_true_add", new Tuple<Type, object>(typeof(float), 0.0f) },
        { "rate_to_false_add", new Tuple<Type, object>(typeof(float), 0.0f) }
    };

    public BoolAttribute(string name, Dictionary<string, object> defaultDict) 
        : base(name, defaultDict)
    {
    }

    public bool InitValue(dynamic config)
    {
        string defaultValue = ((string)config.GetType().GetProperty(ConfigItemName("default")).GetValue(config)).ToLower();

        switch (defaultValue)
        {
            case "1":
            case "on":
            case "yes":
            case "true":
                return true;
            case "0":
            case "off":
            case "no":
            case "false":
                return false;
            case "random":
            case "none":
                return RandomValue() < 0.5;
            default:
                throw new InvalidOperationException($"Unknown default value '{defaultValue}' for {Name}");
        }
    }

    public bool MutateValue(bool value, dynamic config)
    {
        float mutateRate = (float)config.GetType().GetProperty(ConfigItemName("mutate_rate")).GetValue(config);

        if (value)
        {
            mutateRate += (float)config.GetType().GetProperty(ConfigItemName("rate_to_false_add")).GetValue(config);
        }
        else
        {
            mutateRate += (float)config.GetType().GetProperty(ConfigItemName("rate_to_true_add")).GetValue(config);
        }

        if (mutateRate > 0)
        {
            float r = RandomValue();
            if (r < mutateRate)
            {
                // Perform a random flip with a 50% chance
                return RandomValue() < 0.5;
            }
        }

        return value;
    }

    public void Validate(dynamic config)
    {
        string defaultValue = ((string)config.GetType().GetProperty(ConfigItemName("default")).GetValue(config)).ToLower();

        if (defaultValue != "1" && defaultValue != "on" && defaultValue != "yes" && defaultValue != "true" &&
            defaultValue != "0" && defaultValue != "off" && defaultValue != "no" && defaultValue != "false" &&
            defaultValue != "random" && defaultValue != "none")
        {
            throw new InvalidOperationException($"Invalid default value for {Name}");
        }
    }

    // Helper method for randomness (replace with your preferred random library)
    private static float RandomValue()
    {
        var rand = new Random();
        return (float)rand.NextDouble();
    }
}
