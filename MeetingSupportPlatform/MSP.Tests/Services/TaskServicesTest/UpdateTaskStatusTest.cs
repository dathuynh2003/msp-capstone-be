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
    public class UpdateTaskStatusTest
    {
        private readonly Mock<IProjectTaskRepository> _mockProjectTaskRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IMilestoneRepository> _mockMilestoneRepository;
        private readonly Mock<ITodoRepository> _mockTodoRepository;
        private readonly Mock<ITaskHistoryService> _mockTaskHistoryService;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly IProjectTaskService _projectTaskService;

        public UpdateTaskStatusTest()
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
        public async Task UpdateTaskStatus_ChangeToInProgress_ReturnsSuccessResponse()
        {
            var taskId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var actorId = Guid.NewGuid();
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

            var task = new ProjectTask
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
                User = new User { Id = userId, Email = "test@example.com", FullName = "Test User" },
                Reviewer = null,
                Milestones = new List<Milestone>()
            };

            var actor = new User { Id = actorId, Email = "actor@example.com", FullName = "Actor User" };

            var updateRequest = new UpdateTaskRequest
            {
                Id = taskId,
                Title = null,
                Description = null,
                Status = "InProgress",
                StartDate = null,
                EndDate = null,
                UserId = userId,
                ReviewerId = null,
                ActorId = actorId,
                MilestoneIds = null
            };

            var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
            mockTransaction.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            mockTransaction.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            mockTransaction.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

            var mockStrategy = new SimpleExecutionStrategy();

            _mockProjectTaskRepository.Setup(x => x.GetTaskByIdAsync(taskId)).ReturnsAsync(task);
            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockUserManager.Setup(x => x.FindByIdAsync(actorId.ToString())).ReturnsAsync(actor);
            _mockUserManager.Setup(x => x.GetRolesAsync(actor)).ReturnsAsync(new List<string> { "ProjectManager" });
            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(task.User);
            _mockProjectTaskRepository.Setup(x => x.CreateExecutionStrategy()).Returns(mockStrategy);
            _mockProjectTaskRepository.Setup(x => x.BeginTransactionAsync()).Returns(Task.FromResult<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>(mockTransaction.Object));
            _mockProjectTaskRepository.Setup(x => x.UpdateAsync(It.IsAny<ProjectTask>())).Returns(Task.CompletedTask);
            _mockProjectTaskRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockTaskHistoryService.Setup(x => x.TrackStatusChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync((TaskHistory)null);
            _mockNotificationService.Setup(x => x.CreateInAppNotificationAsync(It.IsAny<MSP.Application.Models.Requests.Notification.CreateNotificationRequest>()))
                .ReturnsAsync(ApiResponse<MSP.Application.Models.Responses.Notification.NotificationResponse>.SuccessResponse(new MSP.Application.Models.Responses.Notification.NotificationResponse()));
            _mockNotificationService.Setup(x => x.SendEmailNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            var result = await _projectTaskService.UpdateTaskAsync(updateRequest);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("InProgress", result.Data.Status);
        }

        [Fact]
        public async Task UpdateTaskStatus_ChangeToReadyToReview_SendsNotificationToReviewer()
        {
            var taskId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var reviewerId = Guid.NewGuid();
            var actorId = Guid.NewGuid();
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

            var reviewer = new User { Id = reviewerId, Email = "reviewer@example.com", FullName = "Reviewer User" };

            var task = new ProjectTask
            {
                Id = taskId,
                ProjectId = projectId,
                UserId = userId,
                ReviewerId = reviewerId,
                Title = "Test Task",
                Description = "Test Description",
                Status = "InProgress",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(5),
                IsOverdue = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
                User = new User { Id = userId, Email = "test@example.com", FullName = "Test User" },
                Reviewer = reviewer,
                Milestones = new List<Milestone>()
            };

            var actor = new User { Id = actorId, Email = "actor@example.com", FullName = "Actor User" };

            var updateRequest = new UpdateTaskRequest
            {
                Id = taskId,
                Title = null,
                Description = null,
                Status = "ReadyToReview",
                StartDate = null,
                EndDate = null,
                UserId = userId,
                ReviewerId = reviewerId,
                ActorId = actorId,
                MilestoneIds = null
            };

            var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
            mockTransaction.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            mockTransaction.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            mockTransaction.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

            var mockStrategy = new SimpleExecutionStrategy();

            _mockProjectTaskRepository.Setup(x => x.GetTaskByIdAsync(taskId)).ReturnsAsync(task);
            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockUserManager.Setup(x => x.FindByIdAsync(actorId.ToString())).ReturnsAsync(actor);
            _mockUserManager.Setup(x => x.GetRolesAsync(actor)).ReturnsAsync(new List<string> { "ProjectManager" });
            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(task.User);
            _mockUserManager.Setup(x => x.FindByIdAsync(reviewerId.ToString())).ReturnsAsync(reviewer);
            _mockUserManager.Setup(x => x.GetRolesAsync(reviewer)).ReturnsAsync(new List<string> { "ProjectManager" });
            _mockProjectTaskRepository.Setup(x => x.CreateExecutionStrategy()).Returns(mockStrategy);
            _mockProjectTaskRepository.Setup(x => x.BeginTransactionAsync()).Returns(Task.FromResult<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>(mockTransaction.Object));
            _mockProjectTaskRepository.Setup(x => x.UpdateAsync(It.IsAny<ProjectTask>())).Returns(Task.CompletedTask);
            _mockProjectTaskRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockTaskHistoryService.Setup(x => x.TrackStatusChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync((TaskHistory)null);
            _mockNotificationService.Setup(x => x.CreateInAppNotificationAsync(It.IsAny<MSP.Application.Models.Requests.Notification.CreateNotificationRequest>()))
                .ReturnsAsync(ApiResponse<MSP.Application.Models.Responses.Notification.NotificationResponse>.SuccessResponse(new MSP.Application.Models.Responses.Notification.NotificationResponse()));
            _mockNotificationService.Setup(x => x.SendEmailNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            var result = await _projectTaskService.UpdateTaskAsync(updateRequest);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("ReadyToReview", result.Data.Status);
            _mockNotificationService.Verify(
                x => x.CreateInAppNotificationAsync(It.IsAny<MSP.Application.Models.Requests.Notification.CreateNotificationRequest>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task UpdateTaskStatus_ChangeToDone_ReturnsSuccessResponse()
        {
            var taskId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var actorId = Guid.NewGuid();
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

            var task = new ProjectTask
            {
                Id = taskId,
                ProjectId = projectId,
                UserId = userId,
                ReviewerId = null,
                Title = "Test Task",
                Description = "Test Description",
                Status = "ReadyToReview",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(5),
                IsOverdue = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
                User = new User { Id = userId, Email = "test@example.com", FullName = "Test User" },
                Reviewer = null,
                Milestones = new List<Milestone>()
            };

            var actor = new User { Id = actorId, Email = "actor@example.com", FullName = "Actor User" };

            var updateRequest = new UpdateTaskRequest
            {
                Id = taskId,
                Title = null,
                Description = null,
                Status = "Done",
                StartDate = null,
                EndDate = null,
                UserId = userId,
                ReviewerId = null,
                ActorId = actorId,
                MilestoneIds = null
            };

            var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
            mockTransaction.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            mockTransaction.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            mockTransaction.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

            var mockStrategy = new SimpleExecutionStrategy();

            _mockProjectTaskRepository.Setup(x => x.GetTaskByIdAsync(taskId)).ReturnsAsync(task);
            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockUserManager.Setup(x => x.FindByIdAsync(actorId.ToString())).ReturnsAsync(actor);
            _mockUserManager.Setup(x => x.GetRolesAsync(actor)).ReturnsAsync(new List<string> { "ProjectManager" });
            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(task.User);
            _mockProjectTaskRepository.Setup(x => x.CreateExecutionStrategy()).Returns(mockStrategy);
            _mockProjectTaskRepository.Setup(x => x.BeginTransactionAsync()).Returns(Task.FromResult<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>(mockTransaction.Object));
            _mockProjectTaskRepository.Setup(x => x.UpdateAsync(It.IsAny<ProjectTask>())).Returns(Task.CompletedTask);
            _mockProjectTaskRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockTaskHistoryService.Setup(x => x.TrackStatusChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync((TaskHistory)null);
            _mockNotificationService.Setup(x => x.CreateInAppNotificationAsync(It.IsAny<MSP.Application.Models.Requests.Notification.CreateNotificationRequest>()))
                .ReturnsAsync(ApiResponse<MSP.Application.Models.Responses.Notification.NotificationResponse>.SuccessResponse(new MSP.Application.Models.Responses.Notification.NotificationResponse()));
            _mockNotificationService.Setup(x => x.SendEmailNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            var result = await _projectTaskService.UpdateTaskAsync(updateRequest);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Done", result.Data.Status);
        }

        [Fact]
        public async Task UpdateTaskStatus_ChangeToReOpened_SendsNotificationToAssignee()
        {
            var taskId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var actorId = Guid.NewGuid();
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

            var task = new ProjectTask
            {
                Id = taskId,
                ProjectId = projectId,
                UserId = userId,
                ReviewerId = null,
                Title = "Test Task",
                Description = "Test Description",
                Status = "ReadyToReview",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(5),
                IsOverdue = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
                User = new User { Id = userId, Email = "test@example.com", FullName = "Test User" },
                Reviewer = null,
                Milestones = new List<Milestone>()
            };

            var actor = new User { Id = actorId, Email = "actor@example.com", FullName = "Actor User" };

            var updateRequest = new UpdateTaskRequest
            {
                Id = taskId,
                Title = null,
                Description = null,
                Status = "ReOpened",
                StartDate = null,
                EndDate = null,
                UserId = userId,
                ReviewerId = null,
                ActorId = actorId,
                MilestoneIds = null
            };

            var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
            mockTransaction.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            mockTransaction.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            mockTransaction.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

            var mockStrategy = new SimpleExecutionStrategy();

            _mockProjectTaskRepository.Setup(x => x.GetTaskByIdAsync(taskId)).ReturnsAsync(task);
            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockUserManager.Setup(x => x.FindByIdAsync(actorId.ToString())).ReturnsAsync(actor);
            _mockUserManager.Setup(x => x.GetRolesAsync(actor)).ReturnsAsync(new List<string> { "ProjectManager" });
            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(task.User);
            _mockProjectTaskRepository.Setup(x => x.CreateExecutionStrategy()).Returns(mockStrategy);
            _mockProjectTaskRepository.Setup(x => x.BeginTransactionAsync()).Returns(Task.FromResult<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>(mockTransaction.Object));
            _mockProjectTaskRepository.Setup(x => x.UpdateAsync(It.IsAny<ProjectTask>())).Returns(Task.CompletedTask);
            _mockProjectTaskRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockTaskHistoryService.Setup(x => x.TrackStatusChangeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync((TaskHistory)null);
            _mockNotificationService.Setup(x => x.CreateInAppNotificationAsync(It.IsAny<MSP.Application.Models.Requests.Notification.CreateNotificationRequest>()))
                .ReturnsAsync(ApiResponse<MSP.Application.Models.Responses.Notification.NotificationResponse>.SuccessResponse(new MSP.Application.Models.Responses.Notification.NotificationResponse()));
            _mockNotificationService.Setup(x => x.SendEmailNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            var result = await _projectTaskService.UpdateTaskAsync(updateRequest);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("ReOpened", result.Data.Status);
            _mockNotificationService.Verify(
                x => x.CreateInAppNotificationAsync(It.IsAny<MSP.Application.Models.Requests.Notification.CreateNotificationRequest>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task UpdateTaskStatus_InvalidTransition_ReturnsErrorResponse()
        {
            var taskId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var actorId = Guid.NewGuid();
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

            var task = new ProjectTask
            {
                Id = taskId,
                ProjectId = projectId,
                UserId = userId,
                ReviewerId = null,
                Title = "Test Task",
                Description = "Test Description",
                Status = "Done",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(5),
                IsOverdue = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
                User = new User { Id = userId, Email = "test@example.com", FullName = "Test User" },
                Reviewer = null,
                Milestones = new List<Milestone>()
            };

            var actor = new User { Id = actorId, Email = "actor@example.com", FullName = "Actor User" };

            var updateRequest = new UpdateTaskRequest
            {
                Id = taskId,
                Title = null,
                Description = null,
                Status = "Todo",
                StartDate = null,
                EndDate = null,
                UserId = userId,
                ReviewerId = null,
                ActorId = actorId,
                MilestoneIds = null
            };

            _mockProjectTaskRepository.Setup(x => x.GetTaskByIdAsync(taskId)).ReturnsAsync(task);
            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockUserManager.Setup(x => x.FindByIdAsync(actorId.ToString())).ReturnsAsync(actor);
            _mockUserManager.Setup(x => x.GetRolesAsync(actor)).ReturnsAsync(new List<string> { "ProjectManager" });

            var result = await _projectTaskService.UpdateTaskAsync(updateRequest);

            Assert.False(result.Success);
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
}
