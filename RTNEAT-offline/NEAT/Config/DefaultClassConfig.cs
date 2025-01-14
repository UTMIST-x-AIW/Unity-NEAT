using defaultObjectnamespace;

namespace RTNEAT_offline.NEAT.Config;
using System.Collections.Generic;
using System.Linq;

public class DefaultClassConfig
{
    private readonly Dictionary<string, object> _params = new();
    public DefaultClassConfig(Dictionary<string, object> paramDict , List<ConfigParameter> paramList)
    {
        var paramListNames = new List<string>();

        foreach (var param in paramList)
        {
            _params[param.getname()] = param.Interpret(paramDict);
        }
        
        var unknownList = paramDict.Keys.Where(key => !paramListNames.Contains(key)).ToList();
        if (unknownList.Count > 0)
        {
            if (unknownList.Count > 1)
                throw new ArgumentException($"Unknown configuration items:\n\t{string.Join("\n\t", unknownList)}");
            throw new ArgumentException($"Unknown configuration item {unknownList[0]}");
        }
    }
}