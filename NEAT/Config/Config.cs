using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NEAT.Config
{
    public class Config
    {
        private readonly Dictionary<string, object> _parameters;
        private readonly List<ConfigParameter> _configParameters;

        public Config()
        {
            _parameters = new Dictionary<string, object>();
            _configParameters = new List<ConfigParameter>
            {
                new ConfigParameter("num_inputs", typeof(int)),
                new ConfigParameter("num_outputs", typeof(int)),
                new ConfigParameter("num_hidden", typeof(int), 0),
                new ConfigParameter("population_size", typeof(int), 150),
                new ConfigParameter("compatibility_threshold", typeof(double), 3.0),
                new ConfigParameter("weight_coefficient", typeof(double), 0.5),
                new ConfigParameter("disjoint_coefficient", typeof(double), 1.0),
                new ConfigParameter("excess_coefficient", typeof(double), 1.0),
                new ConfigParameter("survival_threshold", typeof(double), 0.2),
                new ConfigParameter("survival_minspecies_size", typeof(int), 2),
                new ConfigParameter("elitism", typeof(bool), true),
                new ConfigParameter("reset_on_extinction", typeof(bool), false)
            };
        }

        public double CompatibilityThreshold { get; private set; }

        public void LoadConfig(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException($"Config file not found: {filename}");

            var lines = File.ReadAllLines(filename);
            string currentSection = "";
            var sectionParams = new Dictionary<string, Dictionary<string, string>>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    currentSection = trimmedLine.Trim('[', ']');
                    if (!sectionParams.ContainsKey(currentSection))
                    {
                        sectionParams[currentSection] = new Dictionary<string, string>();
                    }
                    continue;
                }

                var parts = trimmedLine.Split('=');
                if (parts.Length != 2)
                    continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                if (string.IsNullOrEmpty(currentSection))
                {
                    // Parse and validate parameter
                    var param = _configParameters.FirstOrDefault(p => p.Name == key);
                    if (param != null)
                    {
                        _parameters[key] = param.Parse(value);
                    }
                    else
                    {
                        // Store unvalidated parameter
                        _parameters[key] = value;
                    }
                }
                else
                {
                    // Store in section parameters
                    _parameters[$"{currentSection}.{key}"] = value;
                    sectionParams[currentSection][key] = value;
                }

                switch (key)
                {
                    case "compatibility_threshold":
                        CompatibilityThreshold = double.Parse(value);
                        break;
                }
            }

            // Set default values for missing parameters
            foreach (var param in _configParameters)
            {
                if (!_parameters.ContainsKey(param.Name) && param.DefaultValue != null)
                {
                    _parameters[param.Name] = param.DefaultValue;
                }
            }
        }

        public T GetParameter<T>(string name, T defaultValue)
        {
            if (!_parameters.TryGetValue(name, out var value))
                return defaultValue;

            try
            {
                if (typeof(T) == typeof(bool) && value is string strValue)
                {
                    // Handle common boolean string representations
                    return (T)(object)(strValue.ToLower() == "true" || strValue == "1" || strValue.ToLower() == "yes");
                }
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        public void SetParameter<T>(string name, T value)
        {
            var param = _configParameters.FirstOrDefault(p => p.Name == name);
            if (param != null)
            {
                if (!param.ValueType.IsAssignableFrom(typeof(T)))
                {
                    throw new ArgumentException($"Invalid type for parameter '{name}'. Expected {param.ValueType.Name}, got {typeof(T).Name}");
                }
            }
            _parameters[name] = value!;
        }

        public bool HasParameter(string name)
        {
            return _parameters.ContainsKey(name);
        }

        public void SaveConfig(string filename)
        {
            var sections = new Dictionary<string, List<KeyValuePair<string, object>>>();
            var rootParams = new List<KeyValuePair<string, object>>();

            foreach (var param in _parameters)
            {
                var parts = param.Key.Split('.');
                if (parts.Length == 1)
                {
                    rootParams.Add(param);
                }
                else
                {
                    var section = parts[0];
                    var key = parts[1];
                    if (!sections.ContainsKey(section))
                    {
                        sections[section] = new List<KeyValuePair<string, object>>();
                    }
                    sections[section].Add(new KeyValuePair<string, object>(key, param.Value));
                }
            }

            using (var writer = new StreamWriter(filename))
            {
                // Write root parameters
                foreach (var param in rootParams.OrderBy(p => p.Key))
                {
                    var configParam = _configParameters.FirstOrDefault(p => p.Name == param.Key);
                    var value = configParam != null ? configParam.Format(param.Value) : param.Value.ToString();
                    writer.WriteLine($"{param.Key} = {value}");
                }

                // Write sections
                foreach (var section in sections)
                {
                    writer.WriteLine();
                    writer.WriteLine($"[{section.Key}]");
                    foreach (var param in section.Value.OrderBy(p => p.Key))
                    {
                        writer.WriteLine($"{param.Key} = {param.Value}");
                    }
                }
            }
        }
    }
} 