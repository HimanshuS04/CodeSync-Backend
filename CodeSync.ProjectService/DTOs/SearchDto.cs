using System.ComponentModel.DataAnnotations;

namespace CodeSync.ProjectService.DTOs
{
    public class SearchDto
    {
        [Required]
        public string Query { get; set; } = string.Empty;
    }
}