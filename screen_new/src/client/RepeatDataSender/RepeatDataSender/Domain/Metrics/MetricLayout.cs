namespace RepeatDataSender.Domain.Metrics;

/// <summary>
/// Display layout of a single metric, mirrors the
/// <c>print_one_metric(value, metric, value_x, value_y, metric_x, metric_y,
/// value_font_size, metric_font_size)</c> signature on the ESP side.
/// </summary>
public sealed record MetricLayout
{
    public int ValueX { get; init; } = 25;

    public int ValueY { get; init; } = 25;

    public int MetricX { get; init; } = 25;

    public int MetricY { get; init; } = 80;

    public int ValueFontSize { get; init; } = 3;

    public int MetricFontSize { get; init; } = 3;
}
