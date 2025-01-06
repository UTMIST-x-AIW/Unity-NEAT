using System.Collections.Generic;
using System.Linq;

public class Species
{
    public int Key { get; private set; }
    public int Created { get; private set; }
    public int LastImproved { get; set; }
    public object Representative { get; private set; } // Replace `object` with genome type.
    public Dictionary<int, object> Members { get; private set; } // Replace `object` with genome type.
    public double? Fitness { get; set; }
    public double? AdjustedFitness { get; set; }
    public List<double> FitnessHistory { get; private set; }
//  This is to generate a commit    
    public Species(int key, int generation)
    {
        Key = key;
        Created = generation;
        LastImproved = generation;
        Representative = null;
        Members = new Dictionary<int, object>();
        Fitness = null;
        AdjustedFitness = null;
        FitnessHistory = new List<double>();
    }

    public void Update(object representative, Dictionary<int, object> members)
    {
        Representative = representative;
        Members = members;
    }

    public List<double> GetFitnesses()
    {
        return Members.Values
            .Select(member => ((Genome)member).Fitness) // Replace `Genome` with genome type.
            .ToList();
    }
}

