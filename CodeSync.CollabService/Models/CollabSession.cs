namespace CodeSync.CollabService.Models
{
    public class CollabSession
    {
        public Guid SessionId { get; set; } = Guid.NewGuid();
        public Guid ProjectId { get; set; }
        public Guid FileId { get; set; }
        public Guid OwnerId { get; set; }
        public string Status { get; set; } = "ACTIVE";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndedAt { get; set; }
        public int MaxParticipants { get; set; } = 5;

        public List<Participant> Participants { get; set; } = new();
    }
}