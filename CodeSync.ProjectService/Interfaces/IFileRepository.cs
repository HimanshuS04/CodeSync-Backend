using CodeSync.ProjectService.Models;

namespace CodeSync.ProjectService.Interfaces
{
    public interface IFileRepository
    {
        Task<CodeFile> CreateAsync(CodeFile file);
        Task<CodeFile?> FindByIdAsync(Guid fileId);
        Task<List<CodeFile>> FindByProjectIdAsync(Guid projectId);
        Task<CodeFile?> FindByPathAsync(
            Guid projectId, string path);
        Task<CodeFile> UpdateAsync(CodeFile file);
        Task<bool> ExistsAsync(
            Guid projectId, string path);
        Task<int> CountByProjectAsync(Guid projectId);
    }
}