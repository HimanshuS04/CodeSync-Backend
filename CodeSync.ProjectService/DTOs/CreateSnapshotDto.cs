using System.ComponentModel.DataAnnotations;

namespace CodeSync.ProjectService.DTOs
{
    public class CreateSnapshotDto
    {
        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        public Guid FileId { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;
    }
}