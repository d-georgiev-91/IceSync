using EFCore.BulkExtensions;
using IceSync.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IceSync.Data.Repositories;

public class WorkflowRepository : IWorkflowRepository
{
    private readonly ApplicationDbContext _context;

    public WorkflowRepository(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<Workflow>> GetAllAsync() => await _context.Workflows.ToListAsync();

    public async Task<Workflow?> GetByIdAsync(int id) => await _context.Workflows.FindAsync(id);

    public async Task AddAsync(Workflow workflow) => await _context.Workflows.AddAsync(workflow);

    public void Update(Workflow workflow) => _context.Workflows.Update(workflow);

    public void Delete(Workflow workflow) => _context.Workflows.Remove(workflow);

    public async Task BulkInsertAsync(IEnumerable<Workflow> workflows) => await _context.BulkInsertAsync(workflows);

    public async Task BulkUpdateAsync(IEnumerable<Workflow> workflows) => await _context.BulkUpdateAsync(workflows);

    public async Task BulkDeleteAsync(IEnumerable<Workflow> workflows) => await _context.BulkDeleteAsync(workflows);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}