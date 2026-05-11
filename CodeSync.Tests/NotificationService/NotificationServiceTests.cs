using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeSync.NotificationService.DTOs;
using CodeSync.NotificationService.Interfaces;
using CodeSync.NotificationService.Models;
using CodeSync.NotificationService.Services;
using FluentAssertions;
using Moq;

namespace CodeSync.Tests.NotificationService
{
    [TestClass]
    public class NotificationServiceTests
    {
        private Mock<INotificationRepository> _repoMock = null!;
        private NotificationServiceImpl _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _repoMock = new Mock<INotificationRepository>();
            _service = new NotificationServiceImpl(
                _repoMock.Object);
        }

        // ===== CREATE TESTS =====

        [TestMethod]
        public async Task Create_WithValidData_ReturnsNotification()
        {
            // Arrange
            var dto = new CreateNotificationDto
            {
                RecipientId = Guid.NewGuid(),
                ActorId = Guid.NewGuid(),
                Type = "COMMENT_ADDED",
                Title = "New comment",
                Message = "Someone commented",
                RelatedId = "123",
                RelatedType = "COMMENT"
            };

            _repoMock.Setup(r =>
                r.CreateAsync(It.IsAny<Notification>()))
                .ReturnsAsync((Notification n) => n);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Type.Should().Be("COMMENT_ADDED");
            result.Title.Should().Be("New comment");
            result.IsRead.Should().BeFalse();
        }

        // ===== GET NOTIFICATIONS TESTS =====

        [TestMethod]
        public async Task GetMyNotifications_ReturnsUserNotifications()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var notifications = new List<Notification>
            {
                new()
                {
                    Id = 1,
                    RecipientId = userId,
                    Title = "Notif 1",
                    IsRead = false
                },
                new()
                {
                    Id = 2,
                    RecipientId = userId,
                    Title = "Notif 2",
                    IsRead = true
                }
            };

            _repoMock.Setup(r =>
                r.GetByRecipientAsync(userId))
                .ReturnsAsync(notifications);

            // Act
            var result = await _service
                .GetMyNotificationsAsync(userId);

            // Assert
            result.Should().HaveCount(2);
            result[0].Title.Should().Be("Notif 1");
        }

        // ===== UNREAD COUNT TESTS =====

        [TestMethod]
        public async Task GetUnreadCount_ReturnsCorrectCount()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _repoMock.Setup(r =>
                r.GetUnreadCountAsync(userId))
                .ReturnsAsync(5);

            // Act
            var result = await _service
                .GetUnreadCountAsync(userId);

            // Assert
            result.Should().Be(5);
        }

        // ===== MARK READ TESTS =====

        [TestMethod]
        public async Task MarkRead_WithValidId_MarksAsRead()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var notifId = 1;

            var notification = new Notification
            {
                Id = notifId,
                RecipientId = userId,
                IsRead = false
            };

            _repoMock.Setup(r =>
                r.FindByIdAsync(notifId))
                .ReturnsAsync(notification);

            _repoMock.Setup(r =>
                r.UpdateAsync(It.IsAny<Notification>()))
                .ReturnsAsync((Notification n) => n);

            // Act
            await _service.MarkReadAsync(userId, notifId);

            // Assert
            _repoMock.Verify(r =>
                r.UpdateAsync(It.Is<Notification>(
                    n => n.IsRead == true)), Times.Once);
        }

        [TestMethod]
        public async Task MarkRead_ByWrongUser_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var wrongUserId = Guid.NewGuid();
            var notifId = 1;

            var notification = new Notification
            {
                Id = notifId,
                RecipientId = userId,
                IsRead = false
            };

            _repoMock.Setup(r =>
                r.FindByIdAsync(notifId))
                .ReturnsAsync(notification);

            // Act
            var act = async () => await _service
                .MarkReadAsync(wrongUserId, notifId);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Not authorized");
        }

        [TestMethod]
        public async Task MarkRead_WithInvalidId_ThrowsException()
        {
            // Arrange
            _repoMock.Setup(r =>
                r.FindByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Notification?)null);

            // Act
            var act = async () => await _service
                .MarkReadAsync(Guid.NewGuid(), 999);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Notification not found");
        }

        // ===== MARK ALL READ TESTS =====

        [TestMethod]
        public async Task MarkAllRead_CallsRepository()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _repoMock.Setup(r =>
                r.MarkAllReadAsync(userId))
                .Returns(Task.CompletedTask);

            // Act
            await _service.MarkAllReadAsync(userId);

            // Assert
            _repoMock.Verify(r =>
                r.MarkAllReadAsync(userId), Times.Once);
        }

        // ===== BROADCAST TESTS =====

        [TestMethod]
        public async Task Broadcast_SendsToAllRecipients()
        {
            // Arrange
            var actorId = Guid.NewGuid();
            var recipients = new List<Guid>
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()
            };

            var dto = new BroadcastDto
            {
                Title = "System Update",
                Message = "Platform maintenance",
                RecipientIds = recipients
            };

            _repoMock.Setup(r =>
                r.CreateAsync(It.IsAny<Notification>()))
                .ReturnsAsync((Notification n) => n);

            // Act
            await _service.BroadcastAsync(actorId, dto);

            // Assert
            _repoMock.Verify(r =>
                r.CreateAsync(It.IsAny<Notification>()),
                Times.Exactly(3));
        }
    }
}