using System.ComponentModel.DataAnnotations;

namespace CodeSync.AuthService.DTOs
{
    public class RegisterDto
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9_]{3,20}$",
            ErrorMessage = "Username must be 3-20 characters, letters, numbers and underscore only")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;
    }
}