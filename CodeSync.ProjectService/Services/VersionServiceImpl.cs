using System.Security.Cryptography;
using System.Text;
using CodeSync.ProjectService.DTOs;
using CodeSync.ProjectService.Interfaces;
using CodeSync.ProjectService.Models;

namespace CodeSync.ProjectService.Services
{
    public class VersionServiceImpl : IVersionService
    {
        private readonly ISnapshotRepository _snapshotRepo;
        private readonly IFileRepository _fileRepo;

        private readonly IProjectRepository _projectRepo;
        private readonly INotificationClient _notificationClient;

        public VersionServiceImpl(
            ISnapshotRepository snapshotRepo,
            IFileRepository fileRepo,
            IProjectRepository projectRepo,
            INotificationClient notificationClient)
        {
            _snapshotRepo = snapshotRepo;
            _fileRepo = fileRepo;
            _projectRepo = projectRepo;
            _notificationClient = notificationClient;
        }

        public async Task<SnapshotResponseDto> CreateSnapshotAsync(Guid userId, CreateSnapshotDto dto)
        {
            // Get latest snapshot for parent chain
            var latest = await _snapshotRepo
                .FindLatestByFileAsync(dto.FileId);

            // Generate SHA-256 hash
            var hash = GenerateHash(dto.Content);

            var snapshot = new Snapshot
            {
                ProjectId = dto.ProjectId,
                FileId = dto.FileId,
                AuthorId = userId,
                Message = dto.Message,
                Content = dto.Content,
                Hash = hash,
                ParentId = latest?.Id
            };

            await _snapshotRepo.CreateAsync(snapshot);
            var project = await _projectRepo.FindByIdAsync(dto.ProjectId);
            if (project != null && project.OwnerId != userId)
            {
                await _notificationClient.CreateAsync(new
                {
                    RecipientId = project.OwnerId,
                    ActorId = userId,
                    Type = "SNAPSHOT_CREATED",
                    Title = "New snapshot",
                    Message = $"A new snapshot was created: '{dto.Message}'",
                    RelatedId = snapshot.Id.ToString(),
                    RelatedType = "SNAPSHOT"
                });
            }
            return MapToDto(snapshot);
        }

        public async Task<List<SnapshotResponseDto>> GetFileHistoryAsync(Guid fileId, Guid userId)
        {
            // Get file to find projectId
            var file = await _fileRepo.FindByIdAsync(fileId)
                ?? throw new Exception("File not found");

            // Check access
            var project = await _projectRepo
                .FindByIdAsync(file.ProjectId)
                ?? throw new Exception("Project not found");

            if (project.OwnerId != userId)
            {
                var member = await _projectRepo
                    .FindMemberAsync(file.ProjectId, userId);
                if (member == null)
                    throw new Exception("Access denied");
            }

            var snapshots = await _snapshotRepo
                .FindByFileIdAsync(fileId);
            return snapshots.Select(MapToDto).ToList();
        }

        public async Task<SnapshotResponseDto> GetSnapshotByIdAsync(int id)
        {
            var snapshot = await _snapshotRepo
                .FindByIdAsync(id)
                ?? throw new Exception("Snapshot not found");
            return MapToDto(snapshot);
        }

        public async Task<SnapshotResponseDto> RestoreSnapshotAsync(Guid userId, int snapshotId)
        {
            // Get old snapshot
            var old = await _snapshotRepo
                .FindByIdAsync(snapshotId)
                ?? throw new Exception("Snapshot not found");

            // Update file content
            var file = await _fileRepo
                .FindByIdAsync(old.FileId)
                ?? throw new Exception("File not found");

            file.Content = old.Content;
            file.Size = Encoding.UTF8
                .GetByteCount(old.Content);
            file.LastEditedBy = userId;
            await _fileRepo.UpdateAsync(file);

            // Create NEW snapshot with restored content
            var latest = await _snapshotRepo
                .FindLatestByFileAsync(old.FileId);

            var restored = new Snapshot
            {
                ProjectId = old.ProjectId,
                FileId = old.FileId,
                AuthorId = userId,
                Message = $"Restored from snapshot #{snapshotId}",
                Content = old.Content,
                Hash = old.Hash,
                ParentId = latest?.Id
            };

            await _snapshotRepo.CreateAsync(restored);
            var project = await _projectRepo.FindByIdAsync(old.ProjectId);
            if (project != null && project.OwnerId != userId)
            {
                await _notificationClient.CreateAsync(new
                {
                    RecipientId = project.OwnerId,
                    ActorId = userId,
                    Type = "SNAPSHOT_RESTORED",
                    Title = "Snapshot restored",
                    Message = $"A file was restored from snapshot #{snapshotId}",
                    RelatedId = restored.Id.ToString(),
                    RelatedType = "SNAPSHOT"
                });
            }
            return MapToDto(restored);
        }

        private static string GenerateHash(string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash).ToLower();
        }

        private static SnapshotResponseDto MapToDto(
            Snapshot s) => new()
        {
            Id = s.Id,
            ProjectId = s.ProjectId,
            FileId = s.FileId,
            AuthorId = s.AuthorId,
            Message = s.Message,
            Hash = s.Hash,
            ParentId = s.ParentId,
            CreatedAt = s.CreatedAt
        };
    }
}