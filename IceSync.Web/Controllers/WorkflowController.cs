using IceSync.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using AutoMapper;
using IceSync.ApiClient;

namespace IceSync.Web.Controllers;

public class WorkflowController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly IMapper _mapper;

    public WorkflowController(IApiClient apiClient, IMapper mapper)
    {
        _apiClient = apiClient;
        _mapper = mapper;
    }

    public async Task<IActionResult> Index()
    {
        var apiWorkFlows = await _apiClient.GetWorkflowsAsync();
        var workflows = _mapper.Map<IEnumerable<WorkflowViewModel>?>(apiWorkFlows);

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