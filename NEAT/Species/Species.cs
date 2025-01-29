using System;
using System.Collections.Generic;
using System.Linq;

namespace NEAT.Species
{
    public class Species
    {
        public int Key { get; private set; }
        public List<NEAT.Genome.Genome> Members { get; private set; }
        public NEAT.Genome.Genome Representative { get; private set; }
        public double? FitnessHistory { get; private set; }
        public int Age { get; private set; }

        public Species(int key, NEAT.Genome.Genome representative)
        {
            Key = key;
            Members = new List<NEAT.Genome.Genome>();
            Representative = representative;
            FitnessHistory = null;
            Age = 0;
            
            if (representative != null)
            {
                Members.Add(representative);
            }
        }

        public void UpdateFitness()
        {
            if (Members.Count == 0) return;

            double speciesFitness = Members.Average(m => m.Fitness ?? 0.0);
            FitnessHistory = speciesFitness;
        }

        public void AddMember(NEAT.Genome.Genome genome)
        {
            Members.Add(genome);
            if (Representative == null || (genome.Fitness.HasValue && Representative.Fitness.HasValue && genome.Fitness.Value > Representative.Fitness.Value))
            {
                Representative = genome;
            }
        }

        public void RemoveMember(NEAT.Genome.Genome genome)
        {
            Members.Remove(genome);
        }

        public void Reset()
        {
            Members.Clear();
            Age++;
        }

        public bool IsStagnant(int stagnationGenerations, double improvementThreshold)
        {
            if (Age < stagnationGenerations || FitnessHistory == null)
                return false;

            var currentFitness = Members.Max(m => m.Fitness ?? 0.0);
            return Math.Abs(currentFitness - FitnessHistory.Value) < improvementThreshold;
        }

        public bool IsCompatible(NEAT.Genome.Genome genome, double compatibilityThreshold)
        {
            if (Representative == null)
                return true;

            var distance = CalculateGenomeDistance(genome, Representative);
            return distance < compatibilityThreshold;
        }

        private double CalculateGenomeDistance(NEAT.Genome.Genome genome1, NEAT.Genome.Genome genome2)
        {
            var disjoint = 0;
            var weightDiff = 0.0;
            var matchingGenes = 0;

            // Compare connection genes
            var genes1 = genome1.Connections.Values.ToDictionary(g => g.Key);
            var genes2 = genome2.Connections.Values.ToDictionary(g => g.Key);

            var allInnovations = genes1.Keys.Union(genes2.Keys);

            foreach (var innovation in allInnovations)
            {
                if (genes1.ContainsKey(innovation) && genes2.ContainsKey(innovation))
                {
                    weightDiff += Math.Abs(genes1[innovation].Weight - genes2[innovation].Weight);
                    matchingGenes++;
                }
                else
                {
                    disjoint++;
                }
            }

            var disjointCoeff = 1.0;
            var weightCoeff = 0.4;

            return (disjointCoeff * disjoint / Math.Max(genome1.Connections.Count, genome2.Connections.Count)) +
                   (weightCoeff * weightDiff / (matchingGenes > 0 ? matchingGenes : 1));
        }

        public override string ToString()
        {
            return $"Species(key={Key}, members={Members.Count}, fitness_history={FitnessHistory}, age={Age})";
        }
    }
} 