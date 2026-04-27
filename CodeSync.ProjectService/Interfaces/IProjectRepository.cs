using CodeSync.ProjectService.Models;

namespace CodeSync.ProjectService.Interfaces
{
    public interface IProjectRepository
    {
        Task<Project> CreateAsync(Project project);
        Task<Project?> FindByIdAsync(Guid projectId);
        Task<List<Project>> FindByOwnerIdAsync(Guid ownerId);
        Task<List<Project>> FindPublicAsync();
        Task<List<Project>> SearchAsync(string query);
        Task<Project> UpdateAsync(Project project);
        Task DeleteAsync(Guid projectId);
        Task AddMemberAsync(ProjectMember member);
        Task RemoveMemberAsync(Guid projectId, Guid userId);
        Task<ProjectMember?> FindMemberAsync(
            Guid projectId, Guid userId);

        // Star methods
        Task<StarredProject?> FindStarAsync(
            Guid projectId, Guid userId);
        Task AddStarAsync(StarredProject star);
        Task RemoveStarAsync(StarredProject star);
        Task<List<Guid>> GetStarredProjectIdsAsync(Guid userId);
    }
}