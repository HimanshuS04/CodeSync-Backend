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

        public VersionServiceImpl(
            ISnapshotRepository snapshotRepo,
            IFileRepository fileRepo)
        {
            _snapshotRepo = snapshotRepo;
            _fileRepo = fileRepo;
        }

        public async Task<SnapshotResponseDto>
            CreateSnapshotAsync(
                Guid userId, CreateSnapshotDto dto)
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
            return MapToDto(snapshot);
        }

        public async Task<List<SnapshotResponseDto>>
            GetFileHistoryAsync(Guid fileId)
        {
            var snapshots = await _snapshotRepo
                .FindByFileIdAsync(fileId);
            return snapshots.Select(MapToDto).ToList();
        }

        public async Task<SnapshotResponseDto>
            GetSnapshotByIdAsync(int id)
        {
            var snapshot = await _snapshotRepo
                .FindByIdAsync(id)
                ?? throw new Exception("Snapshot not found");
            return MapToDto(snapshot);
        }

        public async Task<SnapshotResponseDto>
            RestoreSnapshotAsync(
                Guid userId, int snapshotId)
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