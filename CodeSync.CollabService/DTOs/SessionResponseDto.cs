namespace CodeSync.CollabService.DTOs
{
    public class SessionResponseDto
    {
        public Guid SessionId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid FileId { get; set; }
        public Guid OwnerId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int MaxParticipants { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ParticipantDto> Participants { get; set; }
            = new();
    }

    public class ParticipantDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }
}