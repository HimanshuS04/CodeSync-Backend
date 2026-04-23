using CodeSync.CollabService.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.CollabService.Data
{
    public class CollabDbContext : DbContext
    {
        public CollabDbContext(
            DbContextOptions<CollabDbContext> options)
            : base(options) { }

        public DbSet<CollabSession> CollabSessions { get; set; }
        public DbSet<Participant> Participants { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CollabSession>(entity =>
            {
                entity.HasKey(s => s.SessionId);
                entity.HasIndex(s => s.ProjectId);
                entity.HasIndex(s => s.FileId);
                entity.HasMany(s => s.Participants)
                      .WithOne(p => p.Session)
                      .HasForeignKey(p => p.SessionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Participant>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.SessionId);
                entity.HasIndex(p => p.UserId);
            });
        }
    }
}