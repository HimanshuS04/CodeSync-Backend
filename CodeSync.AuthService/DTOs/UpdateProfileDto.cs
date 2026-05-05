using System.ComponentModel.DataAnnotations;

namespace CodeSync.AuthService.DTOs
{
    public class UpdateProfileDto
    {
        [RegularExpression(@"^[a-zA-Z0-9_]{3,20}$",
            ErrorMessage = "3-20 chars, letters/numbers/underscore")]
        public string? Username { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
    }
}