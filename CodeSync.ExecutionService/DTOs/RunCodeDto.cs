using System.ComponentModel.DataAnnotations;

namespace CodeSync.ExecutionService.DTOs
{
    public class RunCodeDto
    {
        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        public Guid FileId { get; set; }

        [Required]
        public string Language { get; set; } = string.Empty;

        [Required]
        public string SourceCode { get; set; } = string.Empty;

        public string? Stdin { get; set; }
    }
}