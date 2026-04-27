using CodeSync.CollabService.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.CollabService.Services
{
    public class SessionCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RedisService _redis;
        private readonly ILogger<SessionCleanupService> _logger;

        public SessionCleanupService(
            IServiceScopeFactory scopeFactory,
            RedisService redis,
            ILogger<SessionCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _redis = redis;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupInactiveSessions();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error cleaning up sessions");
                }

                // Check every 5 minutes
                await Task.Delay(
                    TimeSpan.FromMinutes(5),
                    stoppingToken);
            }
        }

        private async Task CleanupInactiveSessions()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider
                .GetRequiredService<CollabDbContext>();

            var cutoff = DateTime.UtcNow
                .AddMinutes(-30);

            var inactiveSessions = await context
                .CollabSessions
                .Where(s => s.Status == "ACTIVE"
                    && s.CreatedAt < cutoff)
                .Include(s => s.Participants)
                .ToListAsync();

            foreach (var session in inactiveSessions)
            {
                // Check if any participant
                // is still active (joined after cutoff)
                var hasActive = session.Participants
                    .Any(p => p.LeftAt == null
                        && p.JoinedAt > cutoff);

                if (!hasActive)
                {
                    session.Status = "ENDED";
                    session.EndedAt = DateTime.UtcNow;

                    foreach (var p in session.Participants
                        .Where(p => p.LeftAt == null))
                    {
                        p.LeftAt = DateTime.UtcNow;
                    }

                    await _redis.CleanupSessionAsync(
                        session.SessionId);

                    _logger.LogInformation(
                        "Auto-ended inactive session: {Id}",
                        session.SessionId);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}