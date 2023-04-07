using MathNet.Numerics.Distributions;
using Plotly.NET.CSharp;
using System.Runtime.CompilerServices;
using Config = Plotly.NET.Config;
using GenericChartExtensions = Plotly.NET.GenericChartExtensions;

namespace Fast.PRNGs.Tests;

public sealed class Xoshiro256PlusTests
{
    public void DoubleDistributionTest()
    {
        var baselinePrng = new Random();
        var prng = Xoroshiro128Plus.Create();

        const int iterations = 10_000_000;
        var baselineValues = new double[iterations];
        var values = new double[iterations];
        for (int i = 0; i < values.Length; i++)
        {
            var baselineValue = baselinePrng.NextDouble();
            var value = prng.NextDouble();

            AssertInRange(baselineValue);
            AssertInRange(value);

            baselineValues[i] = baselineValue;
            values[i] = value;
        }

        var baselineLabel = "Baseline (System.Random)";
        var prngLabel = "Xoshiro256+";

        var baselineChart = Chart.Histogram<double, double, string>(
            X: baselineValues,
            Name: baselineLabel,
            Text: baselineLabel
        );
        var prngChart = Chart.Histogram<double, double, string>(
            X: values,
            Name: prngLabel,
            Text: prngLabel
        );
        var chart = Chart.Combine(new []{ baselineChart, prngChart });
        chart.SaveHtml("xoshiro256+.html");
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void AssertInRange(double value)
    {
        Assert.True(value >= 0d);
        Assert.True(value < 1.0d);
    }
}
