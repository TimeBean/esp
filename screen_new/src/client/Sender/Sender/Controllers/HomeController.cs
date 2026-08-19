using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Sender.Models;

namespace Sender.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Send(string address, string message, CancellationToken ct)
    {
        var uri = address.Trim();
        if (string.IsNullOrWhiteSpace(uri))
        {
            return Json(new { success = false, result = "Address is required" });
        }

        if (!uri.StartsWith("http://") && !uri.StartsWith("https://"))
            uri = "http://" + uri;

        using var client = _httpClientFactory.CreateClient();
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(new { value = message ?? "" });
            var response = await client.PostAsync(uri, new StringContent(json, System.Text.Encoding.UTF8, "application/json"), ct);
            return Json(new { success = true, result = $"{(int)response.StatusCode} {response.ReasonPhrase}" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, result = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SendImage(string address, IFormFile file, CancellationToken ct)
    {
        var uri = address.Trim();
        if (string.IsNullOrWhiteSpace(uri))
        {
            return Json(new { success = false, result = "Address is required" });
        }

        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, result = "Image file is required" });
        }

        if (!uri.StartsWith("http://") && !uri.StartsWith("https://"))
            uri = "http://" + uri;

        try
        {
            const int size = 240;
            var rgb565 = await ImageToRgb565Async(file, size, ct);

            using var client = _httpClientFactory.CreateClient();
            var content = new ByteArrayContent(rgb565);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            var response = await client.PostAsync(uri, content, ct);
            return Json(new { success = true, result = $"{(int)response.StatusCode} {response.ReasonPhrase}" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, result = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SendMetric(
        string address,
        string value,
        string metric,
        int value_x,
        int value_y,
        int metric_x,
        int metric_y,
        int value_font_size,
        int metric_font_size,
        CancellationToken ct)
    {
        var uri = address.Trim();
        if (string.IsNullOrWhiteSpace(uri))
        {
            return Json(new { success = false, result = "Address is required" });
        }

        if (!uri.StartsWith("http://") && !uri.StartsWith("https://"))
            uri = "http://" + uri;

        try
        {
            var payload = new
            {
                value = value ?? "",
                metric = metric ?? "",
                value_x,
                value_y,
                metric_x,
                metric_y,
                value_font_size,
                metric_font_size
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            using var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync(uri, new StringContent(json, System.Text.Encoding.UTF8, "application/json"), ct);
            return Json(new { success = true, result = $"{(int)response.StatusCode} {response.ReasonPhrase}" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, result = ex.Message });
        }
    }

    private static async Task<byte[]> ImageToRgb565Async(IFormFile file, int size, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        using var image = await Image.LoadAsync<Rgba32>(stream, ct);

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(size, size),
            Mode = ResizeMode.Crop
        }));

        var rgb565 = new byte[size * size * 2];
        image.ProcessPixelRows(accessor =>
        {
            int pixelIndex = 0;
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < row.Length; x++)
                {
                    var p = row[x];
                    var value = (ushort)(((p.R & 0xF8) << 8) | ((p.G & 0xFC) << 3) | (p.B >> 3));
                    rgb565[pixelIndex++] = (byte)(value & 0xFF);
                    rgb565[pixelIndex++] = (byte)(value >> 8);
                }
            }
        });

        return rgb565;
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}