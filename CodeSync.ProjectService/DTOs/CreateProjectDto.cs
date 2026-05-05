using System.ComponentModel.DataAnnotations;

namespace CodeSync.ProjectService.DTOs
{
    public class CreateProjectDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string Language { get; set; } = string.Empty;

        public string Visibility { get; set; } = "PUBLIC";
    }
}