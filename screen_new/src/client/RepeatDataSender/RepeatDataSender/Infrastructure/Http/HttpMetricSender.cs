using System.Text;
using System.Text.Json;
using RepeatDataSender.Application.Abstractions;
using RepeatDataSender.Domain.Sending;

namespace RepeatDataSender.Infrastructure.Http;

/// <summary>
/// Posts one metric to the ESP <c>POST /data</c> endpoint as the JSON body
/// expected by <c>http_service</c> (the <c>metric</c> branch of the renderer,
/// which calls <c>print_one_metric</c>).
/// </summary>
public sealed class HttpMetricSender(HttpClient httpClient) : IMetricSender
{
    public async Task SendAsync(Uri targetUri, MetricSendRequest request, CancellationToken cancellationToken)
    {
        var payload = new
        {
            value = request.Value,
            metric = request.Label,
            value_x = request.Layout.ValueX,
            value_y = request.Layout.ValueY,
            metric_x = request.Layout.MetricX,
            metric_y = request.Layout.MetricY,
            value_font_size = request.Layout.ValueFontSize,
            metric_font_size = request.Layout.MetricFontSize,
        };

        var json = JsonSerializer.Serialize(payload);

        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await httpClient.PostAsync(targetUri, content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}