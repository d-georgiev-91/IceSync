using IceSync.ApiClient.ResponseModels;

namespace IceSync.ApiClient;

public interface IApiClient
{
    Task<IEnumerable<Workflow>?> GetWorkflowsAsync();

    Task<bool> RunWorkflowAsync(int id);
}
