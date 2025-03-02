public static class SineNEATConfig
{
    public static NEATConfiguration GetSineConfig()
    {
        return new NEATConfiguration
        {
            InputNodes = 1,  // x value
            OutputNodes = 1, // sine approximation
            InitialPopulation = 50,
            MutationRate = 0.3f,
            CrossoverRate = 0.75f,
            WeightMutationRange = 0.5f,
            BiasRange = (-1f, 1f),
            ActivationFunction = ActivationFunction.Tanh
        };
    }
}
