using defaultObjectnamespace;

namespace DefaultNamespace;

public class Config
// A config for user-configurable parameters of NEAT.
{
    private static readonly List<ConfigParameter> Parameters = new List<ConfigParameter>
    {
        new ConfigParameter("pop_size", typeof(int)),
        new ConfigParameter("fitness_criterion", typeof(string)),
        new ConfigParameter("fitness_threshold", typeof(float)),
        new ConfigParameter("reset_on_extinction", typeof(bool)),
        new ConfigParameter("no_fitness_termination", typeof(bool), false)
    };

    public Type GenomeType { get; }
    public Type ReproductionType { get; }
    public Type SpeciesSetType { get; }
    public Type StagnationType { get; }
    public Dictionary<string, object> ConfigValues { get; private set; }


    public Config(Type genomeType, Type reproductionType, Type speciesSetType, Type stagnationType, string fileName)
    {
        if (!File.Exists(fileName))
        {
            throw new FileNotFoundException($"Configuration file not found {fileName}");
        }

        GenomeType = genomeType;
        ReproductionType = reproductionType;
        SpeciesSetType = speciesSetType;
        StagnationType = stagnationType;

        var config = ParseConfigFile(fileName);

        ValidateAndSetParameters(config, "NEAT");

        // Parse sub-configuration sections.
        GenomeConfig = ParseSubConfig(GenomeType, config);
        ReproductionConfig = ParseSubConfig(ReproductionType, config);
        SpeciesSetConfig = ParseSubConfig(SpeciesSetType, config);
        StagnationConfig = ParseSubConfig(StagnationType, config);
    }

    public object GenomeConfig { get; private set; }
    public object ReproductionConfig { get; private set; }
    public object SpeciesSetConfig { get; private set; }
    public object StagnationConfig { get; private set; }

    private Dictionary<string, Dictionary<string, string>> ParseConfigFile(string filename)
    {
        var config = new Dictionary<string, Dictionary<string, string>>();

        foreach (var line in File.ReadLines(filename))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                var section = line.Trim('[', ']');
                if (!config.ContainsKey(section))
                {
                    config[section] = new Dictionary<string, string>();
                }
            }
            else
            {
                var keyValue = line.Split('=', 2);
                if (keyValue.Length == 2)
                {
                    var key = keyValue[0].Trim();
                    var value = keyValue[1].Trim();

                    if (config.Count > 0)
                    {
                        var lastSection = config.Keys.Last(); 
                        if (config.TryGetValue(lastSection, out var sectionConfig))
                        {
                            sectionConfig[key] = value; 
                        }
                    }
                }
            }
        }

        return config;
    }

    private void ValidateAndSetParameters(
        Dictionary<string, Dictionary<string, string>> config,
        string section)
    {
        if (!config.ContainsKey(section))
        {
            throw new Exception($"Missing required section: {section}");
        }

        var sectionConfig = config[section];
        ConfigValues = new Dictionary<string, object>();

        foreach (var param in Parameters)
        {
            if (sectionConfig.TryGetValue(param.getname(), out var value))
            {
                ConfigValues[param.getname()] = param.Parse(section, sectionConfig);
            }
            else if (param.getDefault() != null)
            {
                ConfigValues[param.getname()] = param.getDefault();
            }
            else
            {
                throw new Exception($"Missing required parameter: {param.getname()}");
            }
        }

        var unknownKeys = sectionConfig.Keys.Except(Parameters.Select(p => p.getname()));
        if (unknownKeys.Any())
        {
            throw new Exception($"Unknown configuration keys: {string.Join(", ", unknownKeys)}");
        }
    }

    private object ParseSubConfig(Type configType,
        Dictionary<string, Dictionary<string, string>> config)
    {
        var sectionName = configType.Name;
        if (!config.TryGetValue(sectionName, out var sectionConfig))
        {
            throw new Exception($"Missing section for {sectionName}");
        }
        
        var instance = Activator.CreateInstance(configType);
        
        var parseConfigMethod = configType.GetMethod("ParseConfig");
        if (parseConfigMethod == null)
        {
            throw new Exception($"The type {configType.Name} does not contain a method named 'ParseConfig'");
        }

        // Invoke the ParseConfig method on the instance
        return parseConfigMethod.Invoke(instance, new object[] { sectionConfig });
    }

    public void Save(string filename)
    {
        using (var writer = new StreamWriter(filename))
        {
            writer.WriteLine("[NEAT]");
            foreach (var param in Parameters)
            {
                writer.WriteLine($"{param.getname()} = {param.Format(ConfigValues[param.getname()])}");
            }

            WriteSubConfig(writer, GenomeType, GenomeConfig);
            WriteSubConfig(writer, ReproductionType, ReproductionConfig);
            WriteSubConfig(writer, SpeciesSetType, SpeciesSetConfig);
            WriteSubConfig(writer, StagnationType, StagnationConfig);
        }
    }

    private void WriteSubConfig(StreamWriter writer, Type type, object config)
    {
        writer.WriteLine($"\n[{type.Name}]");
        var writeConfigMethod = type.GetMethod("WriteConfig");
        writeConfigMethod.Invoke(null, new object[] { writer, config });
    }
}
