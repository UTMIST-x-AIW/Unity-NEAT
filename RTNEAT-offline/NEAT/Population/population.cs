using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RTNEAT_offline.NEAT.Reporting;

namespace RTNEAT_offline.NEAT
{
    // Class: Population
    public class CompleteExtinctionException : Exception { }

    public class Population
    {
        private Dictionary<int, Genome> _population;
        private DefaultSpeciesSet _species;
        private Reproduction _reproduction;
        private ReporterSet _reporters;
        private Config _config;
        private Genome _bestGenome;
        private int _generation;

        // NEED TO implement fitness criterion //

        // Initialize Population
        public Population(Config config, (Dictionary<int, Genome>, DefaultSpeciesSet, int)? initialState = null)
        {
            _config = config;
            _reporters = new ReporterSet();
            _reproduction = new Reproduction(config);

            if (initialState == null)
            {
                // Create new population
                _population = _reproduction.CreateNew(config.GenomeType, config.GenomeConfig, config.PopulationSize);
                _species = new DefaultSpeciesSet(config, _reporters.ToList());
                _species.Speciate(_config, _population, _generation);
            }
            else
            {
                // Load initial state
                (_population, _species, _generation) = initialState.Value;
            }
            _bestGenome = null;
        }

        public void AddReporter(IReporter reporter)
        {
            _reporters.Add(reporter);
        }

        public void RemoveReporter(IReporter reporter)
        {
            _reporters.Remove(reporter);
        }

        // run one generation of NEAT
        public Genome Run(Func<Dictionary<int, Genome>, Config, bool> fitnessFunction, int? maxGenerations = null)
        {
            if (_config.NoFitnessTermination && !maxGenerations.HasValue)
            {
                throw new InvalidOperationException("Cannot have no generational limit with no fitness termination");
            }

            _generation = 0;
            while (!maxGenerations.HasValue || _generation < maxGenerations.Value)
            {
                _reporters.StartGeneration(_generation);

                // Evaluate all genomes
                if (!fitnessFunction(_population, _config))
                {
                    _reporters.CompleteExtinction();
                    if (_config.ResetOnExtinction)
                    {
                        _population = _reproduction.CreateNew(_config.GenomeType, _config.GenomeConfig, _config.PopulationSize);
                        _species.Speciate(_config, _population, _generation);
                        continue;
                    }
                    else
                    {
                        throw new CompleteExtinctionException();
                    }
                }

                // Get statistics
                var best = _population.Values.MaxBy(g => g.Fitness);
                _reporters.PostEvaluate(_config, _population, _species, best);

                // Check for success
                if (!_config.NoFitnessTermination && best.Fitness >= _config.FitnessThreshold)
                {
                    _reporters.FoundSolution(_config, _generation, best);
                    return best;
                }

                // Create the next generation
                _population = _reproduction.Reproduce(_config, _population, _species, _generation);
                _species.Speciate(_config, _population, _generation);

                _reporters.EndGeneration(_config, _population, _species);
                _generation++;
            }

            return _population.Values.MaxBy(g => g.Fitness);
        }
    }
}