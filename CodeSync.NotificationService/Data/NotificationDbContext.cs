using CodeSync.NotificationService.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.NotificationService.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(
            DbContextOptions<NotificationDbContext> options)
            : base(options) { }

        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.HasIndex(n => n.RecipientId);
                entity.HasIndex(n => n.IsRead);
            });
        }
    }
}