using CodeSync.CollabService.DTOs;
using CodeSync.CollabService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CodeSync.CollabService.Controllers
{
    [ApiController]
    [Route("api/sessions")]
    public class CollabController : ControllerBase
    {
        private readonly ICollabService _service;

        public CollabController(ICollabService service)
        {
            _service = service;
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create(
            [FromBody] CreateSessionDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _service
                    .CreateSessionAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = ex.Message });
            }
        }

        [HttpPost("join")]
        [Authorize]
        public async Task<IActionResult> Join(
            [FromBody] JoinSessionDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _service
                    .JoinSessionAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = ex.Message });
            }
        }

        [HttpPost("leave")]
        [Authorize]
        public async Task<IActionResult> Leave(
            [FromBody] SessionIdDto dto)
        {
            try
            {
                var userId = GetUserId();
                await _service.LeaveSessionAsync(
                    userId, dto.SessionId);
                return Ok(new { message = "Left session" });
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = ex.Message });
            }
        }

        [HttpPost("end")]
        [Authorize]
        public async Task<IActionResult> End(
            [FromBody] SessionIdDto dto)
        {
            try
            {
                var userId = GetUserId();
                await _service.EndSessionAsync(
                    userId, dto.SessionId);
                return Ok(new { message = "Session ended" });
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = ex.Message });
            }
        }

        [HttpGet("{sessionId}")]
        [Authorize]
        public async Task<IActionResult> GetSession(
            Guid sessionId)
        {
            try
            {
                var result = await _service
                    .GetSessionAsync(sessionId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = ex.Message });
            }
        }

        [HttpGet("project/{projectId}")]
        [Authorize]
        public async Task<IActionResult>
            GetActiveByProject(Guid projectId)
        {
            try
            {
                var result = await _service
                    .GetActiveSessionsByProjectAsync(
                        projectId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = ex.Message });
            }
        }
        [HttpGet("admin/active")]
        [Authorize]
        public async Task<IActionResult> AdminActiveSessions()
        {
            try
            {
                var role = User.FindFirst(
                    System.Security.Claims.ClaimTypes.Role)?.Value;
                if (role != "ADMIN")
                    return StatusCode(403,
                        new { message = "Forbidden" });

                var result = await _service
                    .GetAllActiveSessionsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = ex.Message });
            }
        }

        private Guid GetUserId() => Guid.Parse(
            User.FindFirst(
                ClaimTypes.NameIdentifier)!.Value);
    }
}