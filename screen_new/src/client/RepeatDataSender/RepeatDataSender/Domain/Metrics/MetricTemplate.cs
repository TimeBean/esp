namespace RepeatDataSender.Domain.Metrics;

/// <summary>
/// A configured metric variant. <see cref="Value"/> and <see cref="Label"/>
/// may contain placeholders like <c>{Load1}</c> which are replaced with the
/// collected system values before sending.
/// </summary>
public sealed record MetricTemplate
{
    public required string Value { get; init; }

    public required string Label { get; init; }

    public MetricLayout Layout { get; init; } = new();
}