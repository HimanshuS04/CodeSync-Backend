using System.ComponentModel.DataAnnotations;

namespace CodeSync.ProjectService.DTOs
{
    public class AddMemberByUsernameDto
    {
        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;
    }
}