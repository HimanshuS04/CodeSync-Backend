namespace CodeSync.ProjectService.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid FileId { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int? ParentCommentId { get; set; }
        public bool IsResolved { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Comment? ParentComment { get; set; }
        public List<Comment> Replies { get; set; } = new();
    }
}