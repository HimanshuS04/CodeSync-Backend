namespace CodeSync.ProjectService.DTOs
{
    public class FileTreeItemDto
    {
        public Guid? FileId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Type { get; set; } = "file";
        public string Language { get; set; } = string.Empty;
        public List<FileTreeItemDto> Children { get; set; } = new();
    }
}