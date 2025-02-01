using System;
using System.Text;
using System.IO;
using NEAT.Genome;
using NEAT.Genes;

namespace NEAT.Visualization;

public class NetworkVisualizer
{
    public static string GenerateDotGraph(NEAT.Genome.Genome genome)
    {
        var sb = new StringBuilder();
        sb.AppendLine("digraph neat {");
        sb.AppendLine("  rankdir=LR;");  // Left to right layout
        sb.AppendLine("  node [shape=circle];");  // Default node shape

        // Add nodes
        foreach (var node in genome.Nodes.Values)
        {
            string color = node.Type switch
            {
                NodeType.Input => "lightblue",
                NodeType.Output => "lightgreen",
                _ => "lightgray"
            };
            sb.AppendLine($"  node{node.Key} [label=\"{node.Key}\", style=filled, fillcolor={color}];");
        }

        // Add connections
        foreach (var conn in genome.Connections.Values)
        {
            if (conn.Enabled)
            {
                sb.AppendLine($"  node{conn.InputKey} -> node{conn.OutputKey} [label=\"{conn.Weight:F2}\"];");
            }
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