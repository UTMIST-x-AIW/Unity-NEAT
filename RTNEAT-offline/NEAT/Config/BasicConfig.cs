namespace RTNEAT_offline.NEAT
{
    public class Config
    {
        // Basic configuration properties
        public double CompatibilityThreshold { get; set; } = 3.0;
        public int PopulationSize { get; set; } = 10;
        public double FitnessThreshold { get; set; } = 0.9;
        public bool NoFitnessTermination { get; set; } = false;
        public bool ResetOnExtinction { get; set; } = true;
        
        // These will be needed for reproduction
        public Type GenomeType { get; set; }
        public object GenomeConfig { get; set; }
    }
} 