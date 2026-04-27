using CodeSync.NotificationService.Models;

namespace CodeSync.NotificationService.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification> CreateAsync(Notification notification);
        Task<List<Notification>> GetByRecipientAsync(Guid recipientId);
        Task<int> GetUnreadCountAsync(Guid recipientId);
        Task<Notification?> FindByIdAsync(int id);
        Task<Notification> UpdateAsync(Notification notification);
        Task MarkAllReadAsync(Guid recipientId);
    }
}