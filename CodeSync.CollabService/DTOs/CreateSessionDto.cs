using System.ComponentModel.DataAnnotations;

namespace CodeSync.CollabService.DTOs
{
    public class CreateSessionDto
    {
        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        public Guid FileId { get; set; }

        [Required]
        public Guid OwnerId { get; set; }

        public string InitialContent { get; set; }
            = string.Empty;
    }
}