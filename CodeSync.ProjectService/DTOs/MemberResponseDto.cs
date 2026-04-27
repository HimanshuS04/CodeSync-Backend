namespace CodeSync.ProjectService.DTOs
{
    public class MemberResponseDto
    {
        public Guid UserId { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}