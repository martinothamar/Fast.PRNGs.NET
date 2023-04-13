namespace Fast.PRNGs.Benchmarks;

[Config(typeof(Config))]
public class PRNGsScaling
{
    private Random _random;
    private Shishua _shishua;
    private Xoroshiro128Plus _xoroshiro128plus;
    private Xoshiro256Plus _xoshiro256plus;
    private MWC256 _mwc256;

    [Params(100_000, 1_000_000)]
    public int Iterations { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _random = new Random();
        _shishua = Shishua.Create();
        _xoroshiro128plus = Xoroshiro128Plus.Create();
        _xoshiro256plus = Xoshiro256Plus.Create();
        _mwc256 = MWC256.Create();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _shishua.Dispose();
    }

    [Benchmark(Baseline = true)]
    public double SystemRandomGen()
    {
        for (int i = 0; i < Iterations; i++)
            _ = _random.NextDouble();

        return default;
    }

    [Benchmark]
    public double ShishuaGen()
    {
        for (int i = 0; i < Iterations; i++)
            _ = _shishua.NextDouble();

        return default;
    }

    [Benchmark]
    public double Xoroshiro128PlusGen()
    {
        for (int i = 0; i < Iterations; i++)
            _ = _xoroshiro128plus.NextDouble();

        return default;
    }

    [Benchmark]
    public double Xoshiro256PlusGen()
    {
        for (int i = 0; i < Iterations; i++)
            _ = _xoshiro256plus.NextDouble();

        return default;
    }

    [Benchmark]
    public double MWC256Gen()
    {
        for (int i = 0; i < Iterations; i++)
            _ = _mwc256.NextDouble();

        return default;
    }

    private sealed class Config : ManualConfig
    {
        public Config()
        {
            this.SummaryStyle = SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend);
            this.AddColumn(RankColumn.Arabic);
            this.Orderer = new DefaultOrderer(SummaryOrderPolicy.SlowestToFastest, MethodOrderPolicy.Declared);
        }
    }
}
