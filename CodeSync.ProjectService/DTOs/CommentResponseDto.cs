namespace CodeSync.ProjectService.DTOs
{
    public class CommentResponseDto
    {
        public int Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid FileId { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int? ParentCommentId { get; set; }
        public bool IsResolved { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<CommentResponseDto> Replies { get; set; } = new();
    }
}