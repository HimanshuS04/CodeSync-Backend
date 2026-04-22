namespace CodeSync.ExecutionService.DTOs
{
    public class ExecutionResultDto
    {
        public int Id { get; set; }
        public string Language { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Stdout { get; set; }
        public string? Stderr { get; set; }
        public string? CompileOutput { get; set; }
        public int? ExecutionTimeMs { get; set; }
        public int? MemoryUsedKb { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}