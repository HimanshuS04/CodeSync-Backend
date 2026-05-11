namespace CodeSync.CollabService.Interfaces
{
    public interface IRedisService
    {
        Task SetDocumentAsync(Guid sessionId, string content);
        Task<string> GetDocumentAsync(Guid sessionId);
        Task AddParticipantAsync(Guid sessionId,
            string userId, string username, string color);
        Task RemoveParticipantAsync(
            Guid sessionId, string userId);
        Task SetCursorAsync(Guid sessionId,
            string userId, int line, int col);
        Task CleanupSessionAsync(Guid sessionId);
    }
}