using AutoMapper;
using IceSync.ApiClient;
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

            var workflowsToUpdate = new List<Data.Entities.Workflow>();

            foreach (var dbWorkflow in workflowsFromDb)
            {
                var apiWorkflow = workflowsFromApi.FirstOrDefault(w => w.Id == dbWorkflow.Id);
                if (apiWorkflow == null)
                {
                    continue;
                }

                if (UpdateWorkflowProperties(dbWorkflow, apiWorkflow))
                {
                    workflowsToUpdate.Add(dbWorkflow);
                }
            }

            if (workflowsToAdd.Any())
            {
                await _workflowRepository.BulkInsertAsync(_mapper.Map<IEnumerable<Data.Entities.Workflow>>(workflowsToAdd));
            }

            if (workflowsToDelete.Any())
            {
                await _workflowRepository.BulkDeleteAsync(_mapper.Map<IEnumerable<Data.Entities.Workflow>>(workflowsToDelete));
            }

            if (workflowsToUpdate.Any())
            {
                await _workflowRepository.BulkUpdateAsync(_mapper.Map<IEnumerable<Data.Entities.Workflow>>(workflowsToUpdate));
            }

            _logger.LogInformation("Workflow synchronization completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while synchronizing workflows.");
        }
    }

    private bool UpdateWorkflowProperties(Data.Entities.Workflow dbWorkflow, ApiClient.ResponseModels.Workflow apiWorkflow)
    {
        var isUpdated = false;
        var dbWorkflowProperties = typeof(Data.Entities.Workflow).GetProperties();
        var apiWorkflowType = typeof(ApiClient.ResponseModels.Workflow);

        foreach (var property in dbWorkflowProperties)
        {
            if (property.Name == "Id") continue;

            var apiWorkflowProperty = apiWorkflowType.GetProperty(property.Name) ?? throw new InvalidOperationException($"Property mismatch, no such property in Workflow entity {property.Name}");

            var dbValue = property.GetValue(dbWorkflow);
            var apiValue = apiWorkflowProperty.GetValue(apiWorkflow);

            if (Equals(dbValue, apiValue)) continue;
            property.SetValue(dbWorkflow, apiValue);
            isUpdated = true;
        }

        return isUpdated;
    }
}
