using CodeSync.ProjectService.DTOs;
using CodeSync.ProjectService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CodeSync.ProjectService.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FileController : ControllerBase
    {
        private readonly IFileService _service;

        public FileController(IFileService service)
        {
            _service = service;
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create(
            [FromBody] CreateFileDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _service
                    .CreateFileAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _service
                    .GetFileByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("project/{projectId}")]
        [Authorize]
        public async Task<IActionResult> GetByProject(
            Guid projectId)
        {
            try
            {
                var result = await _service
                    .GetFilesByProjectAsync(projectId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("tree/{projectId}")]
        [Authorize]
        public async Task<IActionResult> GetTree(
            Guid projectId)
        {
            try
            {
                var result = await _service
                    .GetFileTreeAsync(projectId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("content")]
        [Authorize]
        public async Task<IActionResult> UpdateContent(
            [FromBody] UpdateFileContentDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _service
                    .UpdateContentAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("rename")]
        [Authorize]
        public async Task<IActionResult> Rename(
            [FromBody] RenameFileDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _service
                    .RenameFileAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("delete")]
        [Authorize]
        public async Task<IActionResult> Delete(
            [FromBody] FileIdDto dto)
        {
            try
            {
                var userId = GetUserId();
                await _service.DeleteFileAsync(
                    userId, dto.FileId);
                return Ok(new { message = "File deleted" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("restore")]
        [Authorize]
        public async Task<IActionResult> Restore(
            [FromBody] FileIdDto dto)
        {
            try
            {
                var userId = GetUserId();
                await _service.RestoreFileAsync(
                    userId, dto.FileId);
                return Ok(new { message = "File restored" });
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