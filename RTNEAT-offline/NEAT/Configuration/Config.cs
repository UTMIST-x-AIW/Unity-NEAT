using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace RTNEAT_offline.NEAT.Configuration
{
    public class Config
    {
        protected static readonly List<ConfigParameter> Parameters = new List<ConfigParameter>
        {
            new ConfigParameter("pop_size", typeof(int)),
            new ConfigParameter("fitness_criterion", typeof(string)),
            new ConfigParameter("fitness_threshold", typeof(float)),
            new ConfigParameter("reset_on_extinction", typeof(bool)),
            new ConfigParameter("no_fitness_termination", typeof(bool), false),
            new ConfigParameter("max_stagnation", typeof(int), 15),
            new ConfigParameter("species_distance_threshold", typeof(float), 3.0f)
        };

        protected Dictionary<string, object> ConfigValues { get; set; }

        // Properties from the original Config class
        public Type? GenomeType { get; private set; }
        public Type? ReproductionType { get; private set; }
        public Type? SpeciesSetType { get; private set; }
        public Type? StagnationType { get; private set; }
        public object? GenomeConfig { get; private set; }
        public object? ReproductionConfig { get; private set; }
        public object? SpeciesSetConfig { get; private set; }
        public object? StagnationConfig { get; private set; }

        // Properties from the second Config class
        public float SpeciesDistanceThreshold { get; set; } = 3.0f;
        public int Generation { get; set; } = 0;
        public float CompatibilityWeightCoefficient { get; set; } = 0.5f;
        public float NodeAddProb { get; set; } = 0.2f;
        public float NodeDeleteProb { get; set; } = 0.2f;
        public float ConnAddProb { get; set; } = 0.5f;
        public float ConnDeleteProb { get; set; } = 0.5f;
        public float BiasMutateRate { get; set; } = 0.7f;
        public float WeightMutateRate { get; set; } = 0.8f;
        public float WeightReplaceMutateRate { get; set; } = 0.1f;
        public float DisableMutateRate { get; set; } = 0.01f;
        public float EnableMutateRate { get; set; } = 0.01f;
        public int MaxStagnation { get; private set; }

        public Config()
        {
            ConfigValues = new Dictionary<string, object>();
        }

        public Config(Type genomeType, Type reproductionType, Type speciesSetType, Type stagnationType, string fileName)
            : this()
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

        public virtual void Load(string filename)
        {
            if (filename.EndsWith(".json"))
            {
                var jsonString = File.ReadAllText(filename);
                ConfigValues = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString) ?? new Dictionary<string, object>();
            }
            else
            {
                var config = ParseConfigFile(filename);
                ValidateAndSetParameters(config, "NEAT");
            }
        }

        public T Get<T>(string key)
        {
            if (ConfigValues.TryGetValue(key, out var value))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            throw new KeyNotFoundException($"Key {key} not found in configuration");
        }

        public void Set<T>(string key, T value)
        {
            ConfigValues[key] = value!;
        }

        public bool HasKey(string key)
        {
            return ConfigValues.ContainsKey(key);
        }

        public void Save(string filename)
        {
            if (filename.EndsWith(".json"))
            {
                var jsonString = JsonSerializer.Serialize(ConfigValues, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filename, jsonString);
            }
            else
            {
                using var writer = new StreamWriter(filename);
                writer.WriteLine("[NEAT]");
                foreach (var param in Parameters)
                {
                    if (ConfigValues.TryGetValue(param.Name, out var value))
                    {
                        writer.WriteLine($"{param.Name} = {value}");
                    }
                }

                if (GenomeType != null)
                    WriteSubConfig(writer, GenomeType, GenomeConfig!);
                if (ReproductionType != null)
                    WriteSubConfig(writer, ReproductionType, ReproductionConfig!);
                if (SpeciesSetType != null)
                    WriteSubConfig(writer, SpeciesSetType, SpeciesSetConfig!);
                if (StagnationType != null)
                    WriteSubConfig(writer, StagnationType, StagnationConfig!);
            }
        }

        private Dictionary<string, Dictionary<string, string>> ParseConfigFile(string filename)
        {
            var config = new Dictionary<string, Dictionary<string, string>>();
            string currentSection = "";

            foreach (var line in File.ReadLines(filename))
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    if (!config.ContainsKey(currentSection))
                    {
                        config[currentSection] = new Dictionary<string, string>();
                    }
                    continue;
                }

                if (string.IsNullOrEmpty(currentSection))
                    continue;

                var parts = trimmedLine.Split('=', 2);
                if (parts.Length != 2)
                    continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                // Remove any inline comments
                var commentIndex = value.IndexOf('#');
                if (commentIndex >= 0)
                {
                    value = value.Substring(0, commentIndex).Trim();
                }

                config[currentSection][key] = value;
            }

            return config;
        }

        private void ValidateAndSetParameters(Dictionary<string, Dictionary<string, string>> config, string section)
        {
            if (!config.ContainsKey(section))
            {
                throw new Exception($"Missing required section: {section}");
            }

            var sectionConfig = config[section];
            foreach (var param in Parameters)
            {
                if (sectionConfig.TryGetValue(param.Name, out var value))
                {
                    var convertedValue = Convert.ChangeType(value, param.ValueType);
                    ConfigValues[param.Name] = convertedValue;
                }
                else if (param.DefaultValue != null)
                {
                    ConfigValues[param.Name] = param.DefaultValue;
                }
                else
                {
                    throw new Exception($"Missing required parameter: {param.Name}");
                }
            }
        }

        private object? ParseSubConfig(Type configType, Dictionary<string, Dictionary<string, string>> config)
        {
            var sectionName = configType.Name;
            if (!config.ContainsKey(sectionName))
            {
                return null;
            }

            var instance = Activator.CreateInstance(configType);
            var sectionConfig = config[sectionName];

            foreach (var prop in configType.GetProperties())
            {
                if (sectionConfig.TryGetValue(prop.Name, out var value))
                {
                    var convertedValue = Convert.ChangeType(value, prop.PropertyType);
                    prop.SetValue(instance, convertedValue);
                }
            }

            return instance;
        }

        private void WriteSubConfig(StreamWriter writer, Type type, object config)
        {
            writer.WriteLine($"[{type.Name}]");
            foreach (var prop in type.GetProperties())
            {
                var value = prop.GetValue(config);
                if (value != null)
                {
                    writer.WriteLine($"{prop.Name} = {value}");
                }
            }
            writer.WriteLine();
        }

        public virtual void LoadFromParameters(Dictionary<string, string> parameters)
        {
            foreach (var param in Parameters)
            {
                if (parameters.TryGetValue(param.Name, out var value))
                {
                    ConfigValues[param.Name] = Convert.ChangeType(value, param.ValueType);
                }
            }
        }

        public virtual List<ConfigParameter> GetConfigParameters()
        {
            return new List<ConfigParameter>(Parameters);
        }
    }
} 