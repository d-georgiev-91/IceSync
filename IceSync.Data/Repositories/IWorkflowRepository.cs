using IceSync.Data.Entities;

namespace IceSync.Data.Repositories;

public interface IWorkflowRepository
{
    Task<IEnumerable<Workflow>> GetAllAsync();
        
    Task<Workflow?> GetByIdAsync(int id);
        
    Task AddAsync(Workflow workflow);
        
    void Update(Workflow workflow);

    void Delete(Workflow workflow);

    Task BulkInsertAsync(IEnumerable<Workflow> workflows);
        
    Task BulkUpdateAsync(IEnumerable<Workflow> workflows);
        
    Task BulkDeleteAsync(IEnumerable<Workflow> workflows);

    Task SaveChangesAsync();
}