using CodeSync.ProjectService.DTOs;
using CodeSync.ProjectService.Interfaces;
using CodeSync.ProjectService.Models;

namespace CodeSync.ProjectService.Services
{
    public class ProjectServiceImpl : IProjectService
    {
        private readonly IProjectRepository _repo;
        private readonly ICacheService _cache;

        private const string PublicProjectsKey = "projects:public";

        public ProjectServiceImpl(
            IProjectRepository repo,
            ICacheService cache)
        {
            _repo = repo;
            _cache = cache;
        }

        public async Task<ProjectResponseDto> CreateProjectAsync(
            Guid ownerId, CreateProjectDto dto)
        {
            var project = new Project
            {
                OwnerId = ownerId,
                Name = dto.Name,
                Description = dto.Description,
                Language = dto.Language,
                Visibility = dto.Visibility
            };

            project.Members.Add(new ProjectMember
            {
                ProjectId = project.ProjectId,
                UserId = ownerId,
                Role = "OWNER"
            });

            await _repo.CreateAsync(project);

            // Invalidate public projects cache
            await _cache.RemoveAsync(PublicProjectsKey);

            return MapToDto(project);
        }

        public async Task<ProjectResponseDto> GetProjectByIdAsync(
            Guid projectId)
        {
            // Try cache first
            var cacheKey = $"project:{projectId}";
            var cached = await _cache
                .GetAsync<ProjectResponseDto>(cacheKey);
            if (cached != null) return cached;

            // DB fallback
            var project = await _repo.FindByIdAsync(projectId)
                ?? throw new Exception("Project not found");

            var dto = MapToDto(project);

            // Set cache
            await _cache.SetAsync(cacheKey, dto,
                TimeSpan.FromMinutes(10));

            return dto;
        }

        public async Task<List<ProjectResponseDto>> GetMyProjectsAsync(
            Guid userId)
        {
            // No cache - always fresh for own projects
            var projects = await _repo.FindByOwnerIdAsync(userId);
            return projects.Select(MapToDto).ToList();
        }

        public async Task<List<ProjectResponseDto>> GetPublicProjectsAsync()
        {
            // Try cache first
            var cached = await _cache
                .GetAsync<List<ProjectResponseDto>>(PublicProjectsKey);
            if (cached != null) return cached;

            // DB fallback
            var projects = await _repo.FindPublicAsync();
            var dtos = projects.Select(MapToDto).ToList();

            // Set cache
            await _cache.SetAsync(PublicProjectsKey, dtos,
                TimeSpan.FromMinutes(5));

            return dtos;
        }

        public async Task<List<ProjectResponseDto>> SearchProjectsAsync(
            string query)
        {
            var projects = await _repo.SearchAsync(query);
            return projects.Select(MapToDto).ToList();
        }

        public async Task<ProjectResponseDto> UpdateProjectAsync(
            Guid projectId, Guid userId, UpdateProjectDto dto)
        {
            var project = await _repo.FindByIdAsync(projectId)
                ?? throw new Exception("Project not found");

            if (project.OwnerId != userId)
                throw new Exception("Not authorized");

            if (dto.Name != null) project.Name = dto.Name;
            if (dto.Description != null)
                project.Description = dto.Description;
            if (dto.Visibility != null)
                project.Visibility = dto.Visibility;

            await _repo.UpdateAsync(project);

            // Invalidate caches
            await _cache.RemoveAsync($"project:{projectId}");
            await _cache.RemoveAsync(PublicProjectsKey);

            return MapToDto(project);
        }

        public async Task DeleteProjectAsync(
            Guid projectId, Guid userId)
        {
            var project = await _repo.FindByIdAsync(projectId)
                ?? throw new Exception("Project not found");

            if (project.OwnerId != userId)
                throw new Exception("Not authorized");

            await _repo.DeleteAsync(projectId);

            // Invalidate caches
            await _cache.RemoveAsync($"project:{projectId}");
            await _cache.RemoveAsync(PublicProjectsKey);
        }

        public async Task StarProjectAsync(Guid projectId)
        {
            var project = await _repo.FindByIdAsync(projectId)
                ?? throw new Exception("Project not found");

            project.StarCount++;
            await _repo.UpdateAsync(project);

            // Invalidate caches
            await _cache.RemoveAsync($"project:{projectId}");
            await _cache.RemoveAsync(PublicProjectsKey);
        }

        public async Task AddMemberAsync(
            Guid projectId, Guid ownerId, Guid newUserId)
        {
            var project = await _repo.FindByIdAsync(projectId)
                ?? throw new Exception("Project not found");

            if (project.OwnerId != ownerId)
                throw new Exception("Not authorized");

            var existing = await _repo.FindMemberAsync(
                projectId, newUserId);
            if (existing != null)
                throw new Exception("Already a member");

            await _repo.AddMemberAsync(new ProjectMember
            {
                ProjectId = projectId,
                UserId = newUserId,
                Role = "EDITOR"
            });
        }

        public async Task RemoveMemberAsync(
            Guid projectId, Guid ownerId, Guid userId)
        {
            var project = await _repo.FindByIdAsync(projectId)
                ?? throw new Exception("Project not found");

            if (project.OwnerId != ownerId)
                throw new Exception("Not authorized");

            await _repo.RemoveMemberAsync(projectId, userId);
        }

        private static ProjectResponseDto MapToDto(Project p) => new()
        {
            ProjectId = p.ProjectId,
            OwnerId = p.OwnerId,
            Name = p.Name,
            Description = p.Description,
            Language = p.Language,
            Visibility = p.Visibility,
            StarCount = p.StarCount,
            ForkCount = p.ForkCount,
            IsArchived = p.IsArchived,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };
    }
}