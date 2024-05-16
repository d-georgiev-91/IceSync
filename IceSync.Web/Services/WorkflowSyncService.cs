using AutoMapper;
using EFCore.BulkExtensions;
using IceSync.ApiClient;
using IceSync.Data;
using IceSync.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IceSync.Web.Services;

public class WorkflowSyncService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IApiClient _apiClient;
    private readonly ILogger<WorkflowSyncService> _logger;
    private readonly IMapper _mapper;

    public WorkflowSyncService(IServiceProvider serviceProvider, IApiClient apiClient, IMapper mapper, ILogger<WorkflowSyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _apiClient = apiClient;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task SynchronizeWorkflows()
    {
        _logger.LogInformation("Starting workflow synchronization.");

        try
        {
            var workflowsFromApi = await _apiClient.GetWorkflowsAsync();

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var workflowsFromDb = await dbContext.Workflows.ToListAsync();

            var workflowsToAdd = workflowsFromApi.Where(apiWorkflow => workflowsFromDb.All(dbWorkflow => dbWorkflow.Id != apiWorkflow.Id)).ToList();
            var workflowsToDelete = workflowsFromDb.Where(dbWorkflow => workflowsFromApi.All(apiWorkflow => apiWorkflow.Id != dbWorkflow.Id)).ToList();

            var workflowsToUpdate = new List<Workflow>();

            foreach (var dbWorkflow in workflowsFromDb)
            {
                var apiWorkflow = workflowsFromApi.FirstOrDefault(w => w.Id == dbWorkflow.Id);
                if (apiWorkflow != null)
                {
                    var isUpdated = false;

                    if (dbWorkflow.Name != apiWorkflow.Name)
                    {
                        dbWorkflow.Name = apiWorkflow.Name;
                        isUpdated = true;
                    }

                    if (dbWorkflow.IsActive != apiWorkflow.IsActive)
                    {
                        dbWorkflow.IsActive = apiWorkflow.IsActive;
                        isUpdated = true;
                    }

                    if (dbWorkflow.IsRunning != apiWorkflow.IsRunning)
                    {
                        dbWorkflow.IsRunning = apiWorkflow.IsRunning;
                        isUpdated = true;
                    }

                    if (dbWorkflow.MultiExecBehavior != apiWorkflow.MultiExecBehavior)
                    {
                        dbWorkflow.MultiExecBehavior = apiWorkflow.MultiExecBehavior;
                        isUpdated = true;
                    }

                    if (isUpdated)
                    {
                        workflowsToUpdate.Add(dbWorkflow);
                    }
                }
            }

            if (workflowsToAdd.Any())
            {
                await dbContext.BulkInsertAsync(_mapper.Map<IEnumerable<Workflow>>(workflowsToAdd));
            }

            if (workflowsToDelete.Any())
            {
                await dbContext.BulkDeleteAsync(_mapper.Map<IEnumerable<Workflow>>(workflowsToDelete));
            }

            if (workflowsToUpdate.Any())
            {
                await dbContext.BulkUpdateAsync(_mapper.Map<IEnumerable<Workflow>>(workflowsToUpdate));
            }

            _logger.LogInformation("Workflow synchronization completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while synchronizing workflows.");
        }
    }
}
