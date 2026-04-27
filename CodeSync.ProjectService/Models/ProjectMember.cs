namespace CodeSync.ProjectService.Models
{
    public class ProjectMember
    {
        public int Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }

        // OWNER / EDITOR / VIEWER
        public string Role { get; set; } = "EDITOR";
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Project? Project { get; set; }
    }
}