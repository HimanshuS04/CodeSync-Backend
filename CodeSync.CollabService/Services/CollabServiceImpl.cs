using CodeSync.CollabService.DTOs;
using CodeSync.CollabService.Helpers;
using CodeSync.CollabService.Interfaces;
using CodeSync.CollabService.Models;

namespace CodeSync.CollabService.Services
{
    public class CollabServiceImpl : ICollabService
    {
        private readonly ICollabRepository _repo;
        private readonly RedisService _redis;

        public CollabServiceImpl(
            ICollabRepository repo,
            RedisService redis)
        {
            _repo = repo;
            _redis = redis;
        }

        public async Task<SessionResponseDto>
            CreateSessionAsync(
                Guid userId, CreateSessionDto dto)
        {
            // Only owner can create session
            if (dto.OwnerId != userId)
                throw new Exception(
                    "Only project owner can start session");

            var session = new CollabSession
            {
                ProjectId = dto.ProjectId,
                FileId = dto.FileId,
                OwnerId = userId,
                Status = "ACTIVE",
                MaxParticipants = 5
            };

            await _repo.CreateAsync(session);

            // Add owner as HOST participant
            var hostParticipant = new Participant
            {
                SessionId = session.SessionId,
                UserId = userId,
                Username = "Host",
                Role = "HOST",
                Color = ColorHelper.GetNextColor()
            };
            await _repo.AddParticipantAsync(hostParticipant);

            // Store initial document in Redis
            await _redis.SetDocumentAsync(
                session.SessionId, dto.InitialContent);

            // Add host to Redis participants
            await _redis.AddParticipantAsync(
                session.SessionId,
                userId.ToString(),
                "Host",
                hostParticipant.Color);

            return await MapToDto(session);
        }

        public async Task<SessionResponseDto>
            JoinSessionAsync(
                Guid userId, JoinSessionDto dto)
        {
            var session = await _repo
                .FindByIdAsync(dto.SessionId)
                ?? throw new Exception("Session not found");

            if (session.Status != "ACTIVE")
                throw new Exception("Session is not active");

            // Check participant limit
            var activeCount = session.Participants
                .Count(p => p.LeftAt == null);
            if (activeCount >= session.MaxParticipants)
                throw new Exception("Session is full");

            // Check if already participant
            var existing = await _repo
                .FindParticipantAsync(
                    dto.SessionId, userId);

            if (existing != null)
            {
                // Rejoin - clear left time
                existing.LeftAt = null;
                await _repo.UpdateParticipantAsync(existing);
            }
            else
            {
                // New participant
                var participant = new Participant
                {
                    SessionId = dto.SessionId,
                    UserId = userId,
                    Username = dto.Username,
                    Role = "EDITOR",
                    Color = ColorHelper.GetNextColor()
                };
                await _repo.AddParticipantAsync(participant);

                await _redis.AddParticipantAsync(
                    dto.SessionId,
                    userId.ToString(),
                    dto.Username,
                    participant.Color);
            }

            return await MapToDto(session);
        }

        public async Task LeaveSessionAsync(
            Guid userId, Guid sessionId)
        {
            var participant = await _repo
                .FindParticipantAsync(sessionId, userId)
                ?? throw new Exception(
                    "Participant not found");

            participant.LeftAt = DateTime.UtcNow;
            await _repo.UpdateParticipantAsync(participant);

            await _redis.RemoveParticipantAsync(
                sessionId, userId.ToString());
        }

        public async Task EndSessionAsync(
            Guid userId, Guid sessionId)
        {
            var session = await _repo
                .FindByIdAsync(sessionId)
                ?? throw new Exception("Session not found");

            if (session.OwnerId != userId)
                throw new Exception(
                    "Only owner can end session");

            session.Status = "ENDED";
            session.EndedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(session);

            await _redis.CleanupSessionAsync(sessionId);
        }

        public async Task<SessionResponseDto> GetSessionAsync(
            Guid sessionId)
        {
            var session = await _repo
                .FindByIdAsync(sessionId)
                ?? throw new Exception("Session not found");
            return await MapToDto(session);
        }

        public async Task<List<SessionResponseDto>>
            GetActiveSessionsByProjectAsync(Guid projectId)
        {
            var sessions = await _repo
                .FindActiveByProjectAsync(projectId);
            var result = new List<SessionResponseDto>();
            foreach (var s in sessions)
                result.Add(await MapToDto(s));
            return result;
        }

        private async Task<SessionResponseDto> MapToDto(
            CollabSession s)
        {
            return new SessionResponseDto
            {
                SessionId = s.SessionId,
                ProjectId = s.ProjectId,
                FileId = s.FileId,
                OwnerId = s.OwnerId,
                Status = s.Status,
                MaxParticipants = s.MaxParticipants,
                CreatedAt = s.CreatedAt,
                Participants = s.Participants
                    .Where(p => p.LeftAt == null)
                    .Select(p => new ParticipantDto
                    {
                        UserId = p.UserId,
                        Username = p.Username,
                        Role = p.Role,
                        Color = p.Color
                    }).ToList()
            };
        }
    }
}