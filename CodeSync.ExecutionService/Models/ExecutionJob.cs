namespace CodeSync.ExecutionService.Models
{
    public class ExecutionJob
    {
        public int Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid FileId { get; set; }
        public Guid UserId { get; set; }
        public string Language { get; set; } = string.Empty;
        public string SourceCode { get; set; } = string.Empty;
        public string? Stdin { get; set; }
        public string? Stdout { get; set; }
        public string? Stderr { get; set; }
        public string? CompileOutput { get; set; }
        public string Status { get; set; } = "QUEUED";
        public int? ExecutionTimeMs { get; set; }
        public int? MemoryUsedKb { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}