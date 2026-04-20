using CodeSync.ProjectService.Data;
using CodeSync.ProjectService.Interfaces;
using CodeSync.ProjectService.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.ProjectService.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ProjectDbContext _context;

        public ProjectRepository(ProjectDbContext context)
        {
            _context = context;
        }

        public async Task<Project> CreateAsync(Project project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<Project?> FindByIdAsync(Guid projectId)
            => await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p =>
                    p.ProjectId == projectId);

        public async Task<List<Project>> FindByOwnerIdAsync(
            Guid ownerId)
            => await _context.Projects
                .Where(p => p.OwnerId == ownerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

        public async Task<List<Project>> FindPublicAsync()
            => await _context.Projects
                .Where(p => p.Visibility == "PUBLIC"
                         && !p.IsArchived)
                .OrderByDescending(p => p.StarCount)
                .ToListAsync();

        public async Task<List<Project>> SearchAsync(string query)
            => await _context.Projects
                .Where(p => p.Visibility == "PUBLIC"
                    && p.Name.ToLower()
                       .Contains(query.ToLower()))
                .ToListAsync();

        public async Task<Project> UpdateAsync(Project project)
        {
            project.UpdatedAt = DateTime.UtcNow;
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task DeleteAsync(Guid projectId)
        {
            var project = await FindByIdAsync(projectId);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddMemberAsync(ProjectMember member)
        {
            _context.ProjectMembers.Add(member);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveMemberAsync(
            Guid projectId, Guid userId)
        {
            var member = await FindMemberAsync(
                projectId, userId);
            if (member != null)
            {
                _context.ProjectMembers.Remove(member);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ProjectMember?> FindMemberAsync(
            Guid projectId, Guid userId)
            => await _context.ProjectMembers
                .FirstOrDefaultAsync(m =>
                    m.ProjectId == projectId
                    && m.UserId == userId);
        public async Task<StarredProject?> FindStarAsync(
            Guid projectId, Guid userId)
            => await _context.StarredProjects
                .FirstOrDefaultAsync(s =>
                    s.ProjectId == projectId
                    && s.UserId == userId);

        public async Task AddStarAsync(StarredProject star)
        {
            _context.StarredProjects.Add(star);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveStarAsync(StarredProject star)
        {
            _context.StarredProjects.Remove(star);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Guid>> GetStarredProjectIdsAsync(
            Guid userId)
            => await _context.StarredProjects
                .Where(s => s.UserId == userId)
                .Select(s => s.ProjectId)
                .ToListAsync();
    }
}