using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RepeatDataSender.Application.Metrics;
using RepeatDataSender.Infrastructure.Configuration;

namespace RepeatDataSender.Presentation;

/// <summary>
/// Background loop: every <see cref="AppOptions.IntervalMilliseconds"/>
/// collects the system snapshot and sends all configured metric variants to
/// the display.
/// </summary>
public sealed class MetricPumpHostedService(
    IMediator mediator,
    IOptions<AppOptions> options,
    ILogger<MetricPumpHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var app = options.Value;

        if (!Uri.TryCreate(app.TargetUri, UriKind.Absolute, out var uri))
        {
            logger.LogError("Invalid TargetUri in config: {TargetUri}", app.TargetUri);
            return;
        }

        if (app.Metrics.Count == 0)
        {
            logger.LogWarning("No metrics configured in appsettings.json (RepeatData:Metrics)");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var command = new SendMetricsCommand
                {
                    TargetUri = uri,
                    Metrics = app.Metrics.Select(metric => metric.ToTemplate()).ToList(),
                    DelayBetweenMetricsMilliseconds = app.DelayBetweenMetricsMilliseconds,
                };

                await mediator.Send(command, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send metrics");
            }

            await Task.Delay(app.IntervalMilliseconds, stoppingToken);
        }
    }
}