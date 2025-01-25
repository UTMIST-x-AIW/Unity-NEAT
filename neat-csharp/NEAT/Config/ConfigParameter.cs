using System;

namespace NEAT.Config
{
    public class ConfigParameter<T>
    {
        public string Name { get; }
        public T DefaultValue { get; }
        public string Description { get; }
        public T Value { get; private set; }

        public ConfigParameter(string name, T defaultValue, string description = "")
        {
            Name = name;
            DefaultValue = defaultValue;
            Description = description;
            Value = defaultValue;
        }

        public void SetValue(T value)
        {
            Value = value;
        }

        public T GetValue()
        {
            return Value;
        }
    }
} 