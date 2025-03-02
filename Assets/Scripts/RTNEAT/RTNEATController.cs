using UnityEngine;
using System.Collections.Generic;

public class RTNEATController : MonoBehaviour
{
    private NEATPopulation population;
    private float evaluationTimer;
    public float evaluationInterval = 5f; // Time between evaluations
    public int populationSize = 50;
    public float survivalThreshold = 0.2f; // Bottom 20% get replaced

    private void Start()
    {
        InitializeRTNEAT();
    }

    private void InitializeRTNEAT()
    {
        population = new NEATPopulation(populationSize);
        population.Initialize();
        evaluationTimer = 0f;
    }

    private void Update()
    {
        evaluationTimer += Time.deltaTime;
        
        if (evaluationTimer >= evaluationInterval)
        {
            PerformRealTimeEvolution();
            evaluationTimer = 0f;
        }
    }

    private void PerformRealTimeEvolution()
    {
        // Sort population by fitness
        population.SortByFitness();

        // Replace worst performing individuals
        int replacementCount = Mathf.RoundToInt(populationSize * survivalThreshold);
        for (int i = 0; i < replacementCount; i++)
        {
            // Get parents from top performing individuals
            var parent1 = population.GetRandomTopPerformer();
            var parent2 = population.GetRandomTopPerformer();

            // Create offspring
            var offspring = population.CreateOffspring(parent1, parent2);
            
            // Replace worst performing individual
            population.ReplaceIndividual(populationSize - 1 - i, offspring);
        }

        // Adjust species
        population.AdjustSpecies();
    }
}
