using CodeSync.NotificationService.DTOs;
using CodeSync.NotificationService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CodeSync.NotificationService.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(
            INotificationService service)
        {
            _service = service;
        }

        // For cross-service triggers
        [HttpPost("create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateNotificationDto dto)
        {
            try
            {
                var result = await _service.CreateAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = ex.Message });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMine()
        {
            try
            {
                var userId = GetUserId();
                var result = await _service
                    .GetMyNotificationsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = ex.Message });
            }
        }

        [HttpGet("unread-count")]
        [Authorize]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetUserId();
                var count = await _service
                    .GetUnreadCountAsync(userId);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = ex.Message });
            }
        }

        [HttpPut("read")]
        [Authorize]
        public async Task<IActionResult> MarkRead(
            [FromBody] NotificationIdDto dto)
        {
            try
            {
                var userId = GetUserId();
                await _service.MarkReadAsync(
                    userId, dto.NotificationId);
                return Ok(new { message = "Marked read" });
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { message = ex.Message });
            }
        }

        [HttpPut("read-all")]
        [Authorize]
        public async Task<IActionResult> MarkAllRead()
        {
            try
            {
                var userId = GetUserId();
                await _service.MarkAllReadAsync(userId);
                return Ok(new { message = "All read" });
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