using System.Runtime.Intrinsics;

namespace Fast.PRNGs.Benchmarks;

[Config(typeof(SimpleBenchConfig))]
public class PRNGsScaling
{
    private Random _random;
    private Shishua _shishuaSeq;
    private Shishua _shishuaVec256;
    private Shishua _shishuaVec512;
    private Xoroshiro128Plus _xoroshiro128plus;
    private Xoshiro256Plus _xoshiro256plus;
    private MWC256 _mwc256;

    [Params(1 << 17/*, 1 << 20*/)]
    public int Iterations { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _random = new Random();
        _shishuaSeq = Shishua.Create();
        _shishuaVec256 = Shishua.Create();
        _shishuaVec512 = Shishua.Create();
        _xoroshiro128plus = Xoroshiro128Plus.Create();
        _xoshiro256plus = Xoshiro256Plus.Create();
        _mwc256 = MWC256.Create();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _shishuaSeq.Dispose();
        _shishuaVec256.Dispose();
        _shishuaVec512.Dispose();
    }

    [Benchmark(Baseline = true)]
    public double SystemRandomGen()
    {
        for (int i = 0; i < Iterations; i++)
            _ = _random.NextDouble();

        return default;
    }

    [Benchmark]
    public double ShishuaSeqGen()
    {
        for (int i = 0; i < Iterations; i++)
            _ = _shishuaSeq.NextDouble();

        return default;
    }

    [Benchmark]
    public double ShishuaVec256Gen()
    {
        Vector256<double> result = default;
        for (int i = 0; i < Iterations; i += 4)
            _shishuaVec256.NextDoubles256(ref result);

        return default;
    }

    [Benchmark]
    public double ShishuaVec512Gen()
    {
        Vector512<double> result = default;
        for (int i = 0; i < Iterations; i += 8)
            _shishuaVec512.NextDoubles512(ref result);

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
}
