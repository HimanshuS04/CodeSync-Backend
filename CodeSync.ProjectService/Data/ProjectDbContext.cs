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
        }
    }
}