using System;
using System.Collections.Generic;
using System.Linq;
using RTNEAT_offline.NEAT.Configuration;
using RTNEAT_offline.NEAT.Reporting;
using RTNEAT_offline.NEAT.Species;

namespace RTNEAT_offline.NEAT.Stagnation
{
    public class DefaultStagnation
    {
        private readonly Config config;
        private readonly List<IReporter> reporters;

        public DefaultStagnation(Config config, List<IReporter> reporters)
        {
            this.config = config;
            this.reporters = reporters;
        }

        public Dictionary<int, RTNEAT_offline.NEAT.Species.Species> UpdateSpecies(int generation, Dictionary<int, RTNEAT_offline.NEAT.Species.Species> species)
        {
            reporters.ForEach(r => r.Info($"Updating species {generation}"));

            // Get statistics for each species
            var speciesFitness = new Dictionary<int, double>();
            foreach (var (sid, s) in species)
            {
                if (s.Members.Count > 0)
                {
                    var memberFitness = s.Members.Values.Select(m => m.Fitness ?? 0.0).ToList();
                    var meanFitness = memberFitness.Average();
                    speciesFitness[sid] = meanFitness;
                }
            }

            // No species had a valid fitness
            if (!speciesFitness.Any())
                return species;

            // Compute adjusted fitness
            var adjustedFitness = new Dictionary<int, double>();
            foreach (var (sid, meanFitness) in speciesFitness)
            {
                var adjustedFit = meanFitness / species[sid].Members.Count;
                adjustedFitness[sid] = adjustedFit;
                species[sid].AdjustedFitness = adjustedFit;
            }

            // Update species fitness if improved
            foreach (var (sid, s) in species)
            {
                var previousFitness = s.Fitness;
                var currentFitness = speciesFitness[sid];
                s.Fitness = currentFitness;

                if (previousFitness == null || currentFitness > previousFitness)
                {
                    s.LastImproved = generation;
                }
            }

            return species;
        }
    }
}
