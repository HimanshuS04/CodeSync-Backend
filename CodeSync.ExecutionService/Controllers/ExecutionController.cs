using CodeSync.ExecutionService.DTOs;
using CodeSync.ExecutionService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CodeSync.ExecutionService.Controllers
{
    [ApiController]
    [Route("api/executions")]
    public class ExecutionController : ControllerBase
    {
        private readonly IExecutionService _service;

        public ExecutionController(IExecutionService service)
        {
            _service = service;
        }

        [HttpPost("run")]
        [Authorize]
        public async Task<IActionResult> Run(
            [FromBody] RunCodeDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _service
                    .RunCodeAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetResult(int id)
        {
            try
            {
                var result = await _service.GetResultAsync(id);
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
                    .GetByProjectAsync(projectId);
                return Ok(result);
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