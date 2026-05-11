using StackExchange.Redis;
using System.Text.Json;
using CodeSync.CollabService.Interfaces;
namespace CodeSync.CollabService.Services
{
    public class RedisService:IRedisService
    {
        private readonly IDatabase _db;

        public RedisService(IConfiguration config)
        {
            var redis = ConnectionMultiplexer.Connect(
                config.GetConnectionString("Redis")!);
            _db = redis.GetDatabase();
        }

        // Document content
        public async Task SetDocumentAsync(
            Guid sessionId, string content)
        {
            await _db.StringSetAsync(
                $"session:{sessionId}:document",
                content,
                TimeSpan.FromHours(24));
        }

        public async Task<string> GetDocumentAsync(
            Guid sessionId)
        {
            var val = await _db.StringGetAsync(
                $"session:{sessionId}:document");
            return val.IsNullOrEmpty
                ? string.Empty : val.ToString();
        }

        // Participants
        public async Task AddParticipantAsync(
            Guid sessionId, string userId,
            string username, string color)
        {
            var key = $"session:{sessionId}:participants";
            var participant = JsonSerializer.Serialize(
                new { userId, username, color });
            await _db.HashSetAsync(
                key, userId, participant);
            await _db.KeyExpireAsync(
                key, TimeSpan.FromHours(24));
        }

        public async Task RemoveParticipantAsync(
            Guid sessionId, string userId)
        {
            await _db.HashDeleteAsync(
                $"session:{sessionId}:participants",
                userId);
        }

        public async Task<List<dynamic>> GetParticipantsAsync(
            Guid sessionId)
        {
            var entries = await _db.HashGetAllAsync(
                $"session:{sessionId}:participants");
            return entries
                .Select(e => JsonSerializer
                    .Deserialize<dynamic>(
                        e.Value.ToString())!)
                .ToList();
        }

        // Cursor positions
        public async Task SetCursorAsync(
            Guid sessionId, string userId,
            int line, int col)
        {
            var key = $"session:{sessionId}:cursors";
            var cursor = JsonSerializer.Serialize(
                new { line, col });
            await _db.HashSetAsync(key, userId, cursor);
            await _db.KeyExpireAsync(
                key, TimeSpan.FromHours(24));
        }

        // Cleanup session from Redis
        public async Task CleanupSessionAsync(
            Guid sessionId)
        {
            await _db.KeyDeleteAsync(
                $"session:{sessionId}:document");
            await _db.KeyDeleteAsync(
                $"session:{sessionId}:participants");
            await _db.KeyDeleteAsync(
                $"session:{sessionId}:cursors");
        }
    }
}