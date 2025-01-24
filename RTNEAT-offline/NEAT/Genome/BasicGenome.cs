namespace RTNEAT_offline.NEAT
{
    public class Genome
    {
        public int Key { get; set; }
        public double? Fitness { get; set; }
        
        public Genome(int key)
        {
            Key = key;
            Fitness = null;
        }

        public int Size()
        {
            return 0; // Simplified for testing
        }
    }
} 