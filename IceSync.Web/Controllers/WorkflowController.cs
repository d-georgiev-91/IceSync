using IceSync.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace IceSync.Web.Controllers;

public class WorkflowController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WorkflowController> _logger;

    public WorkflowController(IHttpClientFactory httpClientFactory, ILogger<WorkflowController> logger)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        var client = _httpClientFactory.CreateClient("ApiHttpClient");
        var response = await client.GetAsync("https://api-test.universal-loader.com/workflows");
        response.EnsureSuccessStatusCode();
        var workflows = await response.Content.ReadFromJsonAsync<IEnumerable<WorkflowViewModel>>();

        return View(workflows);
    }

    [HttpPost]
    public async Task<IActionResult> Run(int id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiHttpClient");
            var response = await client.PostAsync($"https://api-test.universal-loader.com/workflows/{id}/run", null);
            response.EnsureSuccessStatusCode();
            return Ok();
            return Json(new { success = true, message = $"Workflow {id} is now running." });
        }
        catch (HttpRequestException ex)
        {
            return BadRequest();
            return Json(new { success = false, message = $"Error running workflow {id}: {ex.Message}" });
        }
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}