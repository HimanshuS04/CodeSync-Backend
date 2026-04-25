using CodeSync.ProjectService.DTOs;
using CodeSync.ProjectService.Interfaces;
using CodeSync.ProjectService.Models;

namespace CodeSync.ProjectService.Services
{
    public class CommentServiceImpl : ICommentService
    {
        private readonly ICommentRepository _repo;

        public CommentServiceImpl(ICommentRepository repo)
        {
            _repo = repo;
        }

        public async Task<CommentResponseDto> AddCommentAsync(
            Guid userId, AddCommentDto dto)
        {
            var comment = new Comment
            {
                ProjectId = dto.ProjectId,
                FileId = dto.FileId,
                AuthorId = userId,
                AuthorName = dto.AuthorName,
                Content = dto.Content
            };

            await _repo.CreateAsync(comment);
            return MapToDto(comment);
        }

        public async Task<CommentResponseDto> ReplyAsync(
            Guid userId, ReplyCommentDto dto)
        {
            var parent = await _repo
                .FindByIdAsync(dto.ParentCommentId)
                ?? throw new Exception("Comment not found");

            var reply = new Comment
            {
                ProjectId = parent.ProjectId,
                FileId = parent.FileId,
                AuthorId = userId,
                AuthorName = dto.AuthorName,
                Content = dto.Content,
                ParentCommentId = dto.ParentCommentId
            };

            await _repo.CreateAsync(reply);
            return MapToDto(reply);
        }

        public async Task<List<CommentResponseDto>>
            GetByFileAsync(Guid fileId)
        {
            var comments = await _repo
                .FindByFileIdAsync(fileId);
            return comments.Select(MapToDto).ToList();
        }

        public async Task ResolveAsync(
            Guid userId, int commentId)
        {
            var comment = await _repo
                .FindByIdAsync(commentId)
                ?? throw new Exception("Comment not found");
            comment.IsResolved = true;
            await _repo.UpdateAsync(comment);
        }

        public async Task UnresolveAsync(
            Guid userId, int commentId)
        {
            var comment = await _repo
                .FindByIdAsync(commentId)
                ?? throw new Exception("Comment not found");
            comment.IsResolved = false;
            await _repo.UpdateAsync(comment);
        }

        public async Task DeleteAsync(
            Guid userId, int commentId)
        {
            var comment = await _repo
                .FindByIdAsync(commentId)
                ?? throw new Exception("Comment not found");

            if (comment.AuthorId != userId)
                throw new Exception(
                    "Can only delete own comments");

            await _repo.DeleteAsync(commentId);
        }

        public async Task<int> GetCountAsync(Guid fileId)
            => await _repo.CountByFileAsync(fileId);

        private static CommentResponseDto MapToDto(
            Comment c) => new()
        {
            Id = c.Id,
            ProjectId = c.ProjectId,
            FileId = c.FileId,
            AuthorId = c.AuthorId,
            AuthorName = c.AuthorName,
            Content = c.Content,
            ParentCommentId = c.ParentCommentId,
            IsResolved = c.IsResolved,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            Replies = c.Replies
                .OrderBy(r => r.CreatedAt)
                .Select(MapToDto)
                .ToList()
        };
    }
}