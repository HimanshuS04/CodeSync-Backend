using CodeSync.NotificationService.DTOs;
using CodeSync.NotificationService.Interfaces;
using CodeSync.NotificationService.Models;

namespace CodeSync.NotificationService.Services
{
    public class NotificationServiceImpl
        : INotificationService
    {
        private readonly INotificationRepository _repo;

        public NotificationServiceImpl(
            INotificationRepository repo)
        {
            _repo = repo;
        }

        public async Task<NotificationResponseDto>
            CreateAsync(CreateNotificationDto dto)
        {
            var notification = new Notification
            {
                RecipientId = dto.RecipientId,
                ActorId = dto.ActorId,
                Type = dto.Type,
                Title = dto.Title,
                Message = dto.Message,
                RelatedId = dto.RelatedId,
                RelatedType = dto.RelatedType
            };

            await _repo.CreateAsync(notification);
            return MapToDto(notification);
        }

        public async Task<List<NotificationResponseDto>>
            GetMyNotificationsAsync(Guid userId)
        {
            var list = await _repo.GetByRecipientAsync(userId);
            return list.Select(MapToDto).ToList();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
            => await _repo.GetUnreadCountAsync(userId);

        public async Task MarkReadAsync(
            Guid userId, int notificationId)
        {
            var n = await _repo.FindByIdAsync(notificationId)
                ?? throw new Exception("Notification not found");

            if (n.RecipientId != userId)
                throw new Exception("Not authorized");

            n.IsRead = true;
            await _repo.UpdateAsync(n);
        }

        public async Task MarkAllReadAsync(Guid userId)
            => await _repo.MarkAllReadAsync(userId);
        public async Task BroadcastAsync(
            Guid actorId, BroadcastDto dto)
        {
            foreach (var recipientId in dto.RecipientIds)
            {
                var notification = new Notification
                {
                    RecipientId = recipientId,
                    ActorId = actorId,
                    Type = "ADMIN_BROADCAST",
                    Title = dto.Title,
                    Message = dto.Message,
                    RelatedId = "",
                    RelatedType = "BROADCAST"
                };
                await _repo.CreateAsync(notification);
            }
        }

        private static NotificationResponseDto MapToDto(
            Notification n) => new()
        {
            Id = n.Id,
            RecipientId = n.RecipientId,
            ActorId = n.ActorId,
            Type = n.Type,
            Title = n.Title,
            Message = n.Message,
            RelatedId = n.RelatedId,
            RelatedType = n.RelatedType,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        };
    }
}