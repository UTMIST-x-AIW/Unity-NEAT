namespace RTNEAT_offline.NEAT.Configuration;
public class ConfigParser
{
    private Dictionary<string, Dictionary<string, string>> _sections;

    public ConfigParser()
    {
        _sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
    }

    public void ReadFile(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        string currentSection = "";

        foreach (var line in lines)
        {
            string trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#"))
                continue;

            if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
            {
                currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                if (!_sections.ContainsKey(currentSection))
                    _sections[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                int equalsIndex = trimmedLine.IndexOf('=');
                if (equalsIndex > 0)
                {
                    string key = trimmedLine.Substring(0, equalsIndex).Trim();
                    string value = trimmedLine.Substring(equalsIndex + 1).Trim();
                    _sections[currentSection][key] = value;
                }
            }
        }
    }

    public string GetString(string section, string key, string defaultValue = "")
    {
        if (_sections.TryGetValue(section, out var sectionDict) && sectionDict.TryGetValue(key, out var value))
            return value;
        return defaultValue;
    }

    public int GetInt32(string section, string key, int defaultValue = 0)
    {
        string value = GetString(section, key);
        return int.TryParse(value, out int result) ? result : defaultValue;
    }

    public bool GetBoolean(string section, string key, bool defaultValue = false)
    {
        string value = GetString(section, key);
        return bool.TryParse(value, out bool result) ? result : defaultValue;
    }

    public double GetDouble(string section, string key, double defaultValue = 0.0)
    {
        string value = GetString(section, key);
        return double.TryParse(value, out double result) ? result : defaultValue;
    }

    public List<string> GetList(string section, string key, string separator = " ")
    {
        string value = GetString(section, key);
        return value.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}
