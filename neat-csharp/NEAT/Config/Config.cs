using System;
using System.Collections.Generic;
using System.IO;

namespace NEAT.Config
{
    public class Config
    {
        private readonly Dictionary<string, object> _parameters;

        public Config()
        {
            _parameters = new Dictionary<string, object>();
        }

        public void LoadConfig(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException($"Config file not found: {filename}");

            var lines = File.ReadAllLines(filename);
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                var parts = trimmedLine.Split('=');
                if (parts.Length != 2)
                    continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                // Store the parameter
                _parameters[key] = value;
            }
        }

        public T GetParameter<T>(string name, T defaultValue)
        {
            if (!_parameters.TryGetValue(name, out var value))
                return defaultValue;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        public void SetParameter<T>(string name, T value)
        {
            _parameters[name] = value!;
        }
    }
} 