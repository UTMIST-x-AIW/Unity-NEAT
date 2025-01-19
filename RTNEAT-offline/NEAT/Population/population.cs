using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RTNEATOffline.NEAT.Population
{
    // Class: Population
public class CompleteExtinctionException : Exception { }

public class Population(/*object*/){   
    private Dictionary<int, Genome> _population;
    private SpeciesSet _species;
    private Reproduction _reproduction;
    private ReporterSet _reporters;
    private Config _config;
    private Genome _bestGenome;
    private int _generation;

    // NEED TO implement fitness criterion //

    // Initialize Population
    public Population(
        PopulationConfigs config,
        /*(Dictionary<int, Genome>, SpeciesSet, int)?*/
        initialState = null)
    {
        _config = config;
        _reporter = new ReporterSet();
        //stagnation//
        _reproduction = new Reproduction(config.ReproductionConfig, _reporters, _stagnation, 0, new Dictionary<int, (int, int)>());
        
        // if no initial state, create new population
        if(initialState == null){
            _population = _reproduction.CreateNew(config.GenomeType, config.GenomeConfig, config.PopulationSize);
            _species = new SpeciesSet(config.SpeciesSetConfig, _config.PopulationSize);
            _species.Speciate(_population, _config);
        }
        else{
            // load initial state if provided
            (_population, _species) = initialState.Value;

            // omitted generation from Speciate() and above load initial state
        }
        bestGenome = null;

        // add reporters in Population
        public void AddReporter(reporter){
            _reporters.Add(reporter);
        }

        // remove reporters in Population
        public void RemoveReporter(reporter){
            _reporters.Remove(reporter);
        }

        // run one generation of NEAT
        public void Run(FitnessFunction, int maxGenerations = null){
            // 1. Run NEAT's genetic algorithm for at most <maxGeneration> generations
            // 2. If n is null, run until fitness threshold is reached or extinction
            // 3. Generate next generation
            // 4. need to implement continuous (real-time) speciation
            // 5. go to step 1

            // self
            public Run()
            {
                // If there is no termination condition present, throw error
                if (_config.NoFitnessTermination && maxGenerations == null)
                {
                    throw new InvalidOperationException("Cannot have no generational limit with no fitness termination.");
                }

                // generationCounter 
                int generationCounter = 0;

                // Run NEAT's genetic algorithm for at most <maxGeneration> generations
                while(!maxGeneration.HasValue || generationCounter < maxGenerations)
                {
                    generationCounter ++;
                    _reporters.StartGeneration(_generation);

                    // evaluate all genomes using provided FitnessFunction
                    FitnessFunction(_population.ToList(), _config);

                    // report statistics
                    Genome best = null;
                    foreach(var genome in _population.Values)
                    {
                        if !genome.Fitness.HasValue{
                            throw new InvalidOperationException($"Genome {genome.ID} has no fitness value.");
                        }

                        if best == null || genome.Fitness > best.Fitness
                        {
                            best = genome;
                        }
                        // notify reporters of the completion of fitness evaluation for generation
                         _reporters.PostEvaluate(_config, _population, _species, best);


                        // if exit condition is not met, run reproduction
                        if !_config.NoFitnessTermination && best.Fitness >= _config.FitnessThreshold
                        {
                            _reproduction.Reproduce(_config, _population, _species, _generation);
                            if reset_on_extinction
                            {
                                if _species.ToList().Count == 0
                                {
                                    throw new CompleteExtinctionException();
                                }
                                else
                                {
                                    _reproduction.CreateNew(_config.GenomeType, _config.GenomeConfig, _config.PopulationSize - _species.ToList().Count);
                                }
                            }

                            _species.Speciate(_population, _config); 

                            var bestFitness = _population.Values.Max(g => g.Fitness);
                            if (bestFitness >= _config.FitnessThreshold)
                            {
                                _reporters.ReportFoundSolution(_generation, bestGenome);
                                return bestGenome;
                            }

                            _generation++;
                            _reporters.ReportEndGeneration(_generation, _population, _species);
                        }
                    }
                }
            }


        }


    }
}

}