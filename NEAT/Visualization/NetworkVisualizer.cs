using System.Text;
using NEAT.Genome;
using NEAT.Genes;

namespace Visualization;

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
            string style = conn.Enabled ? "solid" : "dashed";
            string color = conn.Weight > 0 ? "black" : "red";
            double width = Math.Abs(conn.Weight);
            sb.AppendLine($"  node{conn.InputKey} -> node{conn.OutputKey} [style={style}, color={color}, penwidth={width:F2}];");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    public static void SaveDotToFile(string dot, string filePath)
    {
        File.WriteAllText(filePath, dot);
    }
} 