using UnityEngine;

public class SineAgent : RTNEATAgent
{
    private float x = 0f;
    private float stepSize = 0.1f;
    private float currentOutput;

    protected override float[] GetEnvironmentInputs()
    {
        return new float[] { x };
    }

    protected override void ProcessOutputs(float[] outputs)
    {
        currentOutput = outputs[0];
        x += stepSize;
        if (x > 2 * Mathf.PI)
        {
            x = 0f;
        }
    }

    protected override float CalculateFitness()
    {
        float expectedSine = Mathf.Sin(x);
        float error = Mathf.Abs(expectedSine - currentOutput);
        return 1f / (1f + error); // Higher fitness for lower error
    }

    private void OnDrawGizmos()
    {
        // Visualize the agent's output vs actual sine
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(x, currentOutput, 0), 0.1f);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(new Vector3(x, Mathf.Sin(x), 0), 0.1f);
    }
}
