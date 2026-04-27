using CodeSync.ProjectService.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.ProjectService.Data
{
    public class ProjectDbContext : DbContext
    {
        public ProjectDbContext(
            DbContextOptions<ProjectDbContext> options)
            : base(options) { }

        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<StarredProject> StarredProjects { get; set; }
        public DbSet<CodeFile> CodeFiles { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(p => p.ProjectId);
                entity.HasIndex(p => p.OwnerId);
            });

            modelBuilder.Entity<ProjectMember>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasOne(m => m.Project)
                      .WithMany(p => p.Members)
                      .HasForeignKey(m => m.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(m =>
                    new { m.ProjectId, m.UserId }).IsUnique();
            });

            modelBuilder.Entity<StarredProject>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasOne(s => s.Project)
                      .WithMany()
                      .HasForeignKey(s => s.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(s =>
                    new { s.ProjectId, s.UserId }).IsUnique();
            });

            modelBuilder.Entity<CodeFile>(entity =>
            {
                entity.HasKey(f => f.FileId);
                entity.HasIndex(f => f.ProjectId);
                entity.HasIndex(f =>
                    new { f.ProjectId, f.Path }).IsUnique();
                entity.HasOne(f => f.Project)
                      .WithMany()
                      .HasForeignKey(f => f.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);
                // Soft delete filter
                entity.HasQueryFilter(f => !f.IsDeleted);
            });
        }
    }
}