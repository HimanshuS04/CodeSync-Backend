using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeSync.CollabService.DTOs;
using CodeSync.CollabService.Interfaces;
using CodeSync.CollabService.Models;
using CodeSync.CollabService.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CodeSync.Tests.CollabService
{
    [TestClass]
    public class CollabServiceTests
    {
        private Mock<ICollabRepository> _repoMock = null!;
        private Mock<IRedisService> _redisMock = null!;
        private Mock<INotificationClient> _notifMock = null!;
        private Mock<IHttpContextAccessor> _httpMock = null!;
        private CollabServiceImpl _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _repoMock = new Mock<ICollabRepository>();
            _redisMock = new Mock<IRedisService>();
            _notifMock = new Mock<INotificationClient>();
            _httpMock = new Mock<IHttpContextAccessor>();

            _service = new CollabServiceImpl(
                _repoMock.Object,
                _redisMock.Object,
                _notifMock.Object,
                _httpMock.Object);
        }

        // ===== CREATE SESSION TESTS =====

        [TestMethod]
        public async Task CreateSession_ByOwner_CreatesSuccessfully()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var dto = new CreateSessionDto
            {
                ProjectId = Guid.NewGuid(),
                FileId = Guid.NewGuid(),
                OwnerId = ownerId,
                InitialContent = "print('hello')"
            };

            _redisMock.Setup(r =>
                r.SetDocumentAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _redisMock.Setup(r =>
                r.AddParticipantAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _notifMock.Setup(n =>
                n.CreateAsync(It.IsAny<object>()))
                .Returns(Task.CompletedTask);

            _notifMock.Setup(n =>
                n.GetProjectMembersAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new List<ProjectMemberInfo>());

            // Act
            var result = await _service
                .CreateSessionAsync(ownerId, dto);

            // Assert
            result.Should().NotBeNull();
            result.OwnerId.Should().Be(ownerId);
            result.Status.Should().Be("ACTIVE");
        }

        [TestMethod]
        public async Task CreateSession_ByNonOwner_ThrowsException()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var nonOwnerId = Guid.NewGuid();

            var dto = new CreateSessionDto
            {
                ProjectId = Guid.NewGuid(),
                FileId = Guid.NewGuid(),
                OwnerId = ownerId,
                InitialContent = ""
            };

            // Act
            var act = async () => await _service
                .CreateSessionAsync(nonOwnerId, dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage(
                    "Only project owner can start session");
        }

        // ===== JOIN SESSION TESTS =====

        [TestMethod]
        public async Task JoinSession_WithActiveSession_JoinsSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            var session = new CollabSession
            {
                SessionId = sessionId,
                Status = "ACTIVE",
                MaxParticipants = 5,
                Participants = new List<Participant>()
            };

            var dto = new JoinSessionDto
            {
                SessionId = sessionId,
                Username = "testuser"
            };

            _repoMock.Setup(r =>
                r.FindByIdAsync(sessionId))
                .ReturnsAsync(session);

            _repoMock.Setup(r =>
                r.FindParticipantAsync(sessionId, userId))
                .ReturnsAsync((Participant?)null);

            _repoMock.Setup(r =>
                r.AddParticipantAsync(
                    It.IsAny<Participant>()))
                .Returns(Task.CompletedTask);

            _redisMock.Setup(r =>
                r.AddParticipantAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _notifMock.Setup(n =>
                n.CreateAsync(It.IsAny<object>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service
                .JoinSessionAsync(userId, dto);

            // Assert
            result.Should().NotBeNull();
        }

        [TestMethod]
        public async Task JoinSession_WithEndedSession_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            var session = new CollabSession
            {
                SessionId = sessionId,
                Status = "ENDED",
                MaxParticipants = 5,
                Participants = new List<Participant>()
            };

            var dto = new JoinSessionDto
            {
                SessionId = sessionId,
                Username = "testuser"
            };

            _repoMock.Setup(r =>
                r.FindByIdAsync(sessionId))
                .ReturnsAsync(session);

            // Act
            var act = async () => await _service
                .JoinSessionAsync(userId, dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Session is not active");
        }

        [TestMethod]
        public async Task JoinSession_WhenFull_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            var participants = Enumerable.Range(0, 5)
                .Select(_ => new Participant
                {
                    LeftAt = null
                }).ToList();

            var session = new CollabSession
            {
                SessionId = sessionId,
                Status = "ACTIVE",
                MaxParticipants = 5,
                Participants = participants
            };

            var dto = new JoinSessionDto
            {
                SessionId = sessionId,
                Username = "newuser"
            };

            _repoMock.Setup(r =>
                r.FindByIdAsync(sessionId))
                .ReturnsAsync(session);

            // Act
            var act = async () => await _service
                .JoinSessionAsync(userId, dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Session is full");
        }

        // ===== END SESSION TESTS =====

        [TestMethod]
        public async Task EndSession_ByOwner_EndsSuccessfully()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            var session = new CollabSession
            {
                SessionId = sessionId,
                OwnerId = ownerId,
                Status = "ACTIVE",
                Participants = new List<Participant>()
            };

            _repoMock.Setup(r =>
                r.FindByIdAsync(sessionId))
                .ReturnsAsync(session);

            _repoMock.Setup(r =>
                r.UpdateAsync(It.IsAny<CollabSession>()))
                .ReturnsAsync((CollabSession s) => s);

            _redisMock.Setup(r =>
                r.CleanupSessionAsync(sessionId))
                .Returns(Task.CompletedTask);

            // Act
            var act = async () => await _service
                .EndSessionAsync(ownerId, sessionId);

            // Assert
            await act.Should().NotThrowAsync();
            _repoMock.Verify(r =>
                r.UpdateAsync(It.Is<CollabSession>(
                    s => s.Status == "ENDED")), Times.Once);
        }

        [TestMethod]
        public async Task EndSession_ByNonOwner_ThrowsException()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var nonOwnerId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            var session = new CollabSession
            {
                SessionId = sessionId,
                OwnerId = ownerId,
                Status = "ACTIVE",
                Participants = new List<Participant>()
            };

            _repoMock.Setup(r =>
                r.FindByIdAsync(sessionId))
                .ReturnsAsync(session);

            // Act
            var act = async () => await _service
                .EndSessionAsync(nonOwnerId, sessionId);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Only owner can end session");
        }
    }
}