using System.ComponentModel.DataAnnotations;

namespace CodeSync.NotificationService.DTOs
{
    public class CreateNotificationDto
    {
        [Required]
        public Guid RecipientId { get; set; }

        [Required]
        public Guid ActorId { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public string RelatedId { get; set; } = string.Empty;
        public string RelatedType { get; set; } = string.Empty;
    }
}