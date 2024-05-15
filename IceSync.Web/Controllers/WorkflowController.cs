using IceSync.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using IceSync.ApiClient;

namespace IceSync.Web.Controllers;

public class WorkflowController : Controller
{
    private readonly IApiClient _apiClient;

    public WorkflowController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var apiWorkFlows = await _apiClient.GetWorkflowsAsync();
        var workflows = apiWorkFlows.Select(w => new WorkflowViewModel
        {
            Id = w.Id,
            Name = w.Name,
            IsActive = w.IsActive,
            IsRunning = w.IsRunning,
            MultiExecBehavior = w.MultiExecBehavior
        });

        return View(workflows);
    }

    [HttpPost]
    public async Task<IActionResult> Run(int id)
    {
        var result = await _apiClient.RunWorkflowAsync(id);

        if (result)
        {
            return Ok();
        }

        return BadRequest();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}