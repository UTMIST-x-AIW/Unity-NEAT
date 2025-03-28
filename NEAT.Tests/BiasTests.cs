 using System;
using System.Linq;
using System.Collections.Generic;
using NEAT.Genes;
using NEAT.Genome;
using NEAT.NN;
using Xunit;
using NEAT.Config;

namespace NEAT.Tests
{
    public class BiasTests
    {
        // Helper method to create a simple neural network with bias
        private FeedForwardNetwork CreateNetworkWithBias(double bias)
        {
            var nodes = new Dictionary<int, NodeGene>
            {
                { 0, new NodeGene(0, NodeType.Input) },      // Input node
                { 1, new NodeGene(1, NodeType.Output) }      // Output node with bias
            };
            
            // Set the bias on the output node
            nodes[1].Bias = bias;
            
            // Add a connection from input to output
            var connections = new Dictionary<int, ConnectionGene>
            {
                { 0, new ConnectionGene(0, 0, 1, 1.0) }  // Weight = 1.0
            };
            
            // Ensure proper layer assignment
            nodes[0].Layer = 0;
            nodes[1].Layer = 1;
            
            return new FeedForwardNetwork(nodes, connections);
        }
        
        [Fact]
        public void TestBiasAffectsOutput()
        {
            // Create networks with different bias values
            var network1 = CreateNetworkWithBias(0.0);
            var network2 = CreateNetworkWithBias(1.0);
            var network3 = CreateNetworkWithBias(-1.0);
            
            // All networks should produce different outputs for the same input
            var input = new double[] { 0.5 };
            
            var output1 = network1.Activate(input)[0];
            var output2 = network2.Activate(input)[0];
            var output3 = network3.Activate(input)[0];
            
            // With weight=1, input=0.5:
            // network1: tanh(0.5) 
            // network2: tanh(0.5 + 1.0)
            // network3: tanh(0.5 - 1.0)
            
            Assert.NotEqual(output1, output2);
            Assert.NotEqual(output1, output3);
            Assert.NotEqual(output2, output3);
            
            // Verify the exact expected values
            Assert.Equal(Math.Tanh(0.5), output1, 6);  // Precision to 6 decimal places
            Assert.Equal(Math.Tanh(1.5), output2, 6);
            Assert.Equal(Math.Tanh(-0.5), output3, 6);
        }
        
        [Fact]
        public void TestInputNodesIgnoreBias()
        {
            // Create a network with bias on input node (which should be ignored)
            var nodes = new Dictionary<int, NodeGene>
            {
                { 0, new NodeGene(0, NodeType.Input) },      // Input node with bias that should be ignored
                { 1, new NodeGene(1, NodeType.Input) },      // Another input node 
                { 2, new NodeGene(2, NodeType.Output) }      // Output node
            };
            
            // Set bias on input nodes (which should be ignored) and output node
            nodes[0].Bias = 2.0;  // Should be ignored since it's an input node
            nodes[1].Bias = 3.0;  // Should be ignored since it's an input node
            nodes[2].Bias = 1.0;  // Should be applied
            
            // Add connections from inputs to output
            var connections = new Dictionary<int, ConnectionGene>
            {
                { 0, new ConnectionGene(0, 0, 2, 1.0) },  // Weight = 1.0
                { 1, new ConnectionGene(1, 1, 2, 1.0) }   // Weight = 1.0
            };
            
            // Ensure proper layer assignment
            nodes[0].Layer = 0;
            nodes[1].Layer = 0;
            nodes[2].Layer = 1;
            
            var network = new FeedForwardNetwork(nodes, connections);
            
            // Test with input values
            var input = new double[] { 0.5, 0.3 };
            var output = network.Activate(input)[0];
            
            // Expected: tanh(1.0 + 0.5*1.0 + 0.3*1.0) = tanh(1.8)
            Assert.Equal(Math.Tanh(1.8), output, 6);
        }
        
        [Fact]
        public void TestBiasMutation()
        {
            // Create a config that specifies bias mutation parameters
            var config = new Config.Config();
            config.SetParameter("bias_mutation_rate", 1.0);  // 100% mutation rate
            config.SetParameter("mutation_power", 0.0);      // Set to 0 to test the else branch (reassign)
            config.SetParameter("num_inputs", 1);
            config.SetParameter("num_outputs", 1);
            
            // Create a population with this config
            var population = new Population(config);
            
            // Get a genome from the population
            var genome = population.GetBestGenome();
            
            // Store initial bias values
            var initialBiases = genome.Nodes.Values
                .Where(n => n.Type != NodeType.Input)
                .Select(n => n.Bias)
                .ToList();
            
            // Use reflection to access the private MutateGenome method
            var mutateMethod = typeof(Population).GetMethod("MutateGenome", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Invoke mutation multiple times to ensure bias values change
            for (int i = 0; i < 10; i++)
            {
                mutateMethod.Invoke(population, new object[] { genome });
            }
            
            // Check if bias values changed
            var finalBiases = genome.Nodes.Values
                .Where(n => n.Type != NodeType.Input)
                .Select(n => n.Bias)
                .ToList();
            
            // At least some bias values should have changed
            Assert.NotEqual(initialBiases, finalBiases);
        }
        
        [Fact]
        public void TestNodeAdditionWithBias()
        {
            // Create a config that guarantees node addition
            var config = new Config.Config();
            config.SetParameter("node_add_prob", 1.0); // 100% chance to add a node
            config.SetParameter("num_inputs", 1);
            config.SetParameter("num_outputs", 1);
            
            // Create a population with this config
            var population = new Population(config);
            
            // Get initial genome
            var genome = population.GetBestGenome();
            int initialNodeCount = genome.Nodes.Count;
            
            // Use reflection to access the private MutateGenome method
            var mutateMethod = typeof(Population).GetMethod("MutateGenome", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Apply mutation
            mutateMethod.Invoke(population, new object[] { genome });
            
            // Verify a node was added
            Assert.True(genome.Nodes.Count > initialNodeCount);
            
            // Verify the new node has a bias value
            var newNodes = genome.Nodes.Values
                .Where(n => n.Type == NodeType.Hidden)
                .ToList();
            
            Assert.NotEmpty(newNodes);
            
            // Check that each hidden node has a non-default bias value
            foreach (var node in newNodes)
            {
                Assert.NotEqual(0.0, node.Bias);
            }
        }
        
        [Fact]
        public void TestBiasInheritanceInCrossover()
        {
            // Create parent genomes with different bias values
            var genome1 = new Genome.Genome(1);
            genome1.Fitness = 10.0;
            
            var node1_1 = new NodeGene(0, NodeType.Input);
            var node1_2 = new NodeGene(1, NodeType.Output);
            node1_2.Bias = 2.0;  // Set bias on output node
            
            genome1.AddNode(node1_1);
            genome1.AddNode(node1_2);
            genome1.AddConnection(new ConnectionGene(0, 0, 1, 1.0));
            
            var genome2 = new Genome.Genome(2);
            genome2.Fitness = 5.0;
            
            var node2_1 = new NodeGene(0, NodeType.Input);
            var node2_2 = new NodeGene(1, NodeType.Output);
            node2_2.Bias = -2.0;  // Different bias on output node
            
            genome2.AddNode(node2_1);
            genome2.AddNode(node2_2);
            genome2.AddConnection(new ConnectionGene(0, 0, 1, 1.0));
            
            // Perform crossover
            var child = genome1.Crossover(genome2, 3);
            
            // Since genome1 is more fit, child should inherit its bias
            Assert.Equal(2.0, child.Nodes[1].Bias);
        }
    }
}