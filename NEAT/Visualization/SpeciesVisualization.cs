using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEAT.Visualization
{
    public class SpeciesVisualization
    {
        private readonly List<SpeciesSnapshot> _history;
        private int _maxSpeciesId;

        public SpeciesVisualization()
        {
            _history = new List<SpeciesSnapshot>();
            _maxSpeciesId = -1;
        }

        public void RecordGeneration(int generation, List<NEAT.Species.Species> species)
        {
            var snapshot = new SpeciesSnapshot
            {
                Generation = generation,
                SpeciesInfo = species.Select(s => new SpeciesInfo
                {
                    SpeciesId = s.Key,
                    Size = s.Members.Count,
                    AverageFitness = s.Members.Average(m => m.Fitness ?? 0.0),
                    BestFitness = s.Members.Max(m => m.Fitness ?? 0.0),
                    Representative = new GenomeInfo
                    {
                        NodeCount = s.Representative.Nodes.Count,
                        ConnectionCount = s.Representative.Connections.Count
                    }
                }).ToList()
            };

            _maxSpeciesId = Math.Max(_maxSpeciesId, species.Max(s => s.Key));
            _history.Add(snapshot);
        }

        public void PrintHistory()
        {
            Console.WriteLine("\nSpecies Evolution History:");
            Console.WriteLine("=========================\n");

            foreach (var snapshot in _history)
            {
                Console.WriteLine($"Generation {snapshot.Generation}:");
                Console.WriteLine("------------------");

                // Print species distribution
                var distribution = new string[_maxSpeciesId + 1];
                for (int i = 0; i <= _maxSpeciesId; i++)
                {
                    var species = snapshot.SpeciesInfo.FirstOrDefault(s => s.SpeciesId == i);
                    distribution[i] = species != null ? $"S{i}({species.Size})" : "    ";
                }
                Console.WriteLine(string.Join(" | ", distribution));

                // Print detailed species information
                foreach (var species in snapshot.SpeciesInfo.OrderBy(s => s.SpeciesId))
                {
                    Console.WriteLine($"Species {species.SpeciesId}:");
                    Console.WriteLine($"  Size: {species.Size}");
                    Console.WriteLine($"  Avg Fitness: {species.AverageFitness:F3}");
                    Console.WriteLine($"  Best Fitness: {species.BestFitness:F3}");
                    Console.WriteLine($"  Structure: {species.Representative.NodeCount} nodes, {species.Representative.ConnectionCount} connections");
                }
                Console.WriteLine();
            }
        }

        private class SpeciesSnapshot
        {
            public int Generation { get; set; }
            public List<SpeciesInfo> SpeciesInfo { get; set; }
        }

        private class SpeciesInfo
        {
            public int SpeciesId { get; set; }
            public int Size { get; set; }
            public double AverageFitness { get; set; }
            public double BestFitness { get; set; }
            public GenomeInfo Representative { get; set; }
        }

        private class GenomeInfo
        {
            public int NodeCount { get; set; }
            public int ConnectionCount { get; set; }
        }
    }
} 