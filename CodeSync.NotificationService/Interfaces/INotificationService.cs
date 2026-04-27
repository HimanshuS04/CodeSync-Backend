using CodeSync.NotificationService.DTOs;

namespace CodeSync.NotificationService.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationResponseDto> CreateAsync(
            CreateNotificationDto dto);
        Task<List<NotificationResponseDto>>
            GetMyNotificationsAsync(Guid userId);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task MarkReadAsync(Guid userId, int notificationId);
        Task MarkAllReadAsync(Guid userId);
    }
}