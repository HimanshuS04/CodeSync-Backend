using System.ComponentModel.DataAnnotations;

namespace CodeSync.ProjectService.DTOs
{
    public class CreateFileDto
    {
        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Path { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
    }
}