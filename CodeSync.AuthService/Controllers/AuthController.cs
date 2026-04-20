using CodeSync.AuthService.DTOs;
using CodeSync.AuthService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CodeSync.AuthService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // Public endpoints
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterDto dto)
        {
            try
            {
                var result = await _authService
                    .RegisterAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginDto dto)
        {
            try
            {
                var result = await _authService
                    .LoginAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin(
            [FromBody] GoogleAuthDto dto)
        {
            try
            {
                var result = await _authService
                    .GoogleLoginAsync(dto.IdToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Protected endpoints
        // UserId extracted from JWT claims
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetUserId();
                var user = await _authService
                    .GetProfileAsync(userId);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateProfileDto dto)
        {
            try
            {
                var userId = GetUserId();
                var user = await _authService
                    .UpdateProfileAsync(userId, dto);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordDto dto)
        {
            try
            {
                var userId = GetUserId();
                await _authService
                    .ChangePasswordAsync(userId, dto);
                return Ok(new { message = "Password changed" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("search")]
        [Authorize]
        public async Task<IActionResult> SearchUsers(
            [FromBody] SearchDto dto)
        {
            try
            {
                var users = await _authService
                    .SearchUsersAsync(dto.Query);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Admin endpoints
        [HttpGet("admin/users")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _authService
                    .GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("admin/users/suspend")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> SuspendUser(
            [FromBody] UserIdDto dto)
        {
            try
            {
                await _authService
                    .SuspendUserAsync(dto.UserId);
                return Ok(new { message = "User suspended" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("admin/users/reactivate")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ReactivateUser(
            [FromBody] UserIdDto dto)
        {
            try
            {
                await _authService
                    .ReactivateUserAsync(dto.UserId);
                return Ok(new { message = "User reactivated" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Helper - get UserId from JWT claims
        private Guid GetUserId() => Guid.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}