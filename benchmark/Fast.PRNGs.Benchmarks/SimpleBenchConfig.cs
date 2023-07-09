namespace Fast.PRNGs.Benchmarks;

internal sealed class SimpleBenchConfig : ManualConfig
{
    public SimpleBenchConfig()
    {
        this.SummaryStyle = SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend);
        this.AddColumn(RankColumn.Arabic);
        this.Orderer = new DefaultOrderer(SummaryOrderPolicy.SlowestToFastest, MethodOrderPolicy.Declared);
    }
}
