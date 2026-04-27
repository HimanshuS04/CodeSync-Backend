using CodeSync.ProjectService.DTOs;
using CodeSync.ProjectService.Interfaces;
using CodeSync.ProjectService.Models;

namespace CodeSync.ProjectService.Services
{
    public class ProjectServiceImpl : IProjectService
    {
        private readonly IProjectRepository _repo;
        private readonly ICacheService _cache;
        private readonly NotificationClient _notificationClient;

        private const string PublicProjectsKey = "projects:public";

        public ProjectServiceImpl(
            IProjectRepository repo,
            ICacheService cache,
            NotificationClient notificationClient)
        {
            _repo = repo;
            _cache = cache;
            _notificationClient=notificationClient;
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

        public async Task<List<ProjectResponseDto>> GetMyProjectsAsync(Guid userId)
        {
            // Get owned projects
            var owned = await _repo.FindByOwnerIdAsync(userId);

            // Get projects where user is member
            var memberProjects = await _repo
                .FindByMemberAsync(userId);

            // Combine and remove duplicates
            var allProjects = owned
                .Union(memberProjects,
                    new ProjectComparer())
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            return allProjects.Select(MapToDto).ToList();
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

        public async Task<bool> ToggleStarAsync(
            Guid projectId, Guid userId)
        {
            var project = await _repo.FindByIdAsync(projectId)
                ?? throw new Exception("Project not found");

            var existing = await _repo.FindStarAsync(
                projectId, userId);

            if (existing != null)
            {
                // Unstar
                await _repo.RemoveStarAsync(existing);
                project.StarCount = Math.Max(0, project.StarCount - 1);
                await _repo.UpdateAsync(project);

                await _cache.RemoveAsync($"project:{projectId}");
                await _cache.RemoveAsync(PublicProjectsKey);

                return false; // unstarred
            }
            else
            {
                // Star
                await _repo.AddStarAsync(new StarredProject
                {
                    ProjectId = projectId,
                    UserId = userId
                });
                project.StarCount++;
                await _repo.UpdateAsync(project);

                await _cache.RemoveAsync($"project:{projectId}");
                await _cache.RemoveAsync(PublicProjectsKey);

                return true; // starred
            }
        }

    public async Task<List<Guid>> GetStarredProjectIdsAsync(Guid userId)=> await _repo.GetStarredProjectIdsAsync(userId);
        public async Task AddMemberAsync(Guid projectId, Guid ownerId, Guid newUserId)
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

            await _notificationClient.CreateAsync(new
            {
                RecipientId = newUserId,
                ActorId = ownerId,
                Type = "PROJECT_MEMBER_ADDED",
                Title = "Added to project",
                Message = $"You were added to project '{project.Name}'",
                RelatedId = projectId.ToString(),
                RelatedType = "PROJECT"
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
        public async Task<string> GetUserRoleAsync(Guid projectId, Guid userId)
        {
            var project = await _repo.FindByIdAsync(projectId)
                ?? throw new Exception("Project not found");

            // Owner
            if (project.OwnerId == userId)
                return "OWNER";

            // Check membership
            var member = await _repo.FindMemberAsync(
                projectId, userId);

            if (member != null)
                return member.Role;

            // Public project - read only
            if (project.Visibility == "PUBLIC")
                return "VIEWER";

            return "NONE";
        }
        public async Task AddMemberByUsernameAsync(
            Guid projectId, Guid ownerId, string username)
        {
            var project = await _repo.FindByIdAsync(projectId)
                ?? throw new Exception("Project not found");

            if (project.OwnerId != ownerId)
                throw new Exception("Not authorized");

            // Call AuthService to find user by username
            try
            {
                var response = await _notificationClient
                    ._http.GetAsync(
                        $"http://localhost:5001/api/auth/search?q={username}");

                if (!response.IsSuccessStatusCode)
                    throw new Exception("User not found");

                var json = await response.Content.ReadAsStringAsync();
                var users = System.Text.Json.JsonSerializer
                    .Deserialize<List<UserSearchResult>>(json,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                var user = users?.FirstOrDefault(
                    u => u.Username.ToLower() == username.ToLower())
                    ?? throw new Exception(
                        $"User '{username}' not found");

                var existing = await _repo.FindMemberAsync(
                    projectId, user.UserId);
                if (existing != null)
                    throw new Exception("Already a member");

                await _repo.AddMemberAsync(new ProjectMember
                {
                    ProjectId = projectId,
                    UserId = user.UserId,
                    Role = "EDITOR"
                });

                await _notificationClient.CreateAsync(new
                {
                    RecipientId = user.UserId,
                    ActorId = ownerId,
                    Type = "PROJECT_MEMBER_ADDED",
                    Title = "Added to project",
                    Message = $"You were added to project '{project.Name}'",
                    RelatedId = projectId.ToString(),
                    RelatedType = "PROJECT"
                });
                }
                catch (HttpRequestException)
                {
                    throw new Exception("Could not reach auth service");
                }
            }
        public async Task<List<MemberResponseDto>> GetMembersAsync(Guid projectId)
            {
                var project = await _repo.FindByIdAsync(projectId)
                    ?? throw new Exception("Project not found");

                return project.Members
                    .Select(m => new MemberResponseDto
                    {
                        UserId = m.UserId,
                        Role = m.Role
                    }).ToList();
            }
        public async Task<List<ProjectResponseDto>> GetAllProjectsAsync()
        {
            var projects = await _repo.FindPublicAsync();
            // Get ALL projects not just public
            var allProjects = await _repo.GetAllAsync();
            return allProjects.Select(MapToDto).ToList();
        }

        public async Task AdminDeleteProjectAsync(
            Guid projectId)
        {
            await _repo.DeleteAsync(projectId);
            await _cache.RemoveAsync($"project:{projectId}");
            await _cache.RemoveAsync(PublicProjectsKey);
        }

        public async Task<object> GetStatsAsync()
        {
            var totalProjects = await _repo.CountAllAsync();
            var publicProjects = await _repo.CountPublicAsync();
            var totalFiles = await _repo.CountAllFilesAsync();
            return new
            {
                totalProjects,
                publicProjects,
                privateProjects = totalProjects - publicProjects,
                totalFiles
            };
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
        public class ProjectComparer : IEqualityComparer<Project>
        {
            public bool Equals(Project? x, Project? y)
                => x?.ProjectId == y?.ProjectId;

            public int GetHashCode(Project obj)
                => obj.ProjectId.GetHashCode();
        }
    }
}