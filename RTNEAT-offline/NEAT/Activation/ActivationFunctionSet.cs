using System;
using System.Collections.Generic;

namespace RTNEAT_offline.NEAT.Activation
{
    public class ActivationFunctionSet
    {
        private readonly Dictionary<string, Func<double, double>> _functions;

        public ActivationFunctionSet()
        {
            _functions = new Dictionary<string, Func<double, double>>();
        }

        public void Add(string name, Func<double, double> function)
        {
            _functions[name] = function;
        }

        public Func<double, double> Get(string name)
        {
            if (_functions.TryGetValue(name, out var function))
            {
                return function;
            }
            throw new KeyNotFoundException($"Activation function '{name}' not found");
        }
    }
} 