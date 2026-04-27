using CodeSync.ProjectService.DTOs;
using CodeSync.ProjectService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CodeSync.ProjectService.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _service;

        public ProjectController(IProjectService service)
        {
            _service = service;
        }

        // POST /api/projects/create
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create(
            [FromBody] CreateProjectDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _service
                    .CreateProjectAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/projects/my
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyProjects()
        {
            try
            {
                var userId = GetUserId();
                var result = await _service
                    .GetMyProjectsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/projects/public
        // No auth - guests can see
        [HttpGet("public")]
        public async Task<IActionResult> GetPublicProjects()
        {
            try
            {
                var result = await _service
                    .GetPublicProjectsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/projects/search
        [HttpPost("search")]
        public async Task<IActionResult> Search(
            [FromBody] SearchDto dto)
        {
            try
            {
                var result = await _service
                    .SearchProjectsAsync(dto.Query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/projects/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _service
                    .GetProjectByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/projects/update
        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> Update(
            [FromBody] UpdateProjectDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _service
                    .UpdateProjectAsync(
                        dto.ProjectId, userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE /api/projects/delete
        [HttpPost("delete")]
        [Authorize]
        public async Task<IActionResult> Delete(
            [FromBody] ProjectIdDto dto)
        {
            try
            {
                var userId = GetUserId();
                await _service.DeleteProjectAsync(
                    dto.ProjectId, userId);
                return Ok(new { message = "Project deleted" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/projects/star
        [HttpPost("star")]
        [Authorize]
        public async Task<IActionResult> ToggleStar(
            [FromBody] ProjectIdDto dto)
        {
            try
            {
                var userId = GetUserId();
                var isStarred = await _service
                    .ToggleStarAsync(dto.ProjectId, userId);
                return Ok(new
                {
                    isStarred,
                    message = isStarred ? "Starred" : "Unstarred"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/projects/starred
        [HttpGet("starred")]
        [Authorize]
        public async Task<IActionResult> GetStarredIds()
        {
            try
            {
                var userId = GetUserId();
                var ids = await _service
                    .GetStarredProjectIdsAsync(userId);
                return Ok(ids);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/projects/members/add
        [HttpPost("members/add")]
        [Authorize]
        public async Task<IActionResult> AddMember(
            [FromBody] MemberDto dto)
        {
            try
            {
                var ownerId = GetUserId();
                await _service.AddMemberAsync(
                    dto.ProjectId, ownerId, dto.UserId);
                return Ok(new { message = "Member added" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/projects/members/remove
        [HttpPost("members/remove")]
        [Authorize]
        public async Task<IActionResult> RemoveMember(
            [FromBody] MemberDto dto)
        {
            try
            {
                var ownerId = GetUserId();
                await _service.RemoveMemberAsync(
                    dto.ProjectId, ownerId, dto.UserId);
                return Ok(new { message = "Member removed" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        // POST /api/projects/check-access
        [HttpPost("check-access")]
        [Authorize]
        public async Task<IActionResult> CheckAccess(
            [FromBody] ProjectIdDto dto)
        {
            try
            {
                var userId = GetUserId();
                var role = await _service
                    .GetUserRoleAsync(dto.ProjectId, userId);
                return Ok(new { role });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private Guid GetUserId() => Guid.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}