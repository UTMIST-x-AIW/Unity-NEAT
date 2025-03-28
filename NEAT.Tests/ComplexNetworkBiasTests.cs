using System;
using System.Linq;
using System.Collections.Generic;
using NEAT.Genes;
using NEAT.Genome;
using NEAT.NN;
using Xunit;

namespace NEAT.Tests
{
    public class ComplexNetworkBiasTests
    {
        // Helper method to create a complex neural network with multiple layers and biases
        private FeedForwardNetwork CreateComplexNetwork()
        {
            var nodes = new Dictionary<int, NodeGene>
            {
                // Input layer (layer 0)
                { 0, new NodeGene(0, NodeType.Input) },
                { 1, new NodeGene(1, NodeType.Input) },
                
                // Hidden layer 1 (layer 1)
                { 2, new NodeGene(2, NodeType.Hidden) },
                { 3, new NodeGene(3, NodeType.Hidden) },
                
                // Hidden layer 2 (layer 2)
                { 4, new NodeGene(4, NodeType.Hidden) },
                
                // Output layer (layer 3)
                { 5, new NodeGene(5, NodeType.Output) }
            };
            
            // Set layer information
            nodes[0].Layer = 0;
            nodes[1].Layer = 0;
            nodes[2].Layer = 1;
            nodes[3].Layer = 1;
            nodes[4].Layer = 2;
            nodes[5].Layer = 3;
            
            // Set specific bias values for each non-input node
            nodes[2].Bias = 0.5;   // Hidden node
            nodes[3].Bias = -0.3;  // Hidden node
            nodes[4].Bias = 0.7;   // Hidden node
            nodes[5].Bias = -0.2;  // Output node
            
            // Add connections
            var connections = new Dictionary<int, ConnectionGene>
            {
                // Input to hidden layer 1
                { 0, new ConnectionGene(0, 0, 2, 0.1) },
                { 1, new ConnectionGene(1, 0, 3, 0.2) },
                { 2, new ConnectionGene(2, 1, 2, 0.3) },
                { 3, new ConnectionGene(3, 1, 3, 0.4) },
                
                // Hidden layer 1 to hidden layer 2
                { 4, new ConnectionGene(4, 2, 4, 0.5) },
                { 5, new ConnectionGene(5, 3, 4, 0.6) },
                
                // Hidden layer 2 to output
                { 6, new ConnectionGene(6, 4, 5, 0.7) },
                
                // Skip connections
                { 7, new ConnectionGene(7, 2, 5, 0.8) },  // Hidden layer 1 to output
                { 8, new ConnectionGene(8, 0, 4, 0.9) }   // Input to hidden layer 2
            };
            
            return new FeedForwardNetwork(nodes, connections);
        }
        
        [Fact]
        public void TestComplexNetworkBiasCorrectPropagation()
        {
            // Create a network with the specified topology and bias values
            var network = CreateComplexNetwork();
            
            // Test with specific input values
            var input = new double[] { 1.0, -1.0 };
            var output = network.Activate(input)[0];
            
            // Manually calculate the expected output
            // Node 2: tanh(0.5 + 1.0*0.1 + (-1.0)*0.3) = tanh(0.5 + 0.1 - 0.3) = tanh(0.3)
            double node2Value = Math.Tanh(0.3);
            
            // Node 3: tanh(-0.3 + 1.0*0.2 + (-1.0)*0.4) = tanh(-0.3 + 0.2 - 0.4) = tanh(-0.5)
            double node3Value = Math.Tanh(-0.5);
            
            // Node 4: tanh(0.7 + node2Value*0.5 + node3Value*0.6 + 1.0*0.9) = tanh(0.7 + node2Value*0.5 + node3Value*0.6 + 0.9)
            double node4Value = Math.Tanh(0.7 + node2Value * 0.5 + node3Value * 0.6 + 0.9);
            
            // Output (Node 5): tanh(-0.2 + node4Value*0.7 + node2Value*0.8)
            double expectedOutput = Math.Tanh(-0.2 + node4Value * 0.7 + node2Value * 0.8);
            
            // Verify the output matches the expected value
            Assert.Equal(expectedOutput, output, 6);  // Precision to 6 decimal places
        }
        
        [Fact]
        public void TestBiasCompensatesForNoInput()
        {
            // Create a simple network with a strong bias but no inputs
            var nodes = new Dictionary<int, NodeGene>
            {
                { 0, new NodeGene(0, NodeType.Input) },
                { 1, new NodeGene(1, NodeType.Output) }
            };
            
            nodes[0].Layer = 0;
            nodes[1].Layer = 1;
            nodes[1].Bias = 10.0;  // Very strong bias
            
            // Create the network with no connections
            var connections = new Dictionary<int, ConnectionGene>();
            var network = new FeedForwardNetwork(nodes, connections);
            
            // Test with zero input
            var output = network.Activate(new double[] { 0.0 })[0];
            
            // Expected output: tanh(10.0) which is close to 1.0
            Assert.Equal(Math.Tanh(10.0), output, 6);
            Assert.True(output > 0.9999);  // Should be very close to 1.0
        }
        
        [Fact]
        public void TestZeroBiasHasNoEffect()
        {
            // Create two identical networks except one has zero bias
            // Network 1: With zero bias
            var nodes1 = new Dictionary<int, NodeGene>
            {
                { 0, new NodeGene(0, NodeType.Input) },
                { 1, new NodeGene(1, NodeType.Output) }
            };
            
            nodes1[0].Layer = 0;
            nodes1[1].Layer = 1;
            nodes1[1].Bias = 0.0;  // Zero bias
            
            var connections1 = new Dictionary<int, ConnectionGene>
            {
                { 0, new ConnectionGene(0, 0, 1, 1.0) }
            };
            
            var network1 = new FeedForwardNetwork(nodes1, connections1);
            
            // Network 2: Without bias field (default is 0)
            var nodes2 = new Dictionary<int, NodeGene>
            {
                { 0, new NodeGene(0, NodeType.Input) },
                { 1, new NodeGene(1, NodeType.Output) }
            };
            
            nodes2[0].Layer = 0;
            nodes2[1].Layer = 1;
            // No bias set - should default to 0
            
            var connections2 = new Dictionary<int, ConnectionGene>
            {
                { 0, new ConnectionGene(0, 0, 1, 1.0) }
            };
            
            var network2 = new FeedForwardNetwork(nodes2, connections2);
            
            // Test with same input
            var input = new double[] { 0.5 };
            var output1 = network1.Activate(input)[0];
            var output2 = network2.Activate(input)[0];
            
            // Outputs should be identical
            Assert.Equal(output1, output2);
            Assert.Equal(Math.Tanh(0.5), output1, 6);
        }
        
        [Fact]
        public void TestBiasSignFlipsResponse()
        {
            // Create two identical networks except with opposite bias signs
            // Network 1: Positive bias
            var nodes1 = CreateNodesWithBias(2.0);
            var connections1 = CreateBasicConnections();
            var network1 = new FeedForwardNetwork(nodes1, connections1);
            
            // Network 2: Negative bias
            var nodes2 = CreateNodesWithBias(-2.0);
            var connections2 = CreateBasicConnections();
            var network2 = new FeedForwardNetwork(nodes2, connections2);
            
            // Test with zero input (so only bias affects the output)
            var input = new double[] { 0.0 };
            var output1 = network1.Activate(input)[0];
            var output2 = network2.Activate(input)[0];
            
            // Outputs should be opposite in sign
            Assert.True(output1 > 0);
            Assert.True(output2 < 0);
            Assert.Equal(Math.Tanh(2.0), output1, 6);
            Assert.Equal(Math.Tanh(-2.0), output2, 6);
        }
        
        private Dictionary<int, NodeGene> CreateNodesWithBias(double bias)
        {
            var nodes = new Dictionary<int, NodeGene>
            {
                { 0, new NodeGene(0, NodeType.Input) },
                { 1, new NodeGene(1, NodeType.Output) }
            };
            
            nodes[0].Layer = 0;
            nodes[1].Layer = 1;
            nodes[1].Bias = bias;
            
            return nodes;
        }
        
        private Dictionary<int, ConnectionGene> CreateBasicConnections()
        {
            return new Dictionary<int, ConnectionGene>
            {
                { 0, new ConnectionGene(0, 0, 1, 0.0) }  // Weight = 0 to isolate bias effect
            };
        }
    }
} 