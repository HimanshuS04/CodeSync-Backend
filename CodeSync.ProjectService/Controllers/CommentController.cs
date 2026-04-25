using CodeSync.ProjectService.DTOs;
using CodeSync.ProjectService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CodeSync.ProjectService.Controllers
{
    [ApiController]
    [Route("api/comments")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _service;

        public CommentController(ICommentService service)
        {
            _service = service;
        }

        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> Add(
            [FromBody] AddCommentDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _service
                    .AddCommentAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = ex.Message });
            }
        }

        [HttpPost("reply")]
        [Authorize]
        public async Task<IActionResult> Reply(
            [FromBody] ReplyCommentDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _service
                    .ReplyAsync(userId, dto);
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
        public async Task<IActionResult> GetByFile(
            Guid fileId)
        {
            try
            {
                var result = await _service
                    .GetByFileAsync(fileId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = ex.Message });
            }
        }

        [HttpPut("resolve")]
        [Authorize]
        public async Task<IActionResult> Resolve(
            [FromBody] CommentIdDto dto)
        {
            try
            {
                var userId = GetUserId();
                await _service.ResolveAsync(
                    userId, dto.CommentId);
                return Ok(new { message = "Resolved" });
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = ex.Message });
            }
        }

        [HttpPut("unresolve")]
        [Authorize]
        public async Task<IActionResult> Unresolve(
            [FromBody] CommentIdDto dto)
        {
            try
            {
                var userId = GetUserId();
                await _service.UnresolveAsync(
                    userId, dto.CommentId);
                return Ok(new { message = "Unresolved" });
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = ex.Message });
            }
        }

        [HttpPost("delete")]
        [Authorize]
        public async Task<IActionResult> Delete(
            [FromBody] CommentIdDto dto)
        {
            try
            {
                var userId = GetUserId();
                await _service.DeleteAsync(
                    userId, dto.CommentId);
                return Ok(new { message = "Deleted" });
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = ex.Message });
            }
        }

        [HttpGet("count/{fileId}")]
        [Authorize]
        public async Task<IActionResult> GetCount(
            Guid fileId)
        {
            try
            {
                var count = await _service
                    .GetCountAsync(fileId);
                return Ok(new { count });
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