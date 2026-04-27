namespace CodeSync.CollabService.Models
{
    public class Participant
    {
        public int Id { get; set; }
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = "EDITOR";
        public string Color { get; set; } = "#3d5afe";
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LeftAt { get; set; }
        public int CursorLine { get; set; } = 0;
        public int CursorCol { get; set; } = 0;

        public CollabSession? Session { get; set; }
    }
}