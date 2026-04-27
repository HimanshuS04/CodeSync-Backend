using CodeSync.CollabService.Data;
using CodeSync.CollabService.Interfaces;
using CodeSync.CollabService.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.CollabService.Repositories
{
    public class CollabRepository : ICollabRepository
    {
        private readonly CollabDbContext _context;

        public CollabRepository(CollabDbContext context)
        {
            _context = context;
        }

        public async Task<CollabSession> CreateAsync(
            CollabSession session)
        {
            _context.CollabSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<CollabSession?> FindByIdAsync(
            Guid sessionId)
            => await _context.CollabSessions
                .Include(s => s.Participants)
                .FirstOrDefaultAsync(
                    s => s.SessionId == sessionId);

        public async Task<CollabSession> UpdateAsync(
            CollabSession session)
        {
            _context.CollabSessions.Update(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task AddParticipantAsync(
            Participant participant)
        {
            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();
        }

        public async Task<Participant?> FindParticipantAsync(
            Guid sessionId, Guid userId)
            => await _context.Participants
                .FirstOrDefaultAsync(p =>
                    p.SessionId == sessionId
                    && p.UserId == userId);

        public async Task UpdateParticipantAsync(
            Participant participant)
        {
            _context.Participants.Update(participant);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CollabSession>>
            FindActiveByProjectAsync(Guid projectId)
            => await _context.CollabSessions
                .Include(s => s.Participants)
                .Where(s =>
                    s.ProjectId == projectId
                    && s.Status == "ACTIVE")
                .ToListAsync();
        public async Task<List<CollabSession>>FindAllActiveAsync()
            => await _context.CollabSessions
                .Include(s => s.Participants)
                .Where(s => s.Status == "ACTIVE")
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
    }
}