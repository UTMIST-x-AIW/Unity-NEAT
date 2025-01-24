using System;
using System.Collections.Generic;
using System.Linq;

public static class MathUtil
{
    // Method to calculate mean
    public static double Mean(IEnumerable<double> values)
    {
        var valueList = values.ToList();
        return valueList.Sum() / valueList.Count;
    }

    // Method to calculate median (basic median)
    public static double Median(IEnumerable<double> values)
    {
        var valueList = values.ToList();
        valueList.Sort();
        return valueList[valueList.Count / 2];
    }

    // Method to calculate median2 (handles even and odd cases)
    public static double Median2(IEnumerable<double> values)
    {
        var valueList = values.ToList();
        int n = valueList.Count;
        if (n <= 2)
        {
            return Mean(valueList);
        }
        valueList.Sort();
        if (n % 2 == 1)
        {
            return valueList[n / 2];
        }
        int i = n / 2;
        return (valueList[i - 1] + valueList[i]) / 2.0;
    }

    // Method to calculate variance
    public static double Variance(IEnumerable<double> values)
    {
        var valueList = values.ToList();
        double meanValue = Mean(valueList);
        return valueList.Sum(v => Math.Pow(v - meanValue, 2)) / valueList.Count;
    }

    // Method to calculate standard deviation
    public static double Stdev(IEnumerable<double> values)
    {
        return Math.Sqrt(Variance(values));
    }

    // Method to calculate softmax
    public static List<double> Softmax(IEnumerable<double> values)
    {
        var valueList = values.ToList();
        var eValues = valueList.Select(v => Math.Exp(v)).ToList();
        double sum = eValues.Sum();
        double invSum = 1.0 / sum;
        return eValues.Select(ev => ev * invSum).ToList();
    }

    // Lookup function for commonly used functions
    public static readonly Dictionary<string, Func<IEnumerable<double>, double>> StatFunctions = new Dictionary<string, Func<IEnumerable<double>, double>>()
    {
        { "min", values => values.Min() },
        { "max", values => values.Max() },
        { "mean", Mean },
        { "median", Median },
        { "median2", Median2 }
    };
}
