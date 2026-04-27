using System.ComponentModel.DataAnnotations;

namespace CodeSync.ProjectService.DTOs
{
    public class FileIdDto
    {
        [Required]
        public Guid FileId { get; set; }
    }
}