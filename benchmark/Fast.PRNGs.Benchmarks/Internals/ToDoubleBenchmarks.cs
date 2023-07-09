namespace Fast.PRNGs.Benchmarks.Internals;

[ConfigSource]
public class ToDoublesBenchmark
{
    internal const ulong DoubleMask = (1L << 53) - 1;
    internal const double Norm53 = 1.0d / (1L << 53);

    [Params(31512512431231UL)]
    public ulong Value { get; set; }

    [Benchmark]
    public double Original()
    {
        return (Value & DoubleMask) * Norm53;
    }

    [Benchmark]
    public double New()
    {
        return (Value >> 11) * (1.0 / (1ul << 53));
    }

    private class ConfigSourceAttribute : Attribute, IConfigSource
    {
        public IConfig Config { get; }

        public ConfigSourceAttribute() => Config = new SimpleBenchConfig(8);
    }
}
