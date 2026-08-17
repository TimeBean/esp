using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
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