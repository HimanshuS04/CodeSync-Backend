using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeSync.AuthService.DTOs;
using CodeSync.AuthService.Helpers;
using CodeSync.AuthService.Interfaces;
using CodeSync.AuthService.Models;
using CodeSync.AuthService.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CodeSync.Tests.AuthService
{
    [TestClass]
    public class AuthServiceTests
    {
        private Mock<IUserRepository> _userRepoMock = null!;
        private Mock<IConfiguration> _configMock = null!;
        private JwtHelper _jwtHelper = null!;
        private AuthServiceImpl _authService = null!;

        [TestInitialize]
        public void Setup()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _configMock = new Mock<IConfiguration>();

            // Setup JWT config
            var jwtSection = new Mock<IConfigurationSection>();
            _configMock.Setup(c => c["JwtSettings:SecretKey"])
                .Returns("test_secret_key_minimum_32_characters_long");
            _configMock.Setup(c => c["JwtSettings:Issuer"])
                .Returns("CodeSync");
            _configMock.Setup(c => c["JwtSettings:Audience"])
                .Returns("CodeSyncUsers");
            _configMock.Setup(c => c["JwtSettings:ExpiryHours"])
                .Returns("24");

            _jwtHelper = new JwtHelper(_configMock.Object);

            _authService = new AuthServiceImpl(
                _userRepoMock.Object,
                _jwtHelper,
                _configMock.Object);
        }

        // ===== REGISTER TESTS =====

        [TestMethod]
        public async Task Register_WithValidData_ReturnsToken()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "password123"
            };

            _userRepoMock.Setup(r =>
                r.ExistsByEmailAsync(dto.Email))
                .ReturnsAsync(false);

            _userRepoMock.Setup(r =>
                r.ExistsByUsernameAsync(dto.Username))
                .ReturnsAsync(false);

            _userRepoMock.Setup(r =>
                r.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            // Act
            var result = await _authService
                .RegisterAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeEmpty();
            result.Username.Should().Be("testuser");
            result.Email.Should().Be("test@example.com");
            result.Role.Should().Be("DEVELOPER");
        }

        [TestMethod]
        public async Task Register_WithExistingEmail_ThrowsException()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Username = "testuser",
                Email = "existing@example.com",
                Password = "password123"
            };

            _userRepoMock.Setup(r =>
                r.ExistsByEmailAsync(dto.Email))
                .ReturnsAsync(true);

            // Act
            var act = async () => await _authService
                .RegisterAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Email already registered");
        }

        [TestMethod]
        public async Task Register_WithExistingUsername_ThrowsException()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Username = "existinguser",
                Email = "new@example.com",
                Password = "password123"
            };

            _userRepoMock.Setup(r =>
                r.ExistsByEmailAsync(dto.Email))
                .ReturnsAsync(false);

            _userRepoMock.Setup(r =>
                r.ExistsByUsernameAsync(dto.Username))
                .ReturnsAsync(true);

            // Act
            var act = async () => await _authService
                .RegisterAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Username already taken");
        }

        // ===== LOGIN TESTS =====

        [TestMethod]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            var password = "password123";
            var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
            var hash = BCrypt.Net.BCrypt
                .HashPassword(password, salt);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = hash,
                Role = "DEVELOPER",
                IsActive = true
            };

            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = password
            };

            _userRepoMock.Setup(r =>
                r.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeEmpty();
            result.Username.Should().Be("testuser");
        }

        [TestMethod]
        public async Task Login_WithWrongPassword_ThrowsException()
        {
            // Arrange
            var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
            var hash = BCrypt.Net.BCrypt
                .HashPassword("correctpassword", salt);

            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = hash,
                IsActive = true
            };

            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            };

            _userRepoMock.Setup(r =>
                r.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            // Act
            var act = async () => await _authService
                .LoginAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Invalid email or password");
        }

        [TestMethod]
        public async Task Login_WithNonExistentEmail_ThrowsException()
        {
            // Arrange
            var dto = new LoginDto
            {
                Email = "notfound@example.com",
                Password = "password123"
            };

            _userRepoMock.Setup(r =>
                r.FindByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);

            // Act
            var act = async () => await _authService
                .LoginAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Invalid email or password");
        }

        [TestMethod]
        public async Task Login_WithSuspendedAccount_ThrowsException()
        {
            // Arrange
            var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
            var hash = BCrypt.Net.BCrypt
                .HashPassword("password123", salt);

            var user = new User
            {
                Email = "suspended@example.com",
                PasswordHash = hash,
                IsActive = false
            };

            var dto = new LoginDto
            {
                Email = "suspended@example.com",
                Password = "password123"
            };

            _userRepoMock.Setup(r =>
                r.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            // Act
            var act = async () => await _authService
                .LoginAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Account is suspended");
        }

        // ===== PROFILE TESTS =====

        [TestMethod]
        public async Task GetProfile_WithValidId_ReturnsUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                UserId = userId,
                Username = "testuser",
                Email = "test@example.com",
                Role = "DEVELOPER"
            };

            _userRepoMock.Setup(r =>
                r.FindByUserIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _authService
                .GetProfileAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.Username.Should().Be("testuser");
        }

        [TestMethod]
        public async Task GetProfile_WithInvalidId_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _userRepoMock.Setup(r =>
                r.FindByUserIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var act = async () => await _authService
                .GetProfileAsync(userId);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("User not found");
        }

        // ===== UPDATE PROFILE TESTS =====

        [TestMethod]
        public async Task UpdateProfile_WithValidData_UpdatesUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                UserId = userId,
                Username = "oldname",
                Email = "old@example.com"
            };

            var dto = new UpdateProfileDto
            {
                Username = "newname"
            };

            _userRepoMock.Setup(r =>
                r.FindByUserIdAsync(userId))
                .ReturnsAsync(user);

            _userRepoMock.Setup(r =>
                r.ExistsByUsernameAsync("newname"))
                .ReturnsAsync(false);

            _userRepoMock.Setup(r =>
                r.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            // Act
            var result = await _authService
                .UpdateProfileAsync(userId, dto);

            // Assert
            result.Username.Should().Be("newname");
        }

        [TestMethod]
        public async Task UpdateProfile_WithTakenUsername_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                UserId = userId,
                Username = "currentname"
            };

            var dto = new UpdateProfileDto
            {
                Username = "takenname"
            };

            _userRepoMock.Setup(r =>
                r.FindByUserIdAsync(userId))
                .ReturnsAsync(user);

            _userRepoMock.Setup(r =>
                r.ExistsByUsernameAsync("takenname"))
                .ReturnsAsync(true);

            // Act
            var act = async () => await _authService
                .UpdateProfileAsync(userId, dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Username already taken");
        }

        // ===== CHANGE PASSWORD TESTS =====

        [TestMethod]
        public async Task ChangePassword_WithCorrectOldPassword_Succeeds()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
            var hash = BCrypt.Net.BCrypt
                .HashPassword("oldpass", salt);

            var user = new User
            {
                UserId = userId,
                PasswordHash = hash
            };

            var dto = new ChangePasswordDto
            {
                OldPassword = "oldpass",
                NewPassword = "newpass123"
            };

            _userRepoMock.Setup(r =>
                r.FindByUserIdAsync(userId))
                .ReturnsAsync(user);

            _userRepoMock.Setup(r =>
                r.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            // Act
            var act = async () => await _authService
                .ChangePasswordAsync(userId, dto);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task ChangePassword_WithWrongOldPassword_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
            var hash = BCrypt.Net.BCrypt
                .HashPassword("correctpass", salt);

            var user = new User
            {
                UserId = userId,
                PasswordHash = hash
            };

            var dto = new ChangePasswordDto
            {
                OldPassword = "wrongpass",
                NewPassword = "newpass123"
            };

            _userRepoMock.Setup(r =>
                r.FindByUserIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var act = async () => await _authService
                .ChangePasswordAsync(userId, dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Old password is incorrect");
        }

        // ===== SUSPEND/REACTIVATE TESTS =====

        [TestMethod]
        public async Task SuspendUser_WithValidId_SetsIsActiveFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                UserId = userId,
                IsActive = true
            };

            _userRepoMock.Setup(r =>
                r.FindByUserIdAsync(userId))
                .ReturnsAsync(user);

            _userRepoMock.Setup(r =>
                r.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            // Act
            await _authService.SuspendUserAsync(userId);

            // Assert
            _userRepoMock.Verify(r =>
                r.UpdateAsync(It.Is<User>(
                    u => u.IsActive == false)), Times.Once);
        }

        [TestMethod]
        public async Task ReactivateUser_WithValidId_SetsIsActiveTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                UserId = userId,
                IsActive = false
            };

            _userRepoMock.Setup(r =>
                r.FindByUserIdAsync(userId))
                .ReturnsAsync(user);

            _userRepoMock.Setup(r =>
                r.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            // Act
            await _authService.ReactivateUserAsync(userId);

            // Assert
            _userRepoMock.Verify(r =>
                r.UpdateAsync(It.Is<User>(
                    u => u.IsActive == true)), Times.Once);
        }

        [TestMethod]
        public async Task SuspendUser_WithInvalidId_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _userRepoMock.Setup(r =>
                r.FindByUserIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var act = async () => await _authService
                .SuspendUserAsync(userId);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("User not found");
        }

        // ===== SEARCH TESTS =====

        [TestMethod]
        public async Task SearchUsers_WithQuery_ReturnsMatchingUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new() { Username = "himanshu01" },
                new() { Username = "himanshu02" }
            };

            _userRepoMock.Setup(r =>
                r.SearchByUsernameAsync("himanshu"))
                .ReturnsAsync(users);

            // Act
            var result = await _authService
                .SearchUsersAsync("himanshu");

            // Assert
            result.Should().HaveCount(2);
        }
    }
}