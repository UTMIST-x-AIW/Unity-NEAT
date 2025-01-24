namespace RTNEAT_offline.NEAT.Stagnation;
using System;
using System.Collections.Generic;
using System.Linq;

public class DefaultStagnation
{
    private Func<IEnumerable<double>, double> _speciesFitnessFunc;
    private int _maxStagnation;
    private int _speciesElitism;

    // Constructor to initialize stagnation tracking with config values
    public DefaultStagnation(Func<IEnumerable<double>, double> speciesFitnessFunc, int maxStagnation, int speciesElitism)
    {
        _speciesFitnessFunc = speciesFitnessFunc ?? throw new ArgumentNullException(nameof(speciesFitnessFunc));
        _maxStagnation = maxStagnation;
        _speciesElitism = speciesElitism;
    }

    // The main method to update species fitness and track stagnation
    public List<Tuple<string, Species, bool>> Update(DefaultSpeciesSet speciesSet, int generation)
    {
        var speciesData = new List<Tuple<string, Species>>();
        var result = new List<Tuple<string, Species, bool>>();
        int numNonStagnant = speciesSet.Species.Count;

        // Update species' fitness and track stagnation
        foreach (var speciesPair in speciesSet.Species)
        {
            var species = speciesPair.Value;
            double prevFitness = species.FitnessHistory.Any() ? species.FitnessHistory.Max() : double.MinValue;

            species.Fitness = _speciesFitnessFunc(species.GetFitnesses());
            species.FitnessHistory.Add(species.Fitness);

            if (prevFitness == double.MinValue || species.Fitness > prevFitness)
            {
                species.LastImproved = generation;
            }

            speciesData.Add(new Tuple<string, Species>(speciesPair.Key, species));
        }

        // Sort species by fitness in ascending order
        speciesData = speciesData.OrderBy(x => x.Item2.Fitness).ToList();

        List<double> speciesFitnesses = new List<double>();
        foreach (var (sid, species) in speciesData)
        {
            int stagnantTime = generation - species.LastImproved;
            bool isStagnant = false;

            // Check if species is stagnant
            if (numNonStagnant > _speciesElitism)
            {
                isStagnant = stagnantTime >= _maxStagnation;
            }

            // Ensure we don't remove species below the elitism threshold
            if ((speciesData.Count - speciesData.IndexOf(new Tuple<string, Species>(sid, species))) <= _speciesElitism)
            {
                isStagnant = false;
            }

            if (isStagnant)
            {
                numNonStagnant--;
            }

            result.Add(new Tuple<string, Species, bool>(sid, species, isStagnant));
            speciesFitnesses.Add(species.Fitness);
        }

        return result;
    }
}
