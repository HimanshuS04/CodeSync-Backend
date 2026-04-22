using System.ComponentModel.DataAnnotations;

namespace CodeSync.ProjectService.DTOs
{
    public class UpdateFileContentDto
    {
        [Required]
        public Guid FileId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;
    }
}