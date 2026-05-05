using CodeSync.ProjectService.Models;

namespace CodeSync.ProjectService.Interfaces
{
    public interface ICommentRepository
    {
        Task<Comment> CreateAsync(Comment comment);
        Task<Comment?> FindByIdAsync(int id);
        Task<List<Comment>> FindByFileIdAsync(Guid fileId);
        Task<Comment> UpdateAsync(Comment comment);
        Task DeleteAsync(int id);
        Task<int> CountByFileAsync(Guid fileId);
    }
}