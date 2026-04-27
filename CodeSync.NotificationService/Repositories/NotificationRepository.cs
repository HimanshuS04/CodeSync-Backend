using CodeSync.NotificationService.Data;
using CodeSync.NotificationService.Interfaces;
using CodeSync.NotificationService.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.NotificationService.Repositories
{
    public class NotificationRepository
        : INotificationRepository
    {
        private readonly NotificationDbContext _context;

        public NotificationRepository(
            NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<Notification> CreateAsync(
            Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<List<Notification>>
            GetByRecipientAsync(Guid recipientId)
            => await _context.Notifications
                .Where(n => n.RecipientId == recipientId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();

        public async Task<int> GetUnreadCountAsync(
            Guid recipientId)
            => await _context.Notifications
                .CountAsync(n =>
                    n.RecipientId == recipientId
                    && !n.IsRead);

        public async Task<Notification?> FindByIdAsync(int id)
            => await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id);

        public async Task<Notification> UpdateAsync(
            Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task MarkAllReadAsync(Guid recipientId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.RecipientId == recipientId
                         && !n.IsRead)
                .ToListAsync();

            foreach (var n in notifications)
                n.IsRead = true;

            await _context.SaveChangesAsync();
        }
    }
}