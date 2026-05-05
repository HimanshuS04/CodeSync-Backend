using System.ComponentModel.DataAnnotations;

namespace CodeSync.NotificationService.DTOs
{
    public class BroadcastDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public List<Guid> RecipientIds { get; set; } = new();
    }
}