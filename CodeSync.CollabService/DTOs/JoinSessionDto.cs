using System.ComponentModel.DataAnnotations;

namespace CodeSync.CollabService.DTOs
{
    public class JoinSessionDto
    {
        [Required]
        public Guid SessionId { get; set; }

        [Required]
        public string Username { get; set; }
            = string.Empty;
    }
}