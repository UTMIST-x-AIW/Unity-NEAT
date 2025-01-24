using System;
using System.Collections.Generic;
using System.Linq;

namespace RTNEAT_offline.NEAT.Reporting
{
    public class ReporterSet
    {
        private readonly List<IReporter> _reporters;

        public ReporterSet()
        {
            _reporters = new List<IReporter>();
        }

        public void Add(IReporter reporter)
        {
            _reporters.Add(reporter);
        }

        public void Remove(IReporter reporter)
        {
            _reporters.Remove(reporter);
        }

        public void StartGeneration(int gen)
        {
            foreach (var reporter in _reporters)
            {
                reporter.StartGeneration(gen);
            }
        }

        public void EndGeneration(Config config, Dictionary<int, Genome> population, DefaultSpeciesSet speciesSet)
        {
            foreach (var reporter in _reporters)
            {
                reporter.EndGeneration(config, population, speciesSet);
            }
        }

        public void PostEvaluate(Config config, Dictionary<int, Genome> population, DefaultSpeciesSet species, Genome bestGenome)
        {
            foreach (var reporter in _reporters)
            {
                reporter.PostEvaluate(config, population, species, bestGenome);
            }
        }

        public void PostReproduction(Config config, Dictionary<int, Genome> population, DefaultSpeciesSet species)
        {
            foreach (var reporter in _reporters)
            {
                reporter.PostReproduction(config, population, species);
            }
        }

        public void CompleteExtinction()
        {
            foreach (var reporter in _reporters)
            {
                reporter.CompleteExtinction();
            }
        }

        public void FoundSolution(Config config, int generation, Genome best)
        {
            foreach (var reporter in _reporters)
            {
                reporter.FoundSolution(config, generation, best);
            }
        }

        public void SpeciesStagnant(int sid, Species species)
        {
            foreach (var reporter in _reporters)
            {
                reporter.SpeciesStagnant(sid, species);
            }
        }

        public void Info(string msg)
        {
            foreach (var reporter in _reporters)
            {
                reporter.Info(msg);
            }
        }
    }

    public interface IReporter
    {
        void StartGeneration(int generation);
        void EndGeneration(Config config, Dictionary<int, Genome> population, DefaultSpeciesSet speciesSet);
        void PostEvaluate(Config config, Dictionary<int, Genome> population, DefaultSpeciesSet species, Genome bestGenome);
        void PostReproduction(Config config, Dictionary<int, Genome> population, DefaultSpeciesSet species);
        void CompleteExtinction();
        void FoundSolution(Config config, int generation, Genome best);
        void SpeciesStagnant(int sid, Species species);
        void Info(string msg);
    }

    public class ConsoleReporter : IReporter
    {
        private readonly bool _showSpeciesDetail;
        private int _generation;
        private DateTime _generationStartTime;
        private readonly List<double> _generationTimes;
        private int _numExtinctions;

        public ConsoleReporter(bool showSpeciesDetail)
        {
            _showSpeciesDetail = showSpeciesDetail;
            _generationTimes = new List<double>();
            _numExtinctions = 0;
        }

        public void StartGeneration(int generation)
        {
            _generation = generation;
            Console.WriteLine($"\n ****** Running generation {generation} ****** \n");
            _generationStartTime = DateTime.Now;
        }

        public void EndGeneration(Config config, Dictionary<int, Genome> population, DefaultSpeciesSet speciesSet)
        {
            var ng = population.Count;
            var ns = speciesSet.Species.Count;

            if (_showSpeciesDetail)
            {
                Console.WriteLine($"Population of {ng} members in {ns} species:");
                Console.WriteLine("   ID   age  size   fitness   adj fit  stag");
                Console.WriteLine("  ====  ===  ====  =========  =======  ====");
                
                foreach (var sid in speciesSet.Species.Keys.OrderBy(k => k))
                {
                    var s = speciesSet.Species[sid];
                    var age = _generation - s.Created;
                    var size = s.Members.Count;
                    var fitness = s.Fitness.HasValue ? $"{s.Fitness:F3}" : "--";
                    var adjFitness = s.AdjustedFitness.HasValue ? $"{s.AdjustedFitness:F3}" : "--";
                    var stagnation = _generation - s.LastImproved;
                    Console.WriteLine($"  {sid,4}  {age,3}  {size,4}  {fitness,9}  {adjFitness,7}  {stagnation,4}");
                }
            }
            else
            {
                Console.WriteLine($"Population of {ng} members in {ns} species");
            }

            var elapsed = (DateTime.Now - _generationStartTime).TotalSeconds;
            _generationTimes.Add(elapsed);
            if (_generationTimes.Count > 10)
                _generationTimes.RemoveAt(0);

            var average = _generationTimes.Average();
            Console.WriteLine($"Total extinctions: {_numExtinctions}");

            if (_generationTimes.Count > 1)
                Console.WriteLine($"Generation time: {elapsed:F3} sec ({average:F3} average)");
            else
                Console.WriteLine($"Generation time: {elapsed:F3} sec");
        }

        public void PostEvaluate(Config config, Dictionary<int, Genome> population, DefaultSpeciesSet species, Genome bestGenome)
        {
            var fitnesses = population.Values.Select(c => c.Fitness.Value).ToList();
            var fitMean = fitnesses.Average();
            var fitStd = Math.Sqrt(fitnesses.Select(x => Math.Pow(x - fitMean, 2)).Average());
            var bestSpeciesId = species.GenomeToSpecies[bestGenome.Key];

            Console.WriteLine($"Population's average fitness: {fitMean:F5} stdev: {fitStd:F5}");
            Console.WriteLine($"Best fitness: {bestGenome.Fitness:F5} - size: {bestGenome.Size()} - species {bestSpeciesId} - id {bestGenome.Key}");
        }

        public void CompleteExtinction()
        {
            _numExtinctions++;
            Console.WriteLine("All species extinct.");
        }

        public void FoundSolution(Config config, int generation, Genome best)
        {
            Console.WriteLine($"\nBest individual in generation {_generation} meets fitness threshold - complexity: {best.Size()}");
        }

        public void SpeciesStagnant(int sid, Species species)
        {
            if (_showSpeciesDetail)
            {
                Console.WriteLine($"\nSpecies {sid} with {species.Members.Count} members is stagnated: removing it");
            }
        }

        public void Info(string msg)
        {
            Console.WriteLine(msg);
        }

        public void PostReproduction(Config config, Dictionary<int, Genome> population, DefaultSpeciesSet species)
        {
            // Default empty implementation
        }
    }
}
