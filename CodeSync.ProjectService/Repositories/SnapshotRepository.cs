using CodeSync.ProjectService.Data;
using CodeSync.ProjectService.Interfaces;
using CodeSync.ProjectService.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.ProjectService.Repositories
{
    public class SnapshotRepository : ISnapshotRepository
    {
        private readonly ProjectDbContext _context;

        public SnapshotRepository(ProjectDbContext context)
        {
            _context = context;
        }

        public async Task<Snapshot> CreateAsync(
            Snapshot snapshot)
        {
            _context.Snapshots.Add(snapshot);
            await _context.SaveChangesAsync();
            return snapshot;
        }

        public async Task<Snapshot?> FindByIdAsync(int id)
            => await _context.Snapshots
                .FirstOrDefaultAsync(s => s.Id == id);

        public async Task<List<Snapshot>> FindByFileIdAsync(
            Guid fileId)
            => await _context.Snapshots
                .Where(s => s.FileId == fileId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

        public async Task<Snapshot?> FindLatestByFileAsync(
            Guid fileId)
            => await _context.Snapshots
                .Where(s => s.FileId == fileId)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
    }
}