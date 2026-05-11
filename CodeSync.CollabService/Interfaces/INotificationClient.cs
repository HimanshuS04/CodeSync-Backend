using CodeSync.CollabService.Services;

namespace CodeSync.CollabService.Interfaces
{
    public interface INotificationClient
    {
        Task CreateAsync(object payload);
        Task<List<ProjectMemberInfo>> GetProjectMembersAsync(
            Guid projectId, string authHeader);
         Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
    }
}