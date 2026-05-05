using System.ComponentModel.DataAnnotations;

namespace CodeSync.ProjectService.DTOs
{
    public class ProjectIdDto
    {
        [Required]
        public Guid ProjectId { get; set; }
    }
}