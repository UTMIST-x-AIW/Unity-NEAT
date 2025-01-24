using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RTNEAT_offline.NEAT
{
    public class Species
    {
        public int Id { get; }
        public Dictionary<int, object> Members { get; private set; }
        public int Created { get; }
        public double? Fitness { get; set; }
        public double? AdjustedFitness { get; set; }
        public int LastImproved { get; set; }

        public Species(int id, int generation)
        {
            Id = id;
            Created = generation;
            Members = new Dictionary<int, object>();
            LastImproved = generation;
        }

        public void Update(Genome representative, Dictionary<int, object> members)
        {
            Members = members;
        }

        public List<double> GetFitnesses()
        {
            return Members.Values
                .Select(member => ((Genome)member).Fitness) // Replace `Genome` with genome type.
                .ToList();
        }
    }
}

