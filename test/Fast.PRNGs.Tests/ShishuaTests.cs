using Accord.Statistics.Distributions.Univariate;
using Accord.Statistics.Testing;
using Plotly.NET.CSharp;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Fast.PRNGs.Tests;

public sealed class ShishuaTests
{
    public void DoubleDistributionTest()
    {
        if (!Shishua.IsSupported)
            return;

        var baselinePrng = new Random();
        using var prng = Shishua.Create();

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
        Console.WriteLine($"Chi-Squared test: Shishua=(significant={prngTest.Significant}, pValue={prngTest.PValue})");

        var baselineLabel = "Baseline (System.Random)";
        var prngLabel = "Shishua";

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
        chart.SaveHtml("shishua.html");
    }

    public void InitFromNothing()
    {
        if (!Shishua.IsSupported)
            return;

        using var _ = Shishua.Create();
    }

    public void InitFromNew()
    {
        if (!Shishua.IsSupported)
            return;

        using var _ = Shishua.Create(new Random());
    }

    public void Compare()
    {
        if (!Shishua.IsSupported)
            return;

        using var rng1 = Shishua.Create(new Random(0));
        using var rng2 = Shishua.Create(new Random(0));
        using var rng3 = Shishua.Create(new Random(0));

        Vector256<double> vec256 = default;

        var val1 = rng1.NextDouble();
        rng2.NextDoubles256(ref vec256);
        //var val3 = rng3.NextDoubles512()[0];
        Assert.True(Math.Abs(vec256[0] - val1) < 0.0001d);
        //Assert.True(Math.Abs(val3 - val1) < 0.0001d);
    }

    public void InitFromBytes()
    {
        if (!Shishua.IsSupported)
            return;

        Span<byte> seedBytes = stackalloc byte[32];
        Random.Shared.NextBytes(seedBytes);
        using var _ = Shishua.Create(seedBytes);
    }

    public void FailsWhenGivenWrongSizeSeed()
    {
        if (!Shishua.IsSupported)
            return;

        Assert.Throws<ArgumentException>(() => {
            Span<byte> seedBytes = stackalloc byte[33];
            using var _ = Shishua.Create(seedBytes);
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void AssertInRange(double value)
    {
        Assert.True(value >= 0d);
        Assert.True(value < 1.0d);
    }
}
