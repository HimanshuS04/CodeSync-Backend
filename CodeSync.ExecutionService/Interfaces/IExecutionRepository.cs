using CodeSync.ExecutionService.Models;

namespace CodeSync.ExecutionService.Interfaces
{
    public interface IExecutionRepository
    {
        Task<ExecutionJob> CreateAsync(ExecutionJob job);
        Task<ExecutionJob?> FindByIdAsync(int id);
        Task<ExecutionJob> UpdateAsync(ExecutionJob job);
        Task<List<ExecutionJob>> FindByProjectAsync(Guid projectId);
        Task<int> CountAllAsync();
        Task<int> CountByStatusAsync(string status);
    }
}