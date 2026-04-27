namespace CodeSync.ProjectService.DTOs
{
    public class ProjectResponseDto
    {
        public Guid ProjectId { get; set; }
        public Guid OwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Language { get; set; } = string.Empty;
        public string Visibility { get; set; } = string.Empty;
        public int StarCount { get; set; }
        public int ForkCount { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}