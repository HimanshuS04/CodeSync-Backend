using CodeSync.CollabService.DTOs;

namespace CodeSync.CollabService.Interfaces
{
    public interface ICollabService
    {
        Task<SessionResponseDto> CreateSessionAsync(Guid userId, CreateSessionDto dto);
        Task<SessionResponseDto> JoinSessionAsync(Guid userId, JoinSessionDto dto);
        Task LeaveSessionAsync(Guid userId, Guid sessionId);
        Task EndSessionAsync(Guid userId, Guid sessionId);
        Task<SessionResponseDto> GetSessionAsync(Guid sessionId);
        Task<List<SessionResponseDto>> GetActiveSessionsByProjectAsync(Guid projectId);
        Task<List<SessionResponseDto>> GetAllActiveSessionsAsync();
    }
}