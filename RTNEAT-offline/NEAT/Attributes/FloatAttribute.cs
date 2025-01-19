namespace RTNEAT_offline.NEAT.Attributes;

public class FloatAttribute : BaseAttribute
{
    // Config items specific to FloatAttribute
    private static readonly Dictionary<string, Tuple<Type, object>> _configItems = new()
    {
        { "init_mean", Tuple.Create(typeof(float), (object)null) },
        { "init_stdev", Tuple.Create(typeof(float), (object)null) },
        { "init_type", Tuple.Create(typeof(string), (object)"gaussian") },
        { "replace_rate", Tuple.Create(typeof(float), (object)null) },
        { "mutate_rate", Tuple.Create(typeof(float), (object)null) },
        { "mutate_power", Tuple.Create(typeof(float), (object)null) },
        { "max_value", Tuple.Create(typeof(float), (object)null) },
        { "min_value", Tuple.Create(typeof(float), (object)null) }
    };

    public FloatAttribute(string name, Dictionary<string, object> defaultDict) 
        : base(name, defaultDict)
    {
    }

    public float Clamp(float value, dynamic config)
    {
        float minValue = (float)config.GetType().GetProperty(ConfigItemName("min_value")).GetValue(config);
        float maxValue = (float)config.GetType().GetProperty(ConfigItemName("max_value")).GetValue(config);

        return Math.Max(Math.Min(value, maxValue), minValue);
    }

    public float InitValue(dynamic config)
    {
        float mean = (float)config.GetType().GetProperty(ConfigItemName("init_mean")).GetValue(config);
        float stdev = (float)config.GetType().GetProperty(ConfigItemName("init_stdev")).GetValue(config);
        string initType = ((string)config.GetType().GetProperty(ConfigItemName("init_type")).GetValue(config)).ToLower();

        if (initType.Contains("gauss") || initType.Contains("normal"))
        {
            return Clamp(RandomGaussian(mean, stdev), config);
        }
        else if (initType.Contains("uniform"))
        {
            float minValue = Math.Max(
                (float)config.GetType().GetProperty(ConfigItemName("min_value")).GetValue(config),
                mean - (2 * stdev)
            );
            float maxValue = Math.Min(
                (float)config.GetType().GetProperty(ConfigItemName("max_value")).GetValue(config),
                mean + (2 * stdev)
            );
            return RandomUniform(minValue, maxValue);
        }

        throw new InvalidOperationException($"Unknown init_type {initType} for {ConfigItemName("init_type")}");
    }

    public float MutateValue(float value, dynamic config)
    {
        float mutateRate = (float)config.GetType().GetProperty(ConfigItemName("mutate_rate")).GetValue(config);
        float r = RandomValue();

        if (r < mutateRate)
        {
            float mutatePower = (float)config.GetType().GetProperty(ConfigItemName("mutate_power")).GetValue(config);
            return Clamp(value + RandomGaussian(0.0f, mutatePower), config);
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
        float minValue = (float)config.GetType().GetProperty(ConfigItemName("min_value")).GetValue(config);
        float maxValue = (float)config.GetType().GetProperty(ConfigItemName("max_value")).GetValue(config);

        if (maxValue < minValue)
        {
            throw new InvalidOperationException($"Invalid min/max configuration for {Name}");
        }
    }

    // Helper methods for randomness (replace with your preferred random library)
    private static float RandomGaussian(float mean, float stdev)
    {
        var rand = new Random();
        double u1 = 1.0 - rand.NextDouble();
        double u2 = 1.0 - rand.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return (float)(mean + stdev * randStdNormal);
    }

    private static float RandomUniform(float minValue, float maxValue)
    {
        var rand = new Random();
        return (float)(minValue + rand.NextDouble() * (maxValue - minValue));
    }

    private static float RandomValue()
    {
        var rand = new Random();
        return (float)rand.NextDouble();
    }
}