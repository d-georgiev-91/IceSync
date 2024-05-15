using AutoMapper;

namespace IceSync.Web;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<ApiClient.ResponseModels.Workflow, Models.WorkflowViewModel>();
        CreateMap<ApiClient.ResponseModels.Workflow, Data.Entities.Workflow>();
    }
}