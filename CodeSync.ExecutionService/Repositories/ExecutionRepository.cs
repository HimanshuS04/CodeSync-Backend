using CodeSync.ExecutionService.Data;
using CodeSync.ExecutionService.Interfaces;
using CodeSync.ExecutionService.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.ExecutionService.Repositories
{
    public class ExecutionRepository : IExecutionRepository
    {
        private readonly ExecutionDbContext _context;

        public ExecutionRepository(ExecutionDbContext context)
        {
            _context = context;
        }

        public async Task<ExecutionJob> CreateAsync(
            ExecutionJob job)
        {
            _context.ExecutionJobs.Add(job);
            await _context.SaveChangesAsync();
            return job;
        }

        public async Task<ExecutionJob?> FindByIdAsync(int id)
            => await _context.ExecutionJobs
                .FirstOrDefaultAsync(j => j.Id == id);

        public async Task<ExecutionJob> UpdateAsync(
            ExecutionJob job)
        {
            _context.ExecutionJobs.Update(job);
            await _context.SaveChangesAsync();
            return job;
        }

        public async Task<List<ExecutionJob>> FindByProjectAsync(
            Guid projectId)
            => await _context.ExecutionJobs
                .Where(j => j.ProjectId == projectId)
                .OrderByDescending(j => j.CreatedAt)
                .Take(20)
                .ToListAsync();
        public async Task<int> CountAllAsync()
            => await _context.ExecutionJobs.CountAsync();

        public async Task<int> CountByStatusAsync(string status)
            => await _context.ExecutionJobs
                .CountAsync(j => j.Status == status);
    }
}