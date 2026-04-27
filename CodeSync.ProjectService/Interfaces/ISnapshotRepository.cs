using CodeSync.ProjectService.Models;

namespace CodeSync.ProjectService.Interfaces
{
    public interface ISnapshotRepository
    {
        Task<Snapshot> CreateAsync(Snapshot snapshot);
        Task<Snapshot?> FindByIdAsync(int id);
        Task<List<Snapshot>> FindByFileIdAsync(Guid fileId);
        Task<Snapshot?> FindLatestByFileAsync(Guid fileId);
    }
}