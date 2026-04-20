namespace CodeSync.ProjectService.Models
{
    public class StarredProject
    {
        public int Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public DateTime StarredAt { get; set; } = DateTime.UtcNow;

        public Project? Project { get; set; }
    }
}