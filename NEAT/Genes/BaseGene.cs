using System;

namespace NEAT.Genes
{
    public abstract class BaseGene
    {
        public int Key { get; protected set; }
        public bool Enabled { get; set; }

        protected BaseGene(int key)
        {
            Key = key;
            Enabled = true;
        }

        public abstract BaseGene Clone();

        public virtual double DistanceTo(BaseGene other)
        {
            if (GetType() != other.GetType())
                throw new ArgumentException("Gene distance comparison requires same gene type");
            
            return 0.0; // Base distance calculation
        }

        public override string ToString()
        {
            return $"Gene(key={Key})";
        }
    }
} 
