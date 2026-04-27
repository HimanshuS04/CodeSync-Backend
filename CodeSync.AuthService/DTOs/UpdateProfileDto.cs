using System.ComponentModel.DataAnnotations;

namespace CodeSync.AuthService.DTOs
{
    public class UpdateProfileDto
    {
        [RegularExpression(@"^[a-zA-Z0-9_]{3,20}$",
            ErrorMessage = "Username must be 3-20 characters, letters, numbers and underscore only")]
        public string? Username { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }
    }
}