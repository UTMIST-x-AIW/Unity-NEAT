namespace RTNEAT_offline.NEAT.Reporting
{
    public interface IReporter
    {
        void StartGeneration(int generation);
        void EndGeneration(Config config, Dictionary<int, Genome> population, DefaultSpeciesSet species);
        void PostEvaluate(Config config, Dictionary<int, Genome> population, DefaultSpeciesSet species, Genome bestGenome);
        void CompleteExtinction();
        void FoundSolution(Config config, int generation, Genome best);
        void SpeciesStagnant(int sid, Species species);
        void Info(string msg);
        void PostReproduction(Config config, Dictionary<int, Genome> population, DefaultSpeciesSet species);
    }
} 