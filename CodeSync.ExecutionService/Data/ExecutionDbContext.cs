using CodeSync.ExecutionService.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.ExecutionService.Data
{
    public class ExecutionDbContext : DbContext
    {
        public ExecutionDbContext(
            DbContextOptions<ExecutionDbContext> options)
            : base(options) { }

        public DbSet<ExecutionJob> ExecutionJobs { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExecutionJob>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ProjectId);
            });
        }
    }
}