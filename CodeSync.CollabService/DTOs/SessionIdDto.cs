using System.ComponentModel.DataAnnotations;

namespace CodeSync.CollabService.DTOs
{
    public class SessionIdDto
    {
        [Required]
        public Guid SessionId { get; set; }
    }
}