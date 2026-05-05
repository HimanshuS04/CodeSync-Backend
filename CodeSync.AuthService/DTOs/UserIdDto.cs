using System.ComponentModel.DataAnnotations;

namespace CodeSync.AuthService.DTOs
{
    public class UserIdDto
    {
        [Required]
        public Guid UserId { get; set; }
    }
}