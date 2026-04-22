namespace CodeSync.ProjectService.Models
{
    public class Snapshot
    {
        public int Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid FileId { get; set; }
        public Guid AuthorId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}