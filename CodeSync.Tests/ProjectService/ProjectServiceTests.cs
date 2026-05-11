using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeSync.ProjectService.DTOs;
using CodeSync.ProjectService.Interfaces;
using CodeSync.ProjectService.Models;
using CodeSync.ProjectService.Services;
using FluentAssertions;
using Moq;

namespace CodeSync.Tests.ProjectService
{
    [TestClass]
    public class ProjectServiceTests
    {
        private Mock<IProjectRepository> _repoMock = null!;
        private Mock<ICacheService> _cacheMock = null!;
        private Mock<INotificationClient> _notifMock = null!;
        private ProjectServiceImpl _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _repoMock = new Mock<IProjectRepository>();
            _cacheMock = new Mock<ICacheService>();
            _notifMock = new Mock<INotificationClient>();

            _service = new ProjectServiceImpl(
                _repoMock.Object,
                _cacheMock.Object,
                _notifMock.Object);
        }

        // ===== CREATE PROJECT TESTS =====

        [TestMethod]
        public async Task CreateProject_WithValidData_ReturnsProject()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var dto = new CreateProjectDto
            {
                Name = "Test Project",
                Language = "Python",
                Visibility = "PUBLIC"
            };

            _repoMock.Setup(r =>
                r.CreateAsync(It.IsAny<Project>()))
                .ReturnsAsync((Project p) => p);

            _cacheMock.Setup(c =>
                c.RemoveAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service
                .CreateProjectAsync(ownerId, dto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Test Project");
            result.OwnerId.Should().Be(ownerId);
            result.Language.Should().Be("Python");
            result.Visibility.Should().Be("PUBLIC");
        }

        // ===== GET PROJECT TESTS =====

        [TestMethod]
        public async Task GetProjectById_WithValidId_ReturnsProject()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new Project
            {
                ProjectId = projectId,
                Name = "My Project",
                Language = "Python",
                Visibility = "PUBLIC"
            };

            _cacheMock.Setup(c =>
                c.GetAsync<ProjectResponseDto>(
                    It.IsAny<string>()))
                .ReturnsAsync((ProjectResponseDto?)null);

            _repoMock.Setup(r =>
                r.FindByIdAsync(projectId))
                .ReturnsAsync(project);

            _cacheMock.Setup(c =>
                c.SetAsync(It.IsAny<string>(),
                    It.IsAny<ProjectResponseDto>(),
                    It.IsAny<TimeSpan?>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service
                .GetProjectByIdAsync(projectId);

            // Assert
            result.Should().NotBeNull();
            result.ProjectId.Should().Be(projectId);
            result.Name.Should().Be("My Project");
        }

        [TestMethod]
        public async Task GetProjectById_WithInvalidId_ThrowsException()
        {
            // Arrange
            var projectId = Guid.NewGuid();

            _cacheMock.Setup(c =>
                c.GetAsync<ProjectResponseDto>(
                    It.IsAny<string>()))
                .ReturnsAsync((ProjectResponseDto?)null);

            _repoMock.Setup(r =>
                r.FindByIdAsync(projectId))
                .ReturnsAsync((Project?)null);

            // Act
            var act = async () => await _service
                .GetProjectByIdAsync(projectId);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Project not found");
        }

        // ===== UPDATE PROJECT TESTS =====

        [TestMethod]
        public async Task UpdateProject_ByOwner_UpdatesSuccessfully()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var project = new Project
            {
                ProjectId = projectId,
                OwnerId = ownerId,
                Name = "Old Name"
            };

            var dto = new UpdateProjectDto
            {
                ProjectId = projectId,
                Name = "New Name"
            };

            _repoMock.Setup(r =>
                r.FindByIdAsync(projectId))
                .ReturnsAsync(project);

            _repoMock.Setup(r =>
                r.UpdateAsync(It.IsAny<Project>()))
                .ReturnsAsync((Project p) => p);

            _cacheMock.Setup(c =>
                c.RemoveAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service
                .UpdateProjectAsync(
                    projectId, ownerId, dto);

            // Assert
            result.Name.Should().Be("New Name");
        }

        [TestMethod]
        public async Task UpdateProject_ByNonOwner_ThrowsException()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var project = new Project
            {
                ProjectId = projectId,
                OwnerId = ownerId,
                Name = "My Project"
            };

            var dto = new UpdateProjectDto
            {
                ProjectId = projectId,
                Name = "Hacked Name"
            };

            _repoMock.Setup(r =>
                r.FindByIdAsync(projectId))
                .ReturnsAsync(project);

            // Act
            var act = async () => await _service
                .UpdateProjectAsync(
                    projectId, otherUserId, dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Not authorized");
        }

        // ===== DELETE PROJECT TESTS =====

        [TestMethod]
        public async Task DeleteProject_ByOwner_DeletesSuccessfully()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var project = new Project
            {
                ProjectId = projectId,
                OwnerId = ownerId
            };

            _repoMock.Setup(r =>
                r.FindByIdAsync(projectId))
                .ReturnsAsync(project);

            _repoMock.Setup(r =>
                r.DeleteAsync(projectId))
                .Returns(Task.CompletedTask);

            _cacheMock.Setup(c =>
                c.RemoveAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var act = async () => await _service
                .DeleteProjectAsync(projectId, ownerId);

            // Assert
            await act.Should().NotThrowAsync();
            _repoMock.Verify(r =>
                r.DeleteAsync(projectId), Times.Once);
        }

        [TestMethod]
        public async Task DeleteProject_ByNonOwner_ThrowsException()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var otherId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var project = new Project
            {
                ProjectId = projectId,
                OwnerId = ownerId
            };

            _repoMock.Setup(r =>
                r.FindByIdAsync(projectId))
                .ReturnsAsync(project);

            // Act
            var act = async () => await _service
                .DeleteProjectAsync(projectId, otherId);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Not authorized");
        }

        // ===== STAR PROJECT TESTS =====

        [TestMethod]
        public async Task ToggleStar_WhenNotStarred_IncrementsCount()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var project = new Project
            {
                ProjectId = projectId,
                StarCount = 0
            };

            _repoMock.Setup(r =>
                r.FindByIdAsync(projectId))
                .ReturnsAsync(project);

            _repoMock.Setup(r =>
                r.FindStarAsync(projectId, userId))
                .ReturnsAsync((StarredProject?)null);

            _repoMock.Setup(r =>
                r.AddStarAsync(It.IsAny<StarredProject>()))
                .Returns(Task.CompletedTask);

            _repoMock.Setup(r =>
                r.UpdateAsync(It.IsAny<Project>()))
                .ReturnsAsync((Project p) => p);

            _cacheMock.Setup(c =>
                c.RemoveAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service
                .ToggleStarAsync(projectId, userId);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public async Task ToggleStar_WhenAlreadyStarred_DecrementsCount()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var project = new Project
            {
                ProjectId = projectId,
                StarCount = 5
            };

            var existingStar = new StarredProject
            {
                ProjectId = projectId,
                UserId = userId
            };

            _repoMock.Setup(r =>
                r.FindByIdAsync(projectId))
                .ReturnsAsync(project);

            _repoMock.Setup(r =>
                r.FindStarAsync(projectId, userId))
                .ReturnsAsync(existingStar);

            _repoMock.Setup(r =>
                r.RemoveStarAsync(existingStar))
                .Returns(Task.CompletedTask);

            _repoMock.Setup(r =>
                r.UpdateAsync(It.IsAny<Project>()))
                .ReturnsAsync((Project p) => p);

            _cacheMock.Setup(c =>
                c.RemoveAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service
                .ToggleStarAsync(projectId, userId);

            // Assert
            result.Should().BeFalse();
        }

        // ===== MEMBER TESTS =====

        [TestMethod]
        public async Task AddMember_WithValidData_AddsMember()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var newUserId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var project = new Project
            {
                ProjectId = projectId,
                OwnerId = ownerId,
                Name = "Test Project"
            };

            _repoMock.Setup(r =>
                r.FindByIdAsync(projectId))
                .ReturnsAsync(project);

            _repoMock.Setup(r =>
                r.FindMemberAsync(projectId, newUserId))
                .ReturnsAsync((ProjectMember?)null);

            _repoMock.Setup(r =>
                r.AddMemberAsync(
                    It.IsAny<ProjectMember>()))
                .Returns(Task.CompletedTask);

            // Act
            var act = async () => await _service
                .AddMemberAsync(
                    projectId, ownerId, newUserId);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task AddMember_WhenAlreadyMember_ThrowsException()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var existingUserId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var project = new Project
            {
                ProjectId = projectId,
                OwnerId = ownerId
            };

            var existingMember = new ProjectMember
            {
                ProjectId = projectId,
                UserId = existingUserId
            };

            _repoMock.Setup(r =>
                r.FindByIdAsync(projectId))
                .ReturnsAsync(project);

            _repoMock.Setup(r =>
                r.FindMemberAsync(
                    projectId, existingUserId))
                .ReturnsAsync(existingMember);

            // Act
            var act = async () => await _service
                .AddMemberAsync(
                    projectId, ownerId, existingUserId);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Already a member");
        }

        [TestMethod]
        public async Task AddMember_ByNonOwner_ThrowsException()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var nonOwnerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var project = new Project
            {
                ProjectId = projectId,
                OwnerId = ownerId
            };

            _repoMock.Setup(r =>
                r.FindByIdAsync(projectId))
                .ReturnsAsync(project);

            // Act
            var act = async () => await _service
                .AddMemberAsync(
                    projectId, nonOwnerId, Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Not authorized");
        }
    }
}