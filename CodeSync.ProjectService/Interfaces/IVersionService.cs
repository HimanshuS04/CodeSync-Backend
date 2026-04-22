using CodeSync.ProjectService.DTOs;

namespace CodeSync.ProjectService.Interfaces
{
    public interface IVersionService
    {
        Task<SnapshotResponseDto> CreateSnapshotAsync(
            Guid userId, CreateSnapshotDto dto);
        Task<List<SnapshotResponseDto>> GetFileHistoryAsync(
            Guid fileId);
        Task<SnapshotResponseDto> GetSnapshotByIdAsync(int id);
        Task<SnapshotResponseDto> RestoreSnapshotAsync(
            Guid userId, int snapshotId);
    }
}