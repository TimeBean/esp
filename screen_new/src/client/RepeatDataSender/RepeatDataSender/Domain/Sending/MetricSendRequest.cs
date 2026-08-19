using RepeatDataSender.Domain.Metrics;

namespace RepeatDataSender.Domain.Sending;

/// <summary>
/// A fully resolved metric, ready to be rendered by
/// <c>print_one_metric</c> on the ESP. Placeholders were already replaced.
/// </summary>
public sealed record MetricSendRequest(string Value, string Label, MetricLayout Layout);