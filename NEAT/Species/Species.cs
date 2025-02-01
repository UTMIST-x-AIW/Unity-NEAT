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
            var oldRepresentative = Representative;
            Members.Clear();
            Age++;
            if (oldRepresentative != null)
            {
                Members.Add(oldRepresentative);
                Representative = oldRepresentative;
            }
        }

        public bool IsStagnant(int stagnationGenerations, double improvementThreshold)
        {
            if (Age < stagnationGenerations || FitnessHistory == null)
                return false;

            var currentFitness = Members.Max(m => m.Fitness ?? 0.0);
            return Math.Abs(currentFitness - FitnessHistory.Value) < improvementThreshold;
        }

        public bool IsCompatible(NEAT.Genome.Genome genome, double compatibilityThreshold, double disjointCoefficient, double weightCoefficient)
        {
            if (Representative == null)
                return Members.Count == 0;  // Only allow joining if this is a new species

            var distance = genome.CalculateGenomeDistance(Representative, disjointCoefficient, weightCoefficient);
            return distance < compatibilityThreshold;
        }

        public override string ToString()
        {
            return $"Species(key={Key}, members={Members.Count}, fitness_history={FitnessHistory}, age={Age})";
        }
    }
} 