using CodeSync.ProjectService.DTOs;

namespace CodeSync.ProjectService.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectResponseDto> CreateProjectAsync(
            Guid ownerId, CreateProjectDto dto);
        Task<ProjectResponseDto> GetProjectByIdAsync(Guid projectId);
        Task<List<ProjectResponseDto>> GetMyProjectsAsync(Guid userId);
        Task<List<ProjectResponseDto>> GetPublicProjectsAsync();
        Task<List<ProjectResponseDto>> SearchProjectsAsync(string query);
        Task<ProjectResponseDto> UpdateProjectAsync(
            Guid projectId, Guid userId, UpdateProjectDto dto);
        Task DeleteProjectAsync(Guid projectId, Guid userId);
        Task StarProjectAsync(Guid projectId);
        Task AddMemberAsync(Guid projectId, Guid ownerId, Guid newUserId);
        Task RemoveMemberAsync(Guid projectId, Guid ownerId, Guid userId);
    }
}