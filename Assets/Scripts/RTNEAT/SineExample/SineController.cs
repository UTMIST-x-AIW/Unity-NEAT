using UnityEngine;
using System.Collections.Generic;

public class SineController : RTNEATController
{
    public GameObject agentPrefab;
    private List<SineAgent> agents = new List<SineAgent>();

    protected override void InitializeRTNEAT()
    {
        base.InitializeRTNEAT();
        
        // Create initial population of sine agents
        for (int i = 0; i < populationSize; i++)
        {
            GameObject agentObj = Instantiate(agentPrefab, Vector3.right * i, Quaternion.identity);
            SineAgent agent = agentObj.GetComponent<SineAgent>();
            agent.Initialize(population.Genomes[i]);
            agents.Add(agent);
        }
    }

    protected override void OnNewGenome(int index, Genome genome)
    {
        // Update existing agent with new genome
        if (index < agents.Count)
        {
            agents[index].Initialize(genome);
        }
    }

    private void OnDrawGizmos()
    {
        // Draw actual sine wave for reference
        Gizmos.color = Color.green;
        for (float x = 0; x < 2 * Mathf.PI; x += 0.1f)
        {
            Gizmos.DrawLine(
                new Vector3(x, Mathf.Sin(x), 0),
                new Vector3(x + 0.1f, Mathf.Sin(x + 0.1f), 0)
            );
        }
    }
}
