using System;

namespace NEAT.Config
{
    public class ConfigParameter
    {
        public string Name { get; }
        public Type ValueType { get; }
        public object? DefaultValue { get; }

        public ConfigParameter(string name, Type valueType, object? defaultValue = null)
        {
            Name = name;
            ValueType = valueType;
            DefaultValue = defaultValue;
        }

        public object Parse(string value)
        {
            try
            {
                if (ValueType == typeof(bool))
                {
                    return value.ToLower() == "true" || value == "1" || value.ToLower() == "yes";
                }

                if (ValueType.IsEnum)
                {
                    return Enum.Parse(ValueType, value, true);
                }

                return Convert.ChangeType(value, ValueType);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to parse value '{value}' as {ValueType.Name} for parameter '{Name}'", ex);
            }
        }

        public string Format(object value)
        {
            if (value == null)
                return "";

            if (ValueType.IsEnum)
            {
                return value.ToString()?.ToLower() ?? "";
            }

            return value.ToString() ?? "";
        }

        public override string ToString()
        {
            if (DefaultValue == null)
            {
                return $"ConfigParameter(\"{Name}\", {ValueType.Name})";
            }
            return $"ConfigParameter(\"{Name}\", {ValueType.Name}, {Format(DefaultValue)})";
        }
    }
} 