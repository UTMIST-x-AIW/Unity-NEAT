namespace RTNEAT_offline.NEAT
{
    public class Genome
    {
        public int Key { get; set; }
        public double Fitness { get; set; }
        public virtual int Size() => 0;

        public Genome(int key)
        {
            Key = key;
            Fitness = 0.0;
        }
    }
} 