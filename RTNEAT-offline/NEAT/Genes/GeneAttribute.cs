namespace RTNEAT_offline.NEAT
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class GeneAttribute : System.Attribute
    {
        public string Name { get; }
        
        public GeneAttribute(string name)
        {
            Name = name;
        }
    }
} 