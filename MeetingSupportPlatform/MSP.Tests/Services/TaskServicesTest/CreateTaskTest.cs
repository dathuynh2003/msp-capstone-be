using Microsoft.AspNetCore.Identity;
using Moq;
using MSP.Application.Models.Requests.ProjectTask;
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
    public class CreateTaskTest
    {
        private readonly Mock<IProjectTaskRepository> _mockProjectTaskRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IMilestoneRepository> _mockMilestoneRepository;
        private readonly Mock<ITodoRepository> _mockTodoRepository;
        private readonly Mock<ITaskHistoryService> _mockTaskHistoryService;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly IProjectTaskService _projectTaskService;

        public CreateTaskTest()
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

        [Fact]
        public async Task CreateTaskAsync_WithValidRequest_ReturnsSuccessResponse()
        {
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var actorId = Guid.NewGuid();

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Active",
                OwnerId = ownerId,
                CreatedById = ownerId,
                IsDeleted = false
            };

            var user = new User { Id = userId, Email = "test@example.com", FullName = "Test User" };

            var createRequest = new CreateTaskRequest
            {
                ProjectId = projectId,
                UserId = userId,
                Title = "Test Task",
                Description = "Test Description",
                Status = "Todo",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(5),
                ActorId = actorId,
                MilestoneIds = null
            };

            var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
            mockTransaction.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            mockTransaction.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            mockTransaction.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

            var mockStrategy = new SimpleExecutionStrategy();

            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _mockProjectTaskRepository.Setup(x => x.CreateExecutionStrategy()).Returns(mockStrategy);
            _mockProjectTaskRepository.Setup(x => x.BeginTransactionAsync()).Returns(Task.FromResult<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>(mockTransaction.Object));
            _mockProjectTaskRepository.Setup(x => x.AddAsync(It.IsAny<ProjectTask>())).ReturnsAsync((ProjectTask t) => t);
            _mockProjectTaskRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockTaskHistoryService.Setup(x => x.TrackTaskCreationAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>())).ReturnsAsync((TaskHistory)null);
            _mockTaskHistoryService.Setup(x => x.TrackTaskAssignmentAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync((TaskHistory)null);
            _mockNotificationService.Setup(x => x.CreateInAppNotificationAsync(It.IsAny<MSP.Application.Models.Requests.Notification.CreateNotificationRequest>()))
                .ReturnsAsync(ApiResponse<MSP.Application.Models.Responses.Notification.NotificationResponse>.SuccessResponse(new MSP.Application.Models.Responses.Notification.NotificationResponse()));
            _mockNotificationService.Setup(x => x.SendEmailNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            var result = await _projectTaskService.CreateTaskAsync(createRequest);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Test Task", result.Data.Title);
        }

        [Fact]
        public async Task CreateTaskAsync_WithNonExistentProject_ReturnsErrorResponse()
        {
            var projectId = Guid.NewGuid();

            var createRequest = new CreateTaskRequest
            {
                ProjectId = projectId,
                UserId = Guid.NewGuid(),
                Title = "Test Task",
                Description = "Test Description",
                Status = "Todo",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(5),
                ActorId = Guid.NewGuid(),
                MilestoneIds = null
            };

            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync((Project)null);

            var result = await _projectTaskService.CreateTaskAsync(createRequest);

            Assert.False(result.Success);
            Assert.Equal("Project not found", result.Message);
        }

        [Fact]
        public async Task CreateTaskAsync_WithNonExistentUser_ReturnsErrorResponse()
        {
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Active",
                OwnerId = ownerId,
                CreatedById = ownerId,
                IsDeleted = false
            };

            var createRequest = new CreateTaskRequest
            {
                ProjectId = projectId,
                UserId = userId,
                Title = "Test Task",
                Description = "Test Description",
                Status = "Todo",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(5),
                ActorId = Guid.NewGuid(),
                MilestoneIds = null
            };

            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((User)null);

            var result = await _projectTaskService.CreateTaskAsync(createRequest);

            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task CreateTaskAsync_WithInvalidDates_ReturnsErrorResponse()
        {
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Active",
                OwnerId = ownerId,
                CreatedById = ownerId,
                IsDeleted = false
            };

            var user = new User { Id = userId, Email = "test@example.com", FullName = "Test User" };

            var createRequest = new CreateTaskRequest
            {
                ProjectId = projectId,
                UserId = userId,
                Title = "Test Task",
                Description = "Test Description",
                Status = "Todo",
                StartDate = DateTime.UtcNow.AddMonths(2),
                EndDate = DateTime.UtcNow.AddMonths(3),
                ActorId = Guid.NewGuid(),
                MilestoneIds = null
            };

            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

            var result = await _projectTaskService.CreateTaskAsync(createRequest);

            Assert.False(result.Success);
        }
    }

    public class SimpleExecutionStrategy : Microsoft.EntityFrameworkCore.Storage.IExecutionStrategy
    {
        public bool RetriesOnFailure => false;

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, Func<Exception, int, Task<bool>>? verifySucceeded = null)
            => await operation();

        public async Task ExecuteAsync(Func<Task> operation, Func<Exception, Task<bool>>? verifySucceeded = null)
            => await operation();

        public T Execute<T>(Func<T> operation, Func<Exception, int, bool>? verifySucceeded = null)
            => operation();

        public void Execute(Action operation, Action<Exception, int>? verifySucceeded = null)
            => operation();

        public async Task<TResult> ExecuteAsync<TState, TResult>(TState state, Func<Microsoft.EntityFrameworkCore.DbContext, TState, CancellationToken, Task<TResult>> operation, Func<Microsoft.EntityFrameworkCore.DbContext, TState, CancellationToken, Task<Microsoft.EntityFrameworkCore.Storage.ExecutionResult<TResult>>>? verifySucceeded = null, CancellationToken cancellationToken = default)
            => await operation(null, state, cancellationToken);

        public TResult Execute<TState, TResult>(TState state, Func<Microsoft.EntityFrameworkCore.DbContext, TState, TResult> operation, Func<Microsoft.EntityFrameworkCore.DbContext, TState, Microsoft.EntityFrameworkCore.Storage.ExecutionResult<TResult>>? verifySucceeded = null)
            => operation(null, state);
    }
}
