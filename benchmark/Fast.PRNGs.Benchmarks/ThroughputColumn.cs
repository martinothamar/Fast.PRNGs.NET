using BenchmarkDotNet.Running;

namespace Fast.PRNGs.Benchmarks;

public class ThroughputColumn : IColumn
{
    public string Id { get; }

    public string ColumnName { get; }

    private readonly ulong _byteSizePerIteration;

    public ThroughputColumn(ulong byteSizePerIteration)
    {
        ColumnName = "Throughput";
        Id = nameof(TagColumn) + "." + ColumnName;

        _byteSizePerIteration = byteSizePerIteration;
    }

    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
    public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
    {
        var stats = summary[benchmarkCase].ResultStatistics;
        if (stats is null || stats.Mean == default || double.IsNaN(stats.Mean))
            return "?";

        var gbs = (_byteSizePerIteration / 1e9d) / (stats.Mean / 1e9d);
        return $"{gbs:0.00} GB/s";
    }

    public bool IsAvailable(Summary summary) => true;
    public bool AlwaysShow => true;
    public ColumnCategory Category => ColumnCategory.Metric;
    public int PriorityInCategory => 0;
    public bool IsNumeric => true;
    public UnitType UnitType => UnitType.Size;
    public string Legend => $"Throughput in GB/s";
    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);
    public override string ToString() => ColumnName;
}
