using RTNEAT_Offline.NEAT.config;

namespace Defaultnamespace;

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

    public object Parse(string section, ConfigParser configParser)
    {
        if (valueType == typeof(int))
        {
            return configParser.GetInt32(section, name, (int)(defaultObject ?? 0));
        }

        if (valueType == typeof(bool))
        {
            return configParser.GetBoolean(section, name, (bool)(defaultObject ?? false));
        }

        if (valueType == typeof(double))
        {
            return configParser.GetDouble(section, name, (double)(defaultObject ?? 0.0));
        }

        if (valueType == typeof(List<string>))
        {
            return configParser.GetList(section, name);
        }

        if (valueType == typeof(string))
        {
            return configParser.GetString(section, name, (string)(defaultObject ?? ""));
        }

        throw new InvalidOperationException($"Unexpected configuration type: {valueType}");
    }

    public object Interpret(Dictionary<string, string> configDict)
    {
        if (!configDict.TryGetValue(name, out string value))
        {
            if (defaultObject == null)
            {
                throw new InvalidOperationException($"Missing configuration item: {name}");
            }
            else
            {
                Console.WriteLine($"Warning: Using default {defaultObject} for '{name}'");
                if (valueType != typeof(string) && defaultObject.GetType() == valueType)
                {
                    return defaultObject;
                }
                else
                {
                    value = defaultObject.ToString();
                }
            }
        }

        try
        {
            if (valueType == typeof(string))
                return value;
            if (valueType == typeof(int))
                return int.Parse(value);
            if (valueType == typeof(bool))
            {
                if (value.ToLower() == "true")
                    return true;
                else if (value.ToLower() == "false")
                    return false;
                else
                    throw new InvalidOperationException($"{name} must be True or False");
            }

            if (valueType == typeof(double))
                return double.Parse(value);
            if (valueType == typeof(List<string>))
                return value.Split(' ').ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Error interpreting config item '{name}' with value '{value}' and type {valueType}", ex);
        }

        throw new InvalidOperationException($"Unexpected configuration type: {valueType}");
    }

    public string Format(object value)
    {
        if (valueType == typeof(List<string>))
        {
            return string.Join(" ", (List<string>)value);
        }

        return value.ToString();
    }
}