using System;
using System.Collections.Generic;
using System.Linq;
using NEAT.Config;
using NEAT.Species;
using NEAT.Genes;

namespace NEAT
{
    public class Population
    {
        private readonly Config.Config _config;
        private readonly Dictionary<int, Genome.Genome> _population;
        private readonly Dictionary<int, Species.Species> _species;
        private int _generation;
        private readonly Random _random;

        public Population(Config.Config config)
        {
            _config = config;
            _population = new Dictionary<int, Genome.Genome>();
            _species = new Dictionary<int, Species.Species>();
            _generation = 0;
            _random = new Random();

            CreateInitialPopulation();
        }

        private void CreateInitialPopulation()
        {
            int populationSize = _config.GetParameter("population_size", 150);

            for (int i = 0; i < populationSize; i++)
            {
                var genome = CreateInitialGenome(i);
                _population[i] = genome;
            }

            SpeciatePopulation();
        }

        private Genome.Genome CreateInitialGenome(int key)
        {
            var genome = new Genome.Genome(key);
            
            // Add input nodes
            int numInputs = _config.GetParameter("num_inputs", 2);
            for (int i = 0; i < numInputs; i++)
            {
                genome.AddNode(new NodeGene(i, NodeType.Input));
            }

            // Add output nodes
            int numOutputs = _config.GetParameter("num_outputs", 1);
            for (int i = 0; i < numOutputs; i++)
            {
                genome.AddNode(new NodeGene(numInputs + i, NodeType.Output));
            }

            // Add initial connections
            int connectionKey = 0;
            for (int input = 0; input < numInputs; input++)
            {
                for (int output = 0; output < numOutputs; output++)
                {
                    var weight = (_random.NextDouble() * 4) - 2; // Random weight between -2 and 2
                    genome.AddConnection(new ConnectionGene(connectionKey++, input, numInputs + output, weight));
                }
            }

            return genome;
        }

        private void SpeciatePopulation()
        {
            // Reset all species
            foreach (var species in _species.Values)
            {
                species.Reset();
            }

            // Assign genomes to species
            foreach (var genome in _population.Values)
            {
                bool foundSpecies = false;
                foreach (var species in _species.Values)
                {
                    if (species.Representative == null) continue;

                    double distance = genome.CalculateGenomeDistance(
                        species.Representative,
                        _config.GetParameter("disjoint_coefficient", 1.0),
                        _config.GetParameter("weight_coefficient", 0.5));

                    if (distance < _config.GetParameter("compatibility_threshold", 3.0))
                    {
                        species.AddMember(genome);
                        foundSpecies = true;
                        break;
                    }
                }

                if (!foundSpecies)
                {
                    // Create new species
                    var newSpecies = new Species.Species(_species.Count, genome);
                    _species[newSpecies.Key] = newSpecies;
                }
            }

            // Remove empty species
            var emptySpecies = _species.Values.Where(s => s.Members.Count == 0).ToList();
            foreach (var species in emptySpecies)
            {
                _species.Remove(species.Key);
            }
        }

        public void Evolve(Action<Genome.Genome> fitnessFunction)
        {
            // Evaluate all genomes
            foreach (var genome in _population.Values)
            {
                fitnessFunction(genome);
            }

            // Update species fitness
            foreach (var species in _species.Values)
            {
                species.UpdateFitness();
            }

            // Create next generation
            var newPopulation = new Dictionary<int, Genome.Genome>();
            int offspring = 0;

            // Elitism: keep best performing genome from each species
            foreach (var species in _species.Values)
            {
                if (species.Members.Count > 0)
                {
                    var best = species.Members.OrderByDescending(m => m.Fitness).First();
                    newPopulation[offspring++] = best.Clone(offspring);
                }
            }

            // Fill the rest of the population with offspring
            while (offspring < _population.Count)
            {
                var species = SelectSpecies();
                if (species == null) break;

                var child = CreateOffspring(species);
                if (child != null)
                {
                    newPopulation[offspring] = child;
                    offspring++;
                }
            }

            _population.Clear();
            foreach (var genome in newPopulation)
            {
                _population[genome.Key] = genome.Value;
            }

            SpeciatePopulation();
            _generation++;
        }

        private Species.Species SelectSpecies()
        {
            double totalAdjustedFitness = _species.Values.Sum(s => s.Members.Sum(m => m.Fitness ?? 0.0) / s.Members.Count);
            double dart = _random.NextDouble() * totalAdjustedFitness;
            double sum = 0;

            foreach (var species in _species.Values)
            {
                sum += species.Members.Sum(m => m.Fitness ?? 0.0) / species.Members.Count;
                if (sum > dart)
                {
                    return species;
                }
            }

            return _species.Values.FirstOrDefault();
        }

        private Genome.Genome CreateOffspring(Species.Species species)
        {
            if (species.Members.Count == 0) return null;

            var parent1 = species.Members[_random.Next(species.Members.Count)];
            var parent2 = species.Members[_random.Next(species.Members.Count)];

            // Create child through crossover
            var child = parent1.Fitness > parent2.Fitness
                ? parent1.Clone(_population.Count)
                : parent2.Clone(_population.Count);

            // Apply mutations
            MutateGenome(child);

            return child;
        }

        private void MutateGenome(Genome.Genome genome)
        {
            // Weight mutation
            double weightMutationRate = _config.GetParameter("weight_mutation_rate", 0.8);
            double mutationPower = _config.GetParameter("mutation_power", 2.5);

            foreach (var conn in genome.Connections.Values)
            {
                if (_random.NextDouble() < weightMutationRate)
                {
                    // Either perturb the weight or assign a new random weight
                    if (_random.NextDouble() < 0.9)
                    {
                        conn.Weight += (_random.NextDouble() * 2 - 1) * mutationPower;
                    }
                    else
                    {
                        conn.Weight = (_random.NextDouble() * 4) - 2;
                    }
                }
            }

            // Add node mutation
            if (_random.NextDouble() < _config.GetParameter("node_add_prob", 0.2))
            {
                if (genome.Connections.Count > 0)
                {
                    // Choose a random connection to split
                    var conn = genome.Connections.Values.ElementAt(_random.Next(genome.Connections.Count));
                    conn.Enabled = false;

                    // Create new node
                    var newNodeKey = genome.Nodes.Count;
                    var newNode = new NodeGene(newNodeKey, NodeType.Hidden);
                    genome.AddNode(newNode);

                    // Create two new connections
                    var newConn1 = new ConnectionGene(
                        genome.Connections.Count,
                        conn.InputKey,
                        newNodeKey,
                        1.0);

                    var newConn2 = new ConnectionGene(
                        genome.Connections.Count + 1,
                        newNodeKey,
                        conn.OutputKey,
                        conn.Weight);

                    genome.AddConnection(newConn1);
                    genome.AddConnection(newConn2);
                }
            }

            // Add connection mutation
            if (_random.NextDouble() < _config.GetParameter("conn_add_prob", 0.5))
            {
                // Try several times to find an unconnected pair of nodes
                for (int tries = 0; tries < 20; tries++)
                {
                    var sourceKey = genome.Nodes.Keys.ElementAt(_random.Next(genome.Nodes.Count));
                    var targetKey = genome.Nodes.Keys.ElementAt(_random.Next(genome.Nodes.Count));

                    // Don't connect input to input, output to output, or create cycles
                    if (genome.Nodes[sourceKey].Type == NodeType.Output ||
                        genome.Nodes[targetKey].Type == NodeType.Input ||
                        sourceKey == targetKey)
                    {
                        continue;
                    }

                    // Check if connection already exists
                    bool exists = genome.Connections.Values.Any(c =>
                        c.InputKey == sourceKey && c.OutputKey == targetKey);

                    if (!exists)
                    {
                        var newConn = new ConnectionGene(
                            genome.Connections.Count,
                            sourceKey,
                            targetKey,
                            (_random.NextDouble() * 4) - 2);

                        genome.AddConnection(newConn);
                        break;
                    }
                }
            }

            // Delete connection mutation
            if (_random.NextDouble() < _config.GetParameter("conn_delete_prob", 0.5))
            {
                if (genome.Connections.Count > 1) // Keep at least one connection
                {
                    var connToDelete = genome.Connections.Values.ElementAt(_random.Next(genome.Connections.Count));
                    genome.Connections.Remove(connToDelete.Key);
                }
            }
        }

        public Genome.Genome GetBestGenome()
        {
            return _population.Values.OrderByDescending(g => g.Fitness).First();
        }
    }
} 