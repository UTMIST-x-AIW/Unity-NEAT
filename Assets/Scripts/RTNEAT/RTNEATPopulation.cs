using System.Collections.Generic;
using System.Linq;

public class RTNEATPopulation : NEATPopulation
{
    private List<Genome> activeGenomes;
    private float minLifespan = 10f;
    
    public RTNEATPopulation(int populationSize) : base(populationSize)
    {
        activeGenomes = new List<Genome>();
    }

    public void ReplaceIndividual(int index, Genome newGenome)
    {
        if (index >= 0 && index < Genomes.Count)
        {
            Genomes[index] = newGenome;
        }
    }

    public Genome GetRandomTopPerformer()
    {
        // Get top 20% of population
        int topCount = Genomes.Count / 5;
        var topGenomes = Genomes.Take(topCount).ToList();
        
        // Return random genome from top performers
        return topGenomes[Random.Range(0, topCount)];
    }

    public void SortByFitness()
    {
        Genomes.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));
    }
}
