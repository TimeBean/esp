using RepeatDataSender.Domain.Metrics;

namespace RepeatDataSender.Application.Abstractions;

/// <summary>
/// Collects the current values of every supported system metric.
/// </summary>
public interface ISystemInfoCollector
{
    Task<MetricSnapshot> CollectAsync(CancellationToken cancellationToken);
}