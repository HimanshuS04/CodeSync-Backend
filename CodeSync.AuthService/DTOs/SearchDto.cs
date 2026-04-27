using System.ComponentModel.DataAnnotations;

namespace CodeSync.AuthService.DTOs
{
    public class SearchDto
    {
        [Required]
        public string Query { get; set; } = string.Empty;
    }
}