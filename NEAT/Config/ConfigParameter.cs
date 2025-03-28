using System;

namespace NEAT.Config
{
    public class ConfigParameter
    {
        public string Name { get; }
        public Type ValueType { get; }
        public object DefaultValue { get; }

        public ConfigParameter(string name, Type valueType, object defaultValue = null)
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
                throw new ArgumentException(string.Format("Failed to parse value '{0}' as {1} for parameter '{2}'", value, ValueType.Name, Name), ex);
            }
        }

        public string Format(object value)
        {
            if (value == null)
                return "";

            if (ValueType.IsEnum)
            {
                string enumValue = value.ToString();
                return enumValue != null ? enumValue.ToLower() : "";
            }

            string stringValue = value.ToString();
            return stringValue != null ? stringValue : "";
        }

        public override string ToString()
        {
            if (DefaultValue == null)
            {
                return string.Format("ConfigParameter(\"{0}\", {1})", Name, ValueType.Name);
            }
            return string.Format("ConfigParameter(\"{0}\", {1}, {2})", Name, ValueType.Name, Format(DefaultValue));
        }
    }
} 