using CodeSync.ProjectService.DTOs;
using CodeSync.ProjectService.Interfaces;
using CodeSync.ProjectService.Models;

namespace CodeSync.ProjectService.Services
{
    public class CommentServiceImpl : ICommentService
    {
        private readonly ICommentRepository _repo;
        private readonly IProjectRepository _projectRepo;
        private readonly NotificationClient _notificationClient;

        public CommentServiceImpl(
            ICommentRepository repo,
            IProjectRepository projectRepo,
            NotificationClient notificationClient)
        {
            _repo = repo;
            _projectRepo = projectRepo;
            _notificationClient = notificationClient;
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
            var project = await _projectRepo.FindByIdAsync(dto.ProjectId);
            if (project != null && project.OwnerId != userId)
            {
                await _notificationClient.CreateAsync(new
                {
                    RecipientId = project.OwnerId,
                    ActorId = userId,
                    Type = "COMMENT_ADDED",
                    Title = "New file comment",
                    Message = $"{dto.AuthorName} commented on a file",
                    RelatedId = comment.Id.ToString(),
                    RelatedType = "COMMENT"
                });
            }
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
            if (parent.AuthorId != userId)
            {
                await _notificationClient.CreateAsync(new
                {
                    RecipientId = parent.AuthorId,
                    ActorId = userId,
                    Type = "COMMENT_REPLY",
                    Title = "New reply",
                    Message = $"{dto.AuthorName} replied to your comment",
                    RelatedId = reply.Id.ToString(),
                    RelatedType = "COMMENT"
                });
            }
            return MapToDto(reply);
        }

        public async Task<List<CommentResponseDto>> GetByFileAsync(Guid fileId, Guid userId)
        {
            // Get any comment to find projectId
            var allComments = await _repo.FindByFileIdAsync(fileId);

            if (allComments.Count > 0)
            {
                var projectId = allComments[0].ProjectId;
                var project = await _projectRepo
                    .FindByIdAsync(projectId);

                if (project != null && project.OwnerId != userId)
                {
                    var member = await _projectRepo
                        .FindMemberAsync(projectId, userId);
                    if (member == null && project.Visibility != "PUBLIC")
                        throw new Exception("Access denied");
                }
            }

            return allComments.Select(MapToDto).ToList();
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