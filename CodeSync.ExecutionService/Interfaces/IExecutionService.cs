using CodeSync.ExecutionService.DTOs;

namespace CodeSync.ExecutionService.Interfaces
{
    public interface IExecutionService
    {
        Task<ExecutionResultDto> RunCodeAsync(
            Guid userId, RunCodeDto dto);
        Task<ExecutionResultDto> GetResultAsync(int id);
        Task<List<ExecutionResultDto>> GetByProjectAsync(Guid projectId);
        Task<object> GetStatsAsync();
    }
}