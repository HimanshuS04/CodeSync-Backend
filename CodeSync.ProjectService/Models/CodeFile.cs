namespace CodeSync.ProjectService.Models
{
    public class CodeFile
    {
        public Guid FileId { get; set; } = Guid.NewGuid();
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public long Size { get; set; } = 0;
        public Guid CreatedById { get; set; }
        public Guid LastEditedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        public Project? Project { get; set; }
    }
}