using CodeSync.ProjectService.Data;
using CodeSync.ProjectService.Interfaces;
using CodeSync.ProjectService.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.ProjectService.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly ProjectDbContext _context;

        public FileRepository(ProjectDbContext context)
        {
            _context = context;
        }

        public async Task<CodeFile> CreateAsync(CodeFile file)
        {
            _context.CodeFiles.Add(file);
            await _context.SaveChangesAsync();
            return file;
        }

        public async Task<CodeFile?> FindByIdAsync(Guid fileId)
            => await _context.CodeFiles
                .FirstOrDefaultAsync(f => f.FileId == fileId);

        public async Task<List<CodeFile>> FindByProjectIdAsync(
            Guid projectId)
            => await _context.CodeFiles
                .Where(f => f.ProjectId == projectId)
                .OrderBy(f => f.Path)
                .ToListAsync();

        public async Task<CodeFile?> FindByPathAsync(
            Guid projectId, string path)
            => await _context.CodeFiles
                .FirstOrDefaultAsync(f =>
                    f.ProjectId == projectId
                    && f.Path == path);

        public async Task<CodeFile> UpdateAsync(CodeFile file)
        {
            file.UpdatedAt = DateTime.UtcNow;
            _context.CodeFiles.Update(file);
            await _context.SaveChangesAsync();
            return file;
        }

        public async Task<bool> ExistsAsync(
            Guid projectId, string path)
            => await _context.CodeFiles
                .AnyAsync(f =>
                    f.ProjectId == projectId
                    && f.Path == path);

        public async Task<int> CountByProjectAsync(
            Guid projectId)
            => await _context.CodeFiles
                .CountAsync(f => f.ProjectId == projectId);
    }
}