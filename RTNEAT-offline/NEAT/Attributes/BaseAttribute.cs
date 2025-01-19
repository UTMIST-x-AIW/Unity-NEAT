using RTNEAT_offline.NEAT.Configuration;

namespace RTNEAT_offline.NEAT.Attributes;

using System;
using System.Collections.Generic;

public class BaseAttribute
{
    // Superclass for the type-specialized attribute subclasses, used by genes.
    public string Name { get; private set; }
    private Dictionary<string, Tuple<string, object>> _configItems = new Dictionary<string, Tuple<string, object>>();

    public BaseAttribute(string name, Dictionary<string, object> defaultDict)
    {
        Name = name;

        foreach (var item in defaultDict)
        {
            if (_configItems.ContainsKey(item.Key))
            {
                var current = _configItems[item.Key];
                _configItems[item.Key] = Tuple.Create(current.Item1, item.Value);
            }
        }

        foreach (var key in _configItems.Keys)
        {
            string propertyName = ConfigItemName(key);
            // Dynamically assign a property or perform additional logic as needed
        }
    }

    public string ConfigItemName(string configItemBaseName)
    {
        return $"{Name}_{configItemBaseName}";
    }

    public List<ConfigParameter> GetConfigParams()
    {
        var configParams = new List<ConfigParameter>();

        foreach (var item in _configItems)
        {
            configParams.Add(new ConfigParameter(
                ConfigItemName(item.Key),
                item.Value.Item1,
                item.Value.Item2
            ));
        }

        return configParams;
    }
}
