using UnityEngine;

public class SineExampleSetup : MonoBehaviour
{
    void Awake()
    {
        // Set up NEAT configuration
        NEATConfiguration.Current = SineNEATConfig.GetSineConfig();
    }
}
