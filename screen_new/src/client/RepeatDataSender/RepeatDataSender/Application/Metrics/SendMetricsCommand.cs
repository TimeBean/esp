using MediatR;
using Microsoft.Extensions.Logging;
using RepeatDataSender.Application.Abstractions;
using RepeatDataSender.Domain.Metrics;
using RepeatDataSender.Domain.Sending;

namespace RepeatDataSender.Application.Metrics;

/// <summary>
/// Collects one system snapshot, resolves every configured metric variant
/// and sends them to the display, pausing between variants.
/// </summary>
public sealed record SendMetricsCommand : IRequest
{
    public required Uri TargetUri { get; init; }

    public required IReadOnlyList<MetricTemplate> Metrics { get; init; }

    public int DelayBetweenMetricsMilliseconds { get; init; }
}

public sealed class SendMetricsCommandHandler(
    IMediator mediator,
    IMetricSender sender,
    ILogger<SendMetricsCommandHandler> logger)
    : IRequestHandler<SendMetricsCommand>
{
    public async Task Handle(SendMetricsCommand command, CancellationToken cancellationToken)
    {
        var snapshot = await mediator.Send(new CollectSystemInfoQuery(), cancellationToken);

        for (var index = 0; index < command.Metrics.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var template = command.Metrics[index];
            var request = new MetricSendRequest(
                snapshot.Resolve(template.Value),
                snapshot.Resolve(template.Label),
                template.Layout);

            await sender.SendAsync(command.TargetUri, request, cancellationToken);

            logger.LogInformation("Sent '{Label}' = '{Value}' to {Uri}",
                request.Label, request.Value, command.TargetUri);

            if (index < command.Metrics.Count - 1 && command.DelayBetweenMetricsMilliseconds > 0)
            {
                await Task.Delay(command.DelayBetweenMetricsMilliseconds, cancellationToken);
            }
        }
    }
}