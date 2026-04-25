using CodeSync.ProjectService.DTOs;

namespace CodeSync.ProjectService.Interfaces
{
    public interface ICommentService
    {
        Task<CommentResponseDto> AddCommentAsync(
            Guid userId, AddCommentDto dto);
        Task<CommentResponseDto> ReplyAsync(
            Guid userId, ReplyCommentDto dto);
        Task<List<CommentResponseDto>> GetByFileAsync(
            Guid fileId);
        Task ResolveAsync(Guid userId, int commentId);
        Task UnresolveAsync(Guid userId, int commentId);
        Task DeleteAsync(Guid userId, int commentId);
        Task<int> GetCountAsync(Guid fileId);
    }
}