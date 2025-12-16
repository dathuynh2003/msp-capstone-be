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
    public class GetTaskDetailTest
    {
        private readonly Mock<IProjectTaskRepository> _mockProjectTaskRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IMilestoneRepository> _mockMilestoneRepository;
        private readonly Mock<ITodoRepository> _mockTodoRepository;
        private readonly Mock<ITaskHistoryService> _mockTaskHistoryService;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IProjectMemberRepository> _mockProjectMemberRepositoryMock;
        private readonly IProjectTaskService _projectTaskService;

        public GetTaskDetailTest()
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
                _mockNotificationService.Object,
                _mockProjectMemberRepositoryMock.Object
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

        private ProjectTask CreateValidTask(Guid taskId, Guid projectId, Guid userId, Guid? reviewerId = null)
        {
            return new ProjectTask
            {
                Id = taskId,
                ProjectId = projectId,
                UserId = userId,
                ReviewerId = reviewerId,
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
                Reviewer = reviewerId.HasValue ? CreateValidUser(reviewerId.Value, "reviewer@example.com", "Reviewer") : null,
                Milestones = new List<Milestone>()
            };
        }

        [Fact]
        public async Task GetTaskByIdAsync_WithValidId_ReturnsSuccessResponse()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var task = CreateValidTask(taskId, projectId, userId);

            _mockProjectTaskRepository
                .Setup(x => x.GetTaskByIdAsync(taskId))
                .ReturnsAsync(task);

            // Act
            var result = await _projectTaskService.GetTaskByIdAsync(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(taskId, result.Data.Id);
            Assert.Equal("Test Task", result.Data.Title);
            Assert.Equal(projectId, result.Data.ProjectId);
            Assert.Equal(userId, result.Data.UserId);
        }

        [Fact]
        public async Task GetTaskByIdAsync_WithNonExistentId_ReturnsErrorResponse()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _mockProjectTaskRepository
                .Setup(x => x.GetTaskByIdAsync(taskId))
                .ReturnsAsync((ProjectTask)null);

            // Act
            var result = await _projectTaskService.GetTaskByIdAsync(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("Task not found", result.Message);
        }

        [Fact]
        public async Task GetTaskByIdAsync_IncludesUserDetails()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var user = CreateValidUser(userId, "assignee@example.com", "Assignee Name");
            var task = CreateValidTask(taskId, projectId, userId);
            task.User = user;

            _mockProjectTaskRepository
                .Setup(x => x.GetTaskByIdAsync(taskId))
                .ReturnsAsync(task);

            // Act
            var result = await _projectTaskService.GetTaskByIdAsync(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.User);
            Assert.Equal(userId, result.Data.User.Id);
            Assert.Equal("assignee@example.com", result.Data.User.Email);
            Assert.Equal("Assignee Name", result.Data.User.FullName);
        }

        [Fact]
        public async Task GetTaskByIdAsync_IncludesReviewerDetails()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var reviewerId = Guid.NewGuid();

            var reviewer = CreateValidUser(reviewerId, "reviewer@example.com", "Reviewer Name");
            var task = CreateValidTask(taskId, projectId, userId, reviewerId);
            task.Reviewer = reviewer;

            _mockProjectTaskRepository
                .Setup(x => x.GetTaskByIdAsync(taskId))
                .ReturnsAsync(task);

            // Act
            var result = await _projectTaskService.GetTaskByIdAsync(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.Reviewer);
            Assert.Equal(reviewerId, result.Data.Reviewer.Id);
            Assert.Equal("reviewer@example.com", result.Data.Reviewer.Email);
            Assert.Equal("Reviewer Name", result.Data.Reviewer.FullName);
        }

        [Fact]
        public async Task GetTaskByIdAsync_IncludesMilestones()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var milestoneId = Guid.NewGuid();

            var milestone = new Milestone
            {
                Id = milestoneId,
                ProjectId = projectId,
                Name = "Test Milestone",
                DueDate = DateTime.UtcNow.AddMonths(1),
                IsDeleted = false
            };

            var task = CreateValidTask(taskId, projectId, userId);
            task.Milestones = new List<Milestone> { milestone };

            _mockProjectTaskRepository
                .Setup(x => x.GetTaskByIdAsync(taskId))
                .ReturnsAsync(task);

            // Act
            var result = await _projectTaskService.GetTaskByIdAsync(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.Milestones);
            Assert.Single(result.Data.Milestones);
            Assert.Equal(milestoneId, result.Data.Milestones[0].Id);
            Assert.Equal("Test Milestone", result.Data.Milestones[0].Name);
        }

        [Fact]
        public async Task GetTaskByIdAsync_WithoutAssignee_ReturnsNullUser()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var task = new ProjectTask
            {
                Id = taskId,
                ProjectId = projectId,
                UserId = null,
                ReviewerId = null,
                Title = "Unassigned Task",
                Description = "Test Description",
                Status = "Todo",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(5),
                IsOverdue = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
                User = null,
                Reviewer = null,
                Milestones = new List<Milestone>()
            };

            _mockProjectTaskRepository
                .Setup(x => x.GetTaskByIdAsync(taskId))
                .ReturnsAsync(task);

            // Act
            var result = await _projectTaskService.GetTaskByIdAsync(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Null(result.Data.User);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ReturnsCompleteTaskDetails()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var reviewerId = Guid.NewGuid();

            var user = CreateValidUser(userId);
            var reviewer = CreateValidUser(reviewerId, "reviewer@example.com", "Reviewer");
            var task = CreateValidTask(taskId, projectId, userId, reviewerId);
            task.User = user;
            task.Reviewer = reviewer;
            task.Status = "InProgress";
            task.IsOverdue = true;

            _mockProjectTaskRepository
                .Setup(x => x.GetTaskByIdAsync(taskId))
                .ReturnsAsync(task);

            // Act
            var result = await _projectTaskService.GetTaskByIdAsync(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(taskId, result.Data.Id);
            Assert.Equal("Test Task", result.Data.Title);
            Assert.Equal("Test Description", result.Data.Description);
            Assert.Equal("InProgress", result.Data.Status);
            Assert.True(result.Data.IsOverdue);
            Assert.NotNull(result.Data.User);
            Assert.NotNull(result.Data.Reviewer);
        }
    }
}
