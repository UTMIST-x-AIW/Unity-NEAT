using RTNEAT_offline.NEAT.Configuration;
using RTNEAT_offline.NEAT.Attributes;

namespace RTNEAT_offline.NEAT.Genes
{
    public abstract class BaseGene
    {
        public object Key { get; private set; }
        protected static List<BaseAttribute> _geneAttributes;

        protected BaseGene(object key)
        {
            Key = key;
        }

        public override string ToString()
        {
            var attribs = new[] { "Key" }.Concat(_geneAttributes.Select(a => a.Name));
            var attribValues = attribs.Select(a => $"{a}={GetType().GetProperty(a)?.GetValue(this)}");
            return $"{GetType().Name}({string.Join(", ", attribValues)})";
        }

        public static bool operator <(BaseGene left, BaseGene right)
        {
            if (left.Key.GetType() != right.Key.GetType())
                throw new ArgumentException($"Cannot compare keys {left.Key} and {right.Key}");
            return Comparer<object>.Default.Compare(left.Key, right.Key) < 0;
        }

        public static bool operator >(BaseGene left, BaseGene right)
        {
            return !(left < right);
        }

        public static void ParseConfig(Config config, Dictionary<string, object> paramDict)
        {
            // Python code empty???
        }

        public static List<object> GetConfigParams()
        {
            if (_geneAttributes == null)
            {
                throw new InvalidOperationException("GeneAttributes is not initialized. Make sure it is set correctly.");
            }

            var paramsList = new List<object>();
            foreach (var attribute in _geneAttributes)
            {
                paramsList.AddRange(attribute.GetConfigParams());
            }

            return paramsList;
        }

        public static void ValidateAttributes(object config)
        {
            if (_geneAttributes == null)
            {
                throw new InvalidOperationException("GeneAttributes is not initialized. Make sure it is set correctly.");
            }

            foreach (var attribute in _geneAttributes)
            {
                attribute.Validate(config);
            }
        }

        public void InitAttributes(Config config)
        {
            foreach (var attr in _geneAttributes)
            {
                attr.InitValue(this, config);
            }
        }

        public void Mutate(Config config)
        {
            foreach (var attr in _geneAttributes)
            {
                attr.MutateValue(this, config);
            }
        }

        public BaseGene Copy()
        {
            var newGene = (BaseGene)Activator.CreateInstance(GetType(), Key);
            foreach (var attr in _geneAttributes)
            {
                var value = GetType().GetProperty(attr.Name)?.GetValue(this);
                GetType().GetProperty(attr.Name)?.SetValue(newGene, value);
            }
            return newGene;
        }

        public BaseGene Crossover(BaseGene gene2)
        {
            if (!Key.Equals(gene2.Key))
                throw new ArgumentException("Gene keys must match for crossover");

            var random = new Random();
            var newGene = (BaseGene)Activator.CreateInstance(GetType(), Key);

            foreach (var attr in _geneAttributes)
            {
                var value = random.NextDouble() > 0.5 ?
                    GetType().GetProperty(attr.Name)?.GetValue(this) :
                    GetType().GetProperty(attr.Name)?.GetValue(gene2);
                GetType().GetProperty(attr.Name)?.SetValue(newGene, value);
            }

            return newGene;
        }
    }
}
