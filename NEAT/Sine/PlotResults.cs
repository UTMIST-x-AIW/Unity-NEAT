using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class PlotResults
{
    public static void SaveToCSV(List<double> inputs, List<double> expected, List<double> actual, string filepath)
    {
        using (StreamWriter writer = new StreamWriter(filepath))
        {
            writer.WriteLine("Input,Expected,Actual");
            for (int i = 0; i < inputs.Count; i++)
            {
                writer.WriteLine($"{inputs[i]},{expected[i]},{actual[i]}");
            }
        }
    }
}
