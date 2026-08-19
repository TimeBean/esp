using MediatR;
using RepeatDataSender.Application.Abstractions;
using RepeatDataSender.Domain.Metrics;

namespace RepeatDataSender.Application.Metrics;

/// <summary>
/// Snapshot request for the current system metrics.
/// </summary>
public sealed record CollectSystemInfoQuery : IRequest<MetricSnapshot>;

public sealed class CollectSystemInfoQueryHandler(ISystemInfoCollector collector)
    : IRequestHandler<CollectSystemInfoQuery, MetricSnapshot>
{
    public Task<MetricSnapshot> Handle(CollectSystemInfoQuery request, CancellationToken cancellationToken)
    {
        return collector.CollectAsync(cancellationToken);
    }
}