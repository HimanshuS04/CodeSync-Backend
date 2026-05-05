using System.ComponentModel.DataAnnotations;

namespace CodeSync.ProjectService.DTOs
{
    public class SnapshotIdDto
    {
        [Required]
        public int SnapshotId { get; set; }
    }
}