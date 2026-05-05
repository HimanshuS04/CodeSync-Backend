using CodeSync.CollabService.Models;

namespace CodeSync.CollabService.Interfaces
{
    public interface ICollabRepository
    {
        Task<CollabSession> CreateAsync(
            CollabSession session);
        Task<CollabSession?> FindByIdAsync(Guid sessionId);
        Task<CollabSession> UpdateAsync(
            CollabSession session);
        Task AddParticipantAsync(Participant participant);
        Task<Participant?> FindParticipantAsync(
            Guid sessionId, Guid userId);
        Task UpdateParticipantAsync(
            Participant participant);
        Task<List<CollabSession>> FindActiveByProjectAsync(
            Guid projectId);
        Task<List<CollabSession>> FindAllActiveAsync();
    }
}