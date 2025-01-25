namespace RTNEAT_offline.NEAT.Attributes;

using System;
using System.Collections.Generic;
using RTNEAT_offline.NEAT.Configuration;

public class StringAttribute : GeneAttribute
{
    private readonly List<string> _options;
    private readonly float _mutateRate;
    private readonly Random _random;

    public StringAttribute(string name, string defaultValue, List<string>? options = null, float mutateRate = 0.1f) 
        : base(name, defaultValue)
    {
        _options = options ?? new List<string> { defaultValue };
        _mutateRate = mutateRate;
        _random = new Random();

        if (!_options.Contains(defaultValue))
        {
            throw new ArgumentException($"Default value {defaultValue} not in options list for {Name}");
        }
    }

    public override void MutateValue(Config config)
    {
        if (_options == null || !_options.Any())
            return;

        if (_random.NextDouble() < _mutateRate)
        {
            Value = _options[_random.Next(_options.Count)];
        }
    }

    public override void Validate()
    {
        if (Value is not string value)
        {
            throw new ArgumentException($"Value {Value} is not a string for {Name}");
        }
        if (!_options.Contains(value))
        {
            throw new ArgumentException($"Value {value} not in options list for {Name}");
        }
    }
}
