namespace CodeSync.ProjectService.Models
{
    public class Project
    {
        public Guid ProjectId { get; set; } = Guid.NewGuid();
        public Guid OwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Language { get; set; } = string.Empty;
        public string Visibility { get; set; } = "PUBLIC";
        public int StarCount { get; set; } = 0;
        public int ForkCount { get; set; } = 0;
        public bool IsArchived { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<ProjectMember> Members { get; set; } = new();
    }
}