using System.ComponentModel.DataAnnotations;

namespace CodeSync.ProjectService.DTOs
{
    public class CommentIdDto
    {
        [Required]
        public int CommentId { get; set; }
    }
}