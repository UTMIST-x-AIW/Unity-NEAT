using RTNEAT_Offline.NEAT.config;

namespace defaultObjectnamespace;

public class ConfigParameter
{
    private String name;
    private Object valueType;
    private Object defaultObject;

    public ConfigParameter(String name, Object valueType, Object defaultObject = null)
    {
        this.name = name;
        this.valueType = valueType;
        this.defaultObject = defaultObject;
    }

    public override string ToString()
    {
        if (defaultObject == null)
        {
            return $"ConfigParameter(\"{name}\", \"{valueType}\")";
        }

        return $"ConfigParameter(\"{name}\", \"{valueType}\", \"{defaultObject}\")";
    }

    public object Parse(string section, Dictionary<string, string> sectionConfig)
    {
        if (!sectionConfig.TryGetValue(name, out var value))
        {
            throw new Exception($"Missing required parameter '{name}' in section '{section}'.");
        }

        try
        {
            if (valueType == typeof(int))
            {
                return int.Parse(value);
            }
            if (valueType == typeof(bool))
            {
                return bool.Parse(value);
            }
            if (valueType == typeof(double))
            {
                return double.Parse(value);
            }
            if (valueType == typeof(List<string>))
            {
                return value.Split(' ').ToList();
            }
            if (valueType == typeof(string))
            {
                return value;
            }

            throw new InvalidOperationException($"Unsupported configuration type: {valueType}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error parsing parameter '{name}' in section '{section}': {ex.Message}", ex);
        }
    }

    public object Interpret(Dictionary<string, object> config)
    {
        if (!config.TryGetValue(name, out var value))
        {
            if (defaultObject == null)
                throw new Exception($"Missing configuration item: {name}");

            Console.WriteLine($"Using default value for '{name}': {defaultObject}");
            return defaultObject;
        }

        try
        {
            if (valueType == typeof(int))
                return Convert.ToInt32(value);
            if (valueType == typeof(bool))
                return Convert.ToBoolean(value);
            if (valueType == typeof(float))
                return Convert.ToSingle(value);
            if (valueType == typeof(string))
                return value.ToString();
            if (valueType == typeof(List<string>))
                return new List<string>(value.ToString().Split(' '));
        }
        catch
        {
            throw new Exception($"Error interpreting config item '{name}' with value {value} as {valueType}");
        }

        throw new InvalidOperationException($"Unsupported configuration type: {valueType}");
    }

    public string Format(object value)
    {
        if (valueType == typeof(List<string>))
        {
            return string.Join(" ", (List<string>)value);
        }

        return value.ToString();
    }

    public string getname()
    {
        return this.name;
    }

    public object getDefault()
    {
        return this.defaultObject;
    }
    
    //Omitted `write_pretty_params`
}