using Microsoft.AspNetCore.Identity;
using Moq;
using MSP.Application.Models.Responses.ProjectTask;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.ProjectTask;
using MSP.Application.Services.Interfaces.ProjectTask;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Services.Interfaces.TaskHistory;
using MSP.Domain.Entities;
using MSP.Shared.Common;
using Xunit;

namespace MSP.Tests.Services.TaskServicesTest
{
    public class DeleteTaskTest
    {
        private readonly Mock<IProjectTaskRepository> _mockProjectTaskRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IMilestoneRepository> _mockMilestoneRepository;
        private readonly Mock<ITodoRepository> _mockTodoRepository;
        private readonly Mock<ITaskHistoryService> _mockTaskHistoryService;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<INotificationService> _mockNotificationService;

        private readonly IProjectTaskService _projectTaskService;

        public DeleteTaskTest()
        {
            _mockProjectTaskRepository = new Mock<IProjectTaskRepository>();
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockMilestoneRepository = new Mock<IMilestoneRepository>();
            _mockTodoRepository = new Mock<ITodoRepository>();
            _mockTaskHistoryService = new Mock<ITaskHistoryService>();

            _mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null
            );

            _mockNotificationService = new Mock<INotificationService>();

            _projectTaskService = new ProjectTaskService(
                _mockProjectTaskRepository.Object,
                _mockProjectRepository.Object,
                _mockMilestoneRepository.Object,
                _mockUserManager.Object,
                _mockTodoRepository.Object,
                _mockTaskHistoryService.Object,
                _mockNotificationService.Object
            );
        }

        private User CreateValidUser(Guid id, string email = "test@example.com", string fullName = "Test User")
        {
            return new User
            {
                Id = id,
                Email = email,
                FullName = fullName
            };
        }

        private ProjectTask CreateValidTask(Guid taskId, Guid projectId, Guid userId)
        {
            return new ProjectTask
            {
                Id = taskId,
                ProjectId = projectId,
                UserId = userId,
                ReviewerId = null,
                Title = "Test Task",
                Description = "Test Description",
                Status = "Todo",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(5),
                IsOverdue = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
                User = CreateValidUser(userId),
                Reviewer = null,
                Milestones = new List<Milestone>()
            };
        }

        [Fact]
        public async Task DeleteTaskAsync_WithValidTaskId_ReturnsSuccessResponse()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var task = CreateValidTask(taskId, projectId, userId);

            _mockProjectTaskRepository
                .Setup(x => x.GetTaskByIdAsync(taskId))
                .ReturnsAsync(task);

            _mockProjectTaskRepository
                .Setup(x => x.SoftDeleteAsync(It.IsAny<ProjectTask>()))
                .Returns(Task.CompletedTask);

            _mockProjectTaskRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _projectTaskService.DeleteTaskAsync(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Request was successful", result.Message);

            _mockProjectTaskRepository.Verify(x => x.SoftDeleteAsync(It.IsAny<ProjectTask>()), Times.Once);
            _mockProjectTaskRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteTaskAsync_WithNonExistentTaskId_ReturnsErrorResponse()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _mockProjectTaskRepository
                .Setup(x => x.GetTaskByIdAsync(taskId))
                .ReturnsAsync((ProjectTask)null);

            // Act
            var result = await _projectTaskService.DeleteTaskAsync(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("Task not found", result.Message);

            _mockProjectTaskRepository.Verify(x => x.SoftDeleteAsync(It.IsAny<ProjectTask>()), Times.Never);
        }

        [Fact]
        public async Task DeleteTaskAsync_PerformsSoftDelete()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var task = CreateValidTask(taskId, projectId, userId);

            _mockProjectTaskRepository
                .Setup(x => x.GetTaskByIdAsync(taskId))
                .ReturnsAsync(task);

            _mockProjectTaskRepository
                .Setup(x => x.SoftDeleteAsync(It.IsAny<ProjectTask>()))
                .Returns(Task.CompletedTask);

            _mockProjectTaskRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _projectTaskService.DeleteTaskAsync(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            _mockProjectTaskRepository.Verify(
                x => x.SoftDeleteAsync(It.Is<ProjectTask>(t => t.Id == taskId)),
                Times.Once);
        }

        [Fact]
        public async Task DeleteTaskAsync_SavesChangesAfterDelete()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var task = CreateValidTask(taskId, projectId, userId);

            _mockProjectTaskRepository
                .Setup(x => x.GetTaskByIdAsync(taskId))
                .ReturnsAsync(task);

            _mockProjectTaskRepository
                .Setup(x => x.SoftDeleteAsync(It.IsAny<ProjectTask>()))
                .Returns(Task.CompletedTask);

            _mockProjectTaskRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _projectTaskService.DeleteTaskAsync(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            _mockProjectTaskRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteTaskAsync_WithAlreadyDeletedTask_ReturnsErrorResponse()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var task = CreateValidTask(taskId, projectId, userId);
            task.IsDeleted = true;

            _mockProjectTaskRepository
                .Setup(x => x.GetTaskByIdAsync(taskId))
                .ReturnsAsync(task);

            // Act
            var result = await _projectTaskService.DeleteTaskAsync(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("Task not found", result.Message);

            _mockProjectTaskRepository.Verify(x => x.SoftDeleteAsync(It.IsAny<ProjectTask>()), Times.Never);
        }

        [Fact]
        public async Task DeleteTaskAsync_ReturnsCorrectSuccessMessage()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var task = CreateValidTask(taskId, projectId, userId);

            _mockProjectTaskRepository
                .Setup(x => x.GetTaskByIdAsync(taskId))
                .ReturnsAsync(task);

            _mockProjectTaskRepository
                .Setup(x => x.SoftDeleteAsync(It.IsAny<ProjectTask>()))
                .Returns(Task.CompletedTask);

            _mockProjectTaskRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _projectTaskService.DeleteTaskAsync(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Message);
            Assert.Contains("Request was successful", result.Message);
        }
    }
}
