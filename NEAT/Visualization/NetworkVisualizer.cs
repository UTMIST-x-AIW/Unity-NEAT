using System;
using System.Text;
using System.IO;
using System.Linq;
using NEAT.Genome;
using NEAT.Genes;

namespace NEAT.Visualization;

public class NetworkVisualizer
{
    private static readonly (double[] inputs, double output)[] XORTestCases = new[]
    {
        (new[] { 0.0, 0.0 }, 0.0),
        (new[] { 0.0, 1.0 }, 1.0),
        (new[] { 1.0, 0.0 }, 1.0),
        (new[] { 1.0, 1.0 }, 0.0)
    };

    private static void CalculateNodeValues(NEAT.Genome.Genome genome, double[] inputs)
    {
        // Reset all nodes
        foreach (var node in genome.Nodes.Values)
        {
            node.Value = 0.0;
        }

        // Set input values
        for (int i = 0; i < inputs.Length; i++)
        {
            genome.Nodes[i].Value = inputs[i];
        }

        // Activate the network
        var sortedNodes = genome.Nodes.Values
            .OrderBy(n => n.Type == NodeType.Input ? 0 : n.Type == NodeType.Hidden ? 1 : 2)
            .ThenBy(n => n.Key);

        foreach (var node in sortedNodes)
        {
            if (node.Type != NodeType.Input)
            {
                // Sum incoming connections
                double sum = genome.Connections.Values
                    .Where(c => c.OutputKey == node.Key && c.Enabled)
                    .Sum(c => genome.Nodes[c.InputKey].Value * c.Weight);

                // Apply activation function (sigmoid)
                node.Value = 1.0 / (1.0 + Math.Exp(-4.9 * sum));
            }
        }
    }

    public static string GenerateDotGraph(NEAT.Genome.Genome genome)
    {
        var sb = new StringBuilder();
        sb.AppendLine("digraph neat {");
        sb.AppendLine("  compound=true;"); // Enable connections to and from clusters
        sb.AppendLine("  rankdir=LR;");  // Left to right layout
        sb.AppendLine("  node [shape=circle];");  // Default node shape
        sb.AppendLine("  ranksep=2.0;");  // Increase spacing between ranks
        sb.AppendLine("  nodesep=0.5;");  // Increase spacing between nodes

        // Create a subgraph for each test case
        for (int testCase = 0; testCase < XORTestCases.Length; testCase++)
        {
            var (inputs, expectedOutput) = XORTestCases[testCase];
            
            // Calculate node values for this test case
            CalculateNodeValues(genome, inputs);

            sb.AppendLine($"  subgraph cluster_{testCase} {{");
            sb.AppendLine($"    label=\"Test {inputs[0]},{inputs[1]} → {expectedOutput}\";");
            sb.AppendLine("    style=rounded;");
            sb.AppendLine("    color=gray;");

            // Add nodes for this test case
            foreach (var node in genome.Nodes.Values)
            {
                string color = node.Type switch
                {
                    NodeType.Input => "lightblue",
                    NodeType.Output => "lightgreen",
                    _ => "lightgray"
                };
                // Display node value and add test case suffix to make node IDs unique
                sb.AppendLine($"    node{node.Key}_{testCase} [label=\"{node.Value:F3}\", style=filled, fillcolor={color}];");
            }

            // Add connections for this test case
            foreach (var conn in genome.Connections.Values)
            {
                if (conn.Enabled)
                {
                    sb.AppendLine($"    node{conn.InputKey}_{testCase} -> node{conn.OutputKey}_{testCase} [label=\"{conn.Weight:F2}\"];");
                }
            }

            sb.AppendLine("  }");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    public static void SaveDotToFile(string dot, string filePath)
    {
        // Create directory if it doesn't exist
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(filePath, dot);
        
        // Generate SVG using dot command
        var svgPath = Path.ChangeExtension(filePath, ".svg");
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "dot",
            Arguments = $"-Tsvg \"{filePath}\" -o \"{svgPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        
        using var process = System.Diagnostics.Process.Start(startInfo);
        process?.WaitForExit();
        
        if (process?.ExitCode == 0)
        {
            Console.WriteLine($"SVG visualization saved to: {svgPath}");
        }
        else
        {
            Console.WriteLine("Failed to generate SVG visualization");
        }
    }
} 