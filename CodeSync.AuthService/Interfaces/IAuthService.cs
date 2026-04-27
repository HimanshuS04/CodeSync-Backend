using CodeSync.AuthService.DTOs;
using CodeSync.AuthService.Models;

namespace CodeSync.AuthService.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> GoogleLoginAsync(string idToken);
        Task<User> GetProfileAsync(Guid userId);
        Task<User> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
        Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
        Task<List<User>> SearchUsersAsync(string query);
        Task SuspendUserAsync(Guid userId);
        Task ReactivateUserAsync(Guid userId);
        Task<List<User>> GetAllUsersAsync();
    }
}