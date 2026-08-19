using RepeatDataSender.Domain.Sending;

namespace RepeatDataSender.Application.Abstractions;

/// <summary>
/// Sends one resolved metric to the ESP display over HTTP.
/// </summary>
public interface IMetricSender
{
    Task SendAsync(Uri targetUri, MetricSendRequest request, CancellationToken cancellationToken);
}