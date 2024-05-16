using AutoMapper;
using IceSync.ApiClient;
using IceSync.Data.Entities;
using IceSync.Data.Repositories;

namespace IceSync.Web.Services;

public class WorkflowSyncService
{
    private readonly IApiClient _apiClient;
    private readonly IWorkflowRepository _workflowRepository;
    private readonly ILogger<WorkflowSyncService> _logger;
    private readonly IMapper _mapper;

    public WorkflowSyncService(IApiClient apiClient, IWorkflowRepository workflowRepository, IMapper mapper, ILogger<WorkflowSyncService> logger)
    {
        _apiClient = apiClient;
        _workflowRepository = workflowRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task SynchronizeWorkflows()
    {
        _logger.LogInformation("Starting workflow synchronization.");

        try
        {
            var workflowsFromApi = await _apiClient.GetWorkflowsAsync();

            var workflowsFromDb = await _workflowRepository.GetAllAsync();

            var workflowsToAdd = workflowsFromApi.Where(apiWorkflow => workflowsFromDb.All(dbWorkflow => dbWorkflow.Id != apiWorkflow.Id)).ToList();
            var workflowsToDelete = workflowsFromDb.Where(dbWorkflow => workflowsFromApi.All(apiWorkflow => apiWorkflow.Id != dbWorkflow.Id)).ToList();

            var workflowsToUpdate = new List<Workflow>();

            foreach (var dbWorkflow in workflowsFromDb)
            {
                var apiWorkflow = workflowsFromApi.FirstOrDefault(w => w.Id == dbWorkflow.Id);
                if (apiWorkflow == null)
                {
                    continue;
                }

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

            if (workflowsToAdd.Any())
            {
                await _workflowRepository.BulkInsertAsync(_mapper.Map<IEnumerable<Workflow>>(workflowsToAdd));
            }

            if (workflowsToDelete.Any())
            {
                await _workflowRepository.BulkDeleteAsync(_mapper.Map<IEnumerable<Workflow>>(workflowsToDelete));
            }

            if (workflowsToUpdate.Any())
            {
                await _workflowRepository.BulkUpdateAsync(_mapper.Map<IEnumerable<Workflow>>(workflowsToUpdate));
            }

            _logger.LogInformation("Workflow synchronization completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while synchronizing workflows.");
        }
    }
}
