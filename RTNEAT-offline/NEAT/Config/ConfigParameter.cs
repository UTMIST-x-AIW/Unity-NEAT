namespace RTNEAT_offline.NEAT.Configuration;

public class ConfigParameter
{
    private string name;
    private Type valueType;
    private object? defaultObject;
    private string? description;

    public string Name => name;
    public Type ValueType => valueType;
    public object? DefaultValue => defaultObject;
    public string? Description => description;

    public ConfigParameter(string name, Type valueType, object? defaultObject = null, string? description = null)
    {
        this.name = name;
        this.valueType = valueType;
        this.defaultObject = defaultObject;
        this.description = description;
    }

    public ConfigParameter(string name, string type, string defaultValue, string? description = null)
    {
        this.name = name;
        this.valueType = Type.GetType(type) ?? typeof(string);
        this.defaultObject = defaultValue;
        this.description = description;
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
        if (!config.ContainsKey(name))
            return defaultObject;

        var value = config[name];
        if (value == null)
            return defaultObject;

        try
        {
            if (valueType == typeof(double))
            {
                if (value is double d) return d;
                if (value is int i) return (double)i;
                if (value is float f) return (double)f;
                return Convert.ToDouble(value);
            }
            else if (valueType == typeof(int))
            {
                if (value is int i) return i;
                if (value is double d) return (int)d;
                if (value is float f) return (int)f;
                return Convert.ToInt32(value);
            }
            else if (valueType == typeof(bool))
            {
                if (value is bool b) return b;
                if (value is string s)
                {
                    return s.ToLower() switch
                    {
                        "true" or "1" or "yes" or "on" => true,
                        "false" or "0" or "no" or "off" => false,
                        _ => throw new Exception($"Invalid boolean value: {s}")
                    };
                }
                return Convert.ToBoolean(value);
            }
            else if (valueType == typeof(string))
            {
                return value.ToString();
            }
            else if (valueType == typeof(Type))
            {
                return value as Type ?? Type.GetType(value.ToString());
            }
            
            throw new InvalidOperationException($"Unsupported configuration type: {valueType}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error interpreting parameter '{name}': {ex.Message}", ex);
        }
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
