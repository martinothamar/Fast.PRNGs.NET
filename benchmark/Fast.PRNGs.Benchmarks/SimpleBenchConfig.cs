namespace Fast.PRNGs.Benchmarks;

internal sealed class SimpleBenchConfig : ManualConfig
{
    public SimpleBenchConfig(ulong? byteSizePerIteration = null)
    {
        this.SummaryStyle = SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend);
        this.AddColumn(RankColumn.Arabic);
        this.Orderer = new DefaultOrderer(SummaryOrderPolicy.SlowestToFastest, MethodOrderPolicy.Declared);
        if (byteSizePerIteration != null)
            this.AddColumn(new ThroughputColumn(byteSizePerIteration.Value));
    }
}
