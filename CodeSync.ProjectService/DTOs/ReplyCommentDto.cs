using System.ComponentModel.DataAnnotations;

namespace CodeSync.ProjectService.DTOs
{
    public class ReplyCommentDto
    {
        [Required]
        public int ParentCommentId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string AuthorName { get; set; } = string.Empty;
    }
}