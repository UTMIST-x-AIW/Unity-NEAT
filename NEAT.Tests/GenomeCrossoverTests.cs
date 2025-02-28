using System;
using System.Linq;
using NEAT.Genes;
using NEAT.Genome;
using Xunit;

namespace NEAT.Tests
{
    public class GenomeCrossoverTests
    {
        private Genome.Genome CreateTestGenome(int key, double fitness, (int fromNode, int toNode, double weight, bool enabled)[] connections)
        {
            var genome = new Genome.Genome(key);
            genome.Fitness = fitness;

            // Add all nodes first
            var allNodes = connections
                .SelectMany(c => new[] { c.fromNode, c.toNode })
                .Distinct()
                .OrderBy(n => n);

            foreach (var nodeKey in allNodes)
            {
                genome.AddNode(new NodeGene(nodeKey, NodeType.Hidden));
            }

            // Add connections
            int connKey = 1;
            foreach (var conn in connections)
            {
                var connectionGene = new ConnectionGene(connKey++, conn.fromNode, conn.toNode, conn.weight);
                connectionGene.Enabled = conn.enabled;
                genome.AddConnection(connectionGene);
            }

            return genome;
        }

        [Fact]
        public void TestCrossover_MoreFitParentDominant()
        {
            // Arrange
            var parent1 = CreateTestGenome(1, 10.0, new[]
            {
                (1, 2, 0.5, true),
                (2, 3, 0.7, true),
                (1, 3, 0.3, true)
            });

            var parent2 = CreateTestGenome(2, 5.0, new[]
            {
                (1, 2, -0.5, true),
                (2, 4, 0.9, true)  // Disjoint connection
            });

            // Act
            var child = parent1.Crossover(parent2, 3);

            // Assert
            Assert.NotNull(child);
            Assert.Equal(3, child.Key);
            
            // Should have all nodes from more fit parent
            Assert.Equal(parent1.Nodes.Count, child.Nodes.Count);
            Assert.All(parent1.Nodes.Keys, key => Assert.Contains(key, child.Nodes.Keys));

            // Should have all connections from more fit parent
            Assert.Equal(parent1.Connections.Count, child.Connections.Count);
            Assert.All(parent1.Connections.Keys, key => Assert.Contains(key, child.Connections.Keys));
        }

        [Fact]
        public void TestCrossover_MatchingGenesRandomlyInherited()
        {
            // Arrange
            var parent1 = CreateTestGenome(1, 10.0, new[]
            {
                (1, 2, 0.5, true)
            });

            var parent2 = CreateTestGenome(2, 10.0, new[]
            {
                (1, 2, -0.5, true)
            });

            // Act - Run multiple crossovers to test randomness
            var weights = new double[100];
            for (int i = 0; i < 100; i++)
            {
                var child = parent1.Crossover(parent2, i + 3);
                weights[i] = child.Connections[1].Weight;
            }

            // Assert - Should see both weights represented
            Assert.Contains(0.5, weights);
            Assert.Contains(-0.5, weights);
        }

        [Fact]
        public void TestCrossover_DisabledGenesInherited()
        {
            // Arrange
            var parent1 = CreateTestGenome(1, 10.0, new[]
            {
                (1, 2, 0.5, false),  // Disabled connection
                (2, 3, 0.7, true)
            });

            var parent2 = CreateTestGenome(2, 5.0, new[]
            {
                (1, 2, 0.5, true),   // Enabled connection
                (2, 3, 0.7, true)
            });

            // Act
            var child = parent1.Crossover(parent2, 3);

            // Assert
            Assert.False(child.Connections[1].Enabled);  // Should inherit disabled state from more fit parent
        }
    }
} 