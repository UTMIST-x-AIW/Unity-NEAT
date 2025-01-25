using System;
using System.Collections.Generic;

namespace RTNEAT_offline.NEAT.Aggregation
{
    public class AggregationFunctionSet
    {
        private readonly Dictionary<string, Func<IEnumerable<double>, double>> _functions;

        public AggregationFunctionSet()
        {
            _functions = new Dictionary<string, Func<IEnumerable<double>, double>>();
        }

        public void Add(string name, Func<IEnumerable<double>, double> function)
        {
            _functions[name] = function;
        }

        public Func<IEnumerable<double>, double> Get(string name)
        {
            if (_functions.TryGetValue(name, out var function))
            {
                return function;
            }
            throw new KeyNotFoundException($"Aggregation function '{name}' not found");
        }
    }
} 