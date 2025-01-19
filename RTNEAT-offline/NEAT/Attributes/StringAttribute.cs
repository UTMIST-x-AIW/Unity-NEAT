namespace RTNEAT_offline.NEAT.Attributes;

using System;
using System.Collections.Generic;

public class StringAttribute : BaseAttribute
{
    // Config items specific to StringAttribute
    private static readonly Dictionary<string, Tuple<Type, object>> _configItems = new()
    {
        { "default", new Tuple<Type, object>(typeof(string), "random") },
        { "options", new Tuple<Type, object>(typeof(List<string>), null) },
        { "mutate_rate", new Tuple<Type, object>(typeof(float), null) }
    };

    public StringAttribute(string name, Dictionary<string, object> defaultDict) 
        : base(name, defaultDict)
    {
    }

    public string InitValue(dynamic config)
    {
        string defaultValue = (string)config.GetType().GetProperty(ConfigItemName("default")).GetValue(config);

        if (defaultValue.ToLower() == "none" || defaultValue.ToLower() == "random")
        {
            var options = (List<string>)config.GetType().GetProperty(ConfigItemName("options")).GetValue(config);
            return RandomChoice(options);
        }

        return defaultValue;
    }

    public string MutateValue(string value, dynamic config)
    {
        float mutateRate = (float)config.GetType().GetProperty(ConfigItemName("mutate_rate")).GetValue(config);

        if (mutateRate > 0)
        {
            float r = RandomValue();
            if (r < mutateRate)
            {
                var options = (List<string>)config.GetType().GetProperty(ConfigItemName("options")).GetValue(config);
                return RandomChoice(options);
            }
        }

        return value;
    }

    public void Validate(dynamic config)
    {
        string defaultValue = (string)config.GetType().GetProperty(ConfigItemName("default")).GetValue(config);

        if (defaultValue.ToLower() != "none" && defaultValue.ToLower() != "random")
        {
            var options = (List<string>)config.GetType().GetProperty(ConfigItemName("options")).GetValue(config);
            if (!options.Contains(defaultValue))
            {
                throw new InvalidOperationException($"Invalid initial value '{defaultValue}' for {Name}");
            }
        }
    }

    // Helper method for randomness (replace with your preferred random library)
    private static string RandomChoice(List<string> options)
    {
        var rand = new Random();
        int index = rand.Next(options.Count);
        return options[index];
    }

    private static float RandomValue()
    {
        var rand = new Random();
        return (float)rand.NextDouble();
    }
}
