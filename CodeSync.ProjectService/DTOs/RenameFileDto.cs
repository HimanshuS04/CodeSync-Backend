using System.ComponentModel.DataAnnotations;

namespace CodeSync.ProjectService.DTOs
{
    public class RenameFileDto
    {
        [Required]
        public Guid FileId { get; set; }

        [Required]
        public string NewName { get; set; } = string.Empty;
    }
}