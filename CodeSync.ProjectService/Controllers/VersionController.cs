using CodeSync.ProjectService.DTOs;
using CodeSync.ProjectService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CodeSync.ProjectService.Controllers
{
    [ApiController]
    [Route("api/versions")]
    public class VersionController : ControllerBase
    {
        private readonly IVersionService _service;

        public VersionController(IVersionService service)
        {
            _service = service;
        }

        [HttpPost("snapshot")]
        [Authorize]
        public async Task<IActionResult> CreateSnapshot(
            [FromBody] CreateSnapshotDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _service
                    .CreateSnapshotAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = ex.Message });
            }
        }

        [HttpGet("file/{fileId}")]
        [Authorize]
        public async Task<IActionResult> GetFileHistory(
            Guid fileId)
        {
            try
            {
                var result = await _service
                    .GetFileHistoryAsync(fileId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service
                    .GetSnapshotByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = ex.Message });
            }
        }

        [HttpPost("restore")]
        [Authorize]
        public async Task<IActionResult> Restore(
            [FromBody] SnapshotIdDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _service
                    .RestoreSnapshotAsync(
                        userId, dto.SnapshotId);
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