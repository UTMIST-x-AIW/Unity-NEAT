using System;
using System.Collections.Generic;
using RTNEAT_offline.NEAT.Genome;

namespace RTNEAT_offline.NEAT.Species
{
    public class Species
    {
        public int Id { get; private set; }
        public int Created { get; private set; }
        public DefaultGenome? Representative { get; private set; }
        public Dictionary<int, DefaultGenome> Members { get; private set; }
        public double? Fitness { get; set; }
        public double? AdjustedFitness { get; set; }
        public int LastImproved { get; set; }

        public Species(int id, int generation)
        {
            Id = id;
            Created = generation;
            Representative = null;
            Members = new Dictionary<int, DefaultGenome>();
            Fitness = null;
            AdjustedFitness = null;
            LastImproved = generation;
        }

        public void Update(DefaultGenome representative, Dictionary<int, DefaultGenome> members)
        {
            Representative = representative;
            Members = members;
        }
    }
} 