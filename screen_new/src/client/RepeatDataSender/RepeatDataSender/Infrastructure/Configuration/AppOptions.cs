using RepeatDataSender.Domain.Metrics;

namespace RepeatDataSender.Infrastructure.Configuration;

/// <summary>
/// Configuration bound from the <c>RepeatData</c> section of appsettings.json.
/// </summary>
public sealed class AppOptions
{
    public const string SectionName = "RepeatData";

    public string TargetUri { get; set; } = "http://192.168.0.77/data";

    public int IntervalMilliseconds { get; set; } = 5000;

    public int DelayBetweenMetricsMilliseconds { get; set; } = 1000;

    public List<string> Disks { get; set; } = ["/", "/mnt/nvme0n1p2"];

    public List<MetricOptions> Metrics { get; set; } = [];
}

public sealed class MetricOptions
{
    public string Value { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public int ValueX { get; set; } = 25;

    public int ValueY { get; set; } = 25;

    public int MetricX { get; set; } = 25;

    public int MetricY { get; set; } = 80;

    public int ValueFontSize { get; set; } = 3;

    public int MetricFontSize { get; set; } = 3;

    public MetricTemplate ToTemplate()
    {
        return new MetricTemplate
        {
            Value = Value,
            Label = Label,
            Layout = new MetricLayout
            {
                ValueX = ValueX,
                ValueY = ValueY,
                MetricX = MetricX,
                MetricY = MetricY,
                ValueFontSize = ValueFontSize,
                MetricFontSize = MetricFontSize,
            },
        };
    }
}