using CodeSync.CollabService.DTOs;
using CodeSync.CollabService.Helpers;
using CodeSync.CollabService.Interfaces;
using CodeSync.CollabService.Models;

namespace CodeSync.CollabService.Services
{
    public class CollabServiceImpl : ICollabService
    {
        private readonly ICollabRepository _repo;
        private readonly IRedisService _redis;
        private readonly INotificationClient _notificationClient;

        private readonly IHttpContextAccessor _httpContext;

        public CollabServiceImpl(
            ICollabRepository repo,
            IRedisService redis,
            INotificationClient notificationClient,
            IHttpContextAccessor httpContext)
        {
            _repo = repo;
            _redis = redis;
            _notificationClient = notificationClient;
            _httpContext = httpContext;
        }

        public async Task<SessionResponseDto> CreateSessionAsync(Guid userId, CreateSessionDto dto)
        {
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

            var hostParticipant = new Participant
            {
                SessionId = session.SessionId,
                UserId = userId,
                Username = "Host",
                Role = "HOST",
                Color = ColorHelper.GetNextColor()
            };
            await _repo.AddParticipantAsync(hostParticipant);

            await _redis.SetDocumentAsync(
                session.SessionId, dto.InitialContent);

            await _redis.AddParticipantAsync(
                session.SessionId,
                userId.ToString(),
                "Host",
                hostParticipant.Color);

            // Notify owner
            await _notificationClient.CreateAsync(new
            {
                RecipientId = userId,
                ActorId = userId,
                Type = "SESSION_STARTED",
                Title = "Session started",
                Message = "You started a live collaboration session",
                RelatedId = session.SessionId.ToString(),
                RelatedType = "SESSION"
            });

            // Notify all editors of this project
            var authHeader = _httpContext.HttpContext?
                .Request.Headers["Authorization"]
                .FirstOrDefault() ?? "";

            var members = await _notificationClient
                .GetProjectMembersAsync(
                    dto.ProjectId, authHeader);

            // Get project name
            var projectName = "a project";

            try
            {
                var projectResponse = await _notificationClient
                    .SendAsync(new HttpRequestMessage(
                        HttpMethod.Get,
                        $"http://localhost:5002/api/projects/{dto.ProjectId}")
                    {
                        Headers = { { "Authorization", authHeader } }
                    });

                if (projectResponse.IsSuccessStatusCode)
                {
                    var projectJson = await projectResponse.Content
                        .ReadAsStringAsync();
                    var projectDoc = System.Text.Json.JsonDocument
                        .Parse(projectJson);
                    projectName = projectDoc.RootElement
                        .GetProperty("name").GetString() ?? "a project";
                }
            }
            catch { }

            foreach (var member in members)
            {
                if (member.UserId == userId) continue;

                await _notificationClient.CreateAsync(new
                {
                    RecipientId = member.UserId,
                    ActorId = userId,
                    Type = "SESSION_STARTED",
                    Title = $"Session started on \"{projectName}\"",
                    Message = $"A live session was started on project \"{projectName}\"",
                    RelatedId = session.SessionId.ToString(),
                    RelatedType = "SESSION"
                });
            }

            return await MapToDto(session);
        }

        public async Task<SessionResponseDto> JoinSessionAsync(Guid userId, JoinSessionDto dto)
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
            if (session.OwnerId != userId)
            {
                await _notificationClient.CreateAsync(new
                {
                    RecipientId = session.OwnerId,
                    ActorId = userId,
                    Type = "SESSION_JOINED",
                    Title = "User joined session",
                    Message = $"{dto.Username} joined your live session",
                    RelatedId = session.SessionId.ToString(),
                    RelatedType = "SESSION"
                });
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
            var session = await _repo.FindByIdAsync(sessionId);
            if (session != null && session.OwnerId != userId)
            {
                await _notificationClient.CreateAsync(new
                {
                    RecipientId = session.OwnerId,
                    ActorId = userId,
                    Type = "SESSION_LEFT",
                    Title = "User left session",
                    Message = $"{participant.Username} left your live session",
                    RelatedId = sessionId.ToString(),
                    RelatedType = "SESSION"
                });
            }
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
            foreach (var participant in session.Participants
                .Where(p => p.UserId != userId && p.LeftAt == null))
            {
                await _notificationClient.CreateAsync(new
                {
                    RecipientId = participant.UserId,
                    ActorId = userId,
                    Type = "SESSION_ENDED",
                    Title = "Session ended",
                    Message = "The live collaboration session was ended by owner",
                    RelatedId = sessionId.ToString(),
                    RelatedType = "SESSION"
                });
            }
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
        public async Task<List<SessionResponseDto>> GetAllActiveSessionsAsync()
        {
            var sessions = await _repo.FindAllActiveAsync();
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