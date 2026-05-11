using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeSync.ExecutionService.DTOs;
using CodeSync.ExecutionService.Interfaces;
using CodeSync.ExecutionService.Models;
using CodeSync.ExecutionService.Services;
using FluentAssertions;
using Moq;

namespace CodeSync.Tests.ExecutionService
{
    [TestClass]
    public class ExecutionServiceTests
    {
        private Mock<IExecutionRepository> _repoMock = null!;
        private Mock<IJudge0Client> _judge0Mock = null!;
        private ExecutionServiceImpl _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _repoMock = new Mock<IExecutionRepository>();
            _judge0Mock = new Mock<IJudge0Client>();

            _service = new ExecutionServiceImpl(
                _repoMock.Object,
                _judge0Mock.Object);
        }

        // ===== RUN CODE TESTS =====

        [TestMethod]
        public async Task RunCode_WithValidLanguage_ReturnsResult()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new RunCodeDto
            {
                ProjectId = Guid.NewGuid(),
                FileId = Guid.NewGuid(),
                Language = "python",
                SourceCode = "print('hello')",
                Stdin = ""
            };

            var judge0Result = new Judge0Result
            {
                StatusId = 3,
                StatusDescription = "Accepted",
                Stdout = "hello\n",
                Stderr = null,
                Time = "0.05",
                Memory = 1024
            };

            _repoMock.Setup(r =>
                r.CreateAsync(It.IsAny<ExecutionJob>()))
                .ReturnsAsync((ExecutionJob j) => j);

            _judge0Mock.Setup(j =>
                j.SubmitAndWait(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>()))
                .ReturnsAsync(judge0Result);

            _repoMock.Setup(r =>
                r.UpdateAsync(It.IsAny<ExecutionJob>()))
                .ReturnsAsync((ExecutionJob j) => j);

            // Act
            var result = await _service
                .RunCodeAsync(userId, dto);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be("COMPLETED");
            result.Stdout.Should().Be("hello\n");
        }

        [TestMethod]
        public async Task RunCode_WithUnsupportedLanguage_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new RunCodeDto
            {
                ProjectId = Guid.NewGuid(),
                FileId = Guid.NewGuid(),
                Language = "cobol",
                SourceCode = "some code"
            };

            _repoMock.Setup(r =>
                r.CreateAsync(It.IsAny<ExecutionJob>()))
                .ReturnsAsync((ExecutionJob j) => j);

            _repoMock.Setup(r =>
                r.UpdateAsync(It.IsAny<ExecutionJob>()))
                .ReturnsAsync((ExecutionJob j) => j);

            // Act
            var result = await _service
                .RunCodeAsync(userId, dto);

            // Assert
            result.Status.Should().Be("FAILED");
        }

        [TestMethod]
        public async Task RunCode_WithCompilationError_ReturnsFailed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new RunCodeDto
            {
                ProjectId = Guid.NewGuid(),
                FileId = Guid.NewGuid(),
                Language = "java",
                SourceCode = "invalid java code"
            };

            var judge0Result = new Judge0Result
            {
                StatusId = 6,
                StatusDescription = "Compilation Error",
                CompileOutput = "error: ';' expected",
                Stdout = null
            };

            _repoMock.Setup(r =>
                r.CreateAsync(It.IsAny<ExecutionJob>()))
                .ReturnsAsync((ExecutionJob j) => j);

            _judge0Mock.Setup(j =>
                j.SubmitAndWait(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>()))
                .ReturnsAsync(judge0Result);

            _repoMock.Setup(r =>
                r.UpdateAsync(It.IsAny<ExecutionJob>()))
                .ReturnsAsync((ExecutionJob j) => j);

            // Act
            var result = await _service
                .RunCodeAsync(userId, dto);

            // Assert
            result.Status.Should().Be("COMPILATION_ERROR");
            result.CompileOutput.Should()
                .Contain("error: ';' expected");
        }

        [TestMethod]
        public async Task RunCode_WithTimeout_ReturnsTimedOut()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new RunCodeDto
            {
                ProjectId = Guid.NewGuid(),
                FileId = Guid.NewGuid(),
                Language = "python",
                SourceCode = "while True: pass"
            };

            var judge0Result = new Judge0Result
            {
                StatusId = 5,
                StatusDescription = "Time Limit Exceeded",
                Stderr = "Execution timed out"
            };

            _repoMock.Setup(r =>
                r.CreateAsync(It.IsAny<ExecutionJob>()))
                .ReturnsAsync((ExecutionJob j) => j);

            _judge0Mock.Setup(j =>
                j.SubmitAndWait(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>()))
                .ReturnsAsync(judge0Result);

            _repoMock.Setup(r =>
                r.UpdateAsync(It.IsAny<ExecutionJob>()))
                .ReturnsAsync((ExecutionJob j) => j);

            // Act
            var result = await _service
                .RunCodeAsync(userId, dto);

            // Assert
            result.Status.Should().Be("TIMED_OUT");
        }

        // ===== GET RESULT TESTS =====

        [TestMethod]
        public async Task GetResult_WithValidId_ReturnsResult()
        {
            // Arrange
            var jobId = 1;
            var job = new ExecutionJob
            {
                Id = jobId,
                Language = "python",
                Status = "COMPLETED",
                Stdout = "hello\n"
            };

            _repoMock.Setup(r =>
                r.FindByIdAsync(jobId))
                .ReturnsAsync(job);

            // Act
            var result = await _service
                .GetResultAsync(jobId);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be("COMPLETED");
            result.Stdout.Should().Be("hello\n");
        }

        [TestMethod]
        public async Task GetResult_WithInvalidId_ThrowsException()
        {
            // Arrange
            _repoMock.Setup(r =>
                r.FindByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((ExecutionJob?)null);

            // Act
            var act = async () => await _service
                .GetResultAsync(999);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Execution not found");
        }
    }
}