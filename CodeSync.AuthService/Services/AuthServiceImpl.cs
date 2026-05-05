using CodeSync.AuthService.DTOs;
using CodeSync.AuthService.Helpers;
using CodeSync.AuthService.Interfaces;
using CodeSync.AuthService.Models;
using Google.Apis.Auth;

namespace CodeSync.AuthService.Services
{
    public class AuthServiceImpl : IAuthService
    {
        private readonly IUserRepository _repo;
        private readonly JwtHelper _jwt;
        private readonly IConfiguration _config;

        public AuthServiceImpl(
            IUserRepository repo,
            JwtHelper jwt,
            IConfiguration config)
        {
            _repo = repo;
            _jwt = jwt;
            _config = config;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _repo.ExistsByEmailAsync(dto.Email))
                throw new Exception("Email already registered");

            if (await _repo.ExistsByUsernameAsync(dto.Username))
                throw new Exception("Username already taken");

            // Explicit salt + BCrypt hash
            var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password, salt);

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = hash,
                Role = "DEVELOPER"
            };

            await _repo.CreateAsync(user);

            return BuildResponse(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _repo.FindByEmailAsync(dto.Email)
                ?? throw new Exception("Invalid email or password");

            if (!user.IsActive)
                throw new Exception("Account is suspended");

            if (user.PasswordHash == null ||
                !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new Exception("Invalid email or password");

            return BuildResponse(user);
        }

        public async Task<AuthResponseDto> GoogleLoginAsync(string idToken)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _config["GoogleAuth:ClientId"] }
                });

            var user = await _repo.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                user = new User
                {
                    Username = payload.Email.Split('@')[0],
                    Email = payload.Email,
                    Role = "DEVELOPER"
                };
                await _repo.CreateAsync(user);
            }

            if (!user.IsActive)
                throw new Exception("Account is suspended");

            return BuildResponse(user);
        }

        public async Task<User> GetProfileAsync(Guid userId)
            => await _repo.FindByUserIdAsync(userId)
                ?? throw new Exception("User not found");

        public async Task<User> UpdateProfileAsync(
            Guid userId, UpdateProfileDto dto)
        {
            var user = await _repo.FindByUserIdAsync(userId)
                ?? throw new Exception("User not found");

            if (dto.Username != null && dto.Username != user.Username)
            {
                if (await _repo.ExistsByUsernameAsync(dto.Username))
                    throw new Exception("Username already taken");
                user.Username = dto.Username;
            }

            if (dto.Email != null && dto.Email != user.Email)
            {
                if (await _repo.ExistsByEmailAsync(dto.Email))
                    throw new Exception("Email already taken");
                user.Email = dto.Email;
            }

            return await _repo.UpdateAsync(user);
        }

        public async Task ChangePasswordAsync(
            Guid userId, ChangePasswordDto dto)
        {
            var user = await _repo.FindByUserIdAsync(userId)
                ?? throw new Exception("User not found");

            if (user.PasswordHash == null ||
                !BCrypt.Net.BCrypt.Verify(
                    dto.OldPassword, user.PasswordHash))
                throw new Exception("Old password is incorrect");

            var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
            user.PasswordHash = BCrypt.Net.BCrypt
                .HashPassword(dto.NewPassword, salt);

            await _repo.UpdateAsync(user);
        }

        public async Task<List<User>> SearchUsersAsync(string query)
            => await _repo.SearchByUsernameAsync(query);

        public async Task SuspendUserAsync(Guid userId)
        {
            var user = await _repo.FindByUserIdAsync(userId)
                ?? throw new Exception("User not found");
            user.IsActive = false;
            await _repo.UpdateAsync(user);
        }

        public async Task ReactivateUserAsync(Guid userId)
        {
            var user = await _repo.FindByUserIdAsync(userId)
                ?? throw new Exception("User not found");
            user.IsActive = true;
            await _repo.UpdateAsync(user);
        }

        public async Task<List<User>> GetAllUsersAsync()
            => await _repo.GetAllUsersAsync();

        // Helper to build response
        private AuthResponseDto BuildResponse(User user) => new()
        {
            Token = _jwt.GenerateToken(user),
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        };
    }
}