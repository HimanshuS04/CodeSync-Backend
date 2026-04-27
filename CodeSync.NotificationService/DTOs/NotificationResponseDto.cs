namespace CodeSync.NotificationService.DTOs
{
    public class NotificationResponseDto
    {
        public int Id { get; set; }
        public Guid RecipientId { get; set; }
        public Guid ActorId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string RelatedId { get; set; } = string.Empty;
        public string RelatedType { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}