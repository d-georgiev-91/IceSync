using EFCore.BulkExtensions;
using IceSync.ApiClient;
using IceSync.Data;
using Microsoft.EntityFrameworkCore;

namespace IceSync.Web.Services;

public class WorkflowSyncService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IApiClient _apiClient;
    private readonly ILogger<WorkflowSyncService> _logger;

    public WorkflowSyncService(IServiceProvider serviceProvider, IApiClient apiClient, ILogger<WorkflowSyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _apiClient = apiClient;
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
            var workflowsToUpdate = workflowsFromDb.Where(dbWorkflow => workflowsFromApi.Any(apiWorkflow => apiWorkflow.Id == dbWorkflow.Id)).ToList();

            if (workflowsToAdd.Any())
            {
                await dbContext.BulkInsertAsync(workflowsToAdd);
            }

            if (workflowsToDelete.Any())
            {
                await dbContext.BulkDeleteAsync(workflowsToDelete);
            }

            if (workflowsToUpdate.Any())
            {
                await dbContext.BulkUpdateAsync(workflowsToUpdate);
            }

            _logger.LogInformation("Workflow synchronization completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while synchronizing workflows.");
        }
    }
}