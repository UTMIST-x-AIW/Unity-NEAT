using System;
using System.Collections.Generic;
using System.Linq;
using NEAT.Genome;

namespace NEAT.Species
{
    public class Species
    {
        public int Key { get; private set; }
        public List<Genome.Genome> Members { get; private set; }
        public Genome.Genome Representative { get; private set; }
        public double? FitnessHistory { get; private set; }
        public int Age { get; private set; }

        public Species(int key, Genome.Genome representative)
        {
            Key = key;
            Members = new List<Genome.Genome>();
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

        public void AddMember(Genome.Genome genome)
        {
            Members.Add(genome);
        }

        public void RemoveMember(Genome.Genome genome)
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

        public override string ToString()
        {
            return $"Species(key={Key}, members={Members.Count}, fitness_history={FitnessHistory}, age={Age})";
        }
    }
} 