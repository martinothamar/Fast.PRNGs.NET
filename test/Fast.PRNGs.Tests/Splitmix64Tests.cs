﻿using Accord.Statistics.Distributions.Univariate;
using Accord.Statistics.Testing;
using Plotly.NET.CSharp;
using System.Runtime.CompilerServices;

namespace Fast.PRNGs.Tests;

public sealed class Splitmix64Tests
{
    public void DoubleDistributionTest()
    {
        var baselinePrng = new Random();
        var prng = Splitmix64.Create();

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

        var baselineTest = new ChiSquareTest(baselineValues, new UniformContinuousDistribution(0.0d, 1.0d - 0.000001d));
        var prngTest = new ChiSquareTest(values, new UniformContinuousDistribution(0.0d, 1.0d - 0.000001d));
        Console.WriteLine($"Chi-Squared test: Baseline=(significant={baselineTest.Significant}, pValue={baselineTest.PValue})");
        Console.WriteLine($"Chi-Squared test: Splitmix64+=(significant={prngTest.Significant}, pValue={prngTest.PValue})");

        var baselineLabel = "Baseline (System.Random)";
        var prngLabel = "Splitmix64+";

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
        chart.SaveHtml("splitmix64+.html");
    }


    public void InitFromNothing()
    {
        var _ = Splitmix64.Create();
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void AssertInRange(double value)
    {
        Assert.True(value >= 0d);
        Assert.True(value < 1.0d);
    }
}
