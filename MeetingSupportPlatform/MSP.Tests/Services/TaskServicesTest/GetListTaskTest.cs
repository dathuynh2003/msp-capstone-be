using Microsoft.AspNetCore.Identity;
using Moq;
using MSP.Application.Models.Requests;
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
    public class GetListTaskTest
    {
        private readonly Mock<IProjectTaskRepository> _mockProjectTaskRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IMilestoneRepository> _mockMilestoneRepository;
        private readonly Mock<ITodoRepository> _mockTodoRepository;
        private readonly Mock<ITaskHistoryService> _mockTaskHistoryService;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly IProjectTaskService _projectTaskService;

        public GetListTaskTest()
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
        public async Task GetTasksByProjectIdAsync_WithValidProjectId_ReturnsTaskList()
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

            var tasks = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    UserId = userId,
                    ReviewerId = null,
                    Title = "Task 1",
                    Description = "Description 1",
                    Status = "Todo",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(5),
                    IsOverdue = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    User = user,
                    Reviewer = null,
                    Milestones = new List<Milestone>()
                },
                new ProjectTask
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    UserId = userId,
                    ReviewerId = null,
                    Title = "Task 2",
                    Description = "Description 2",
                    Status = "InProgress",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(7),
                    IsOverdue = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    User = user,
                    Reviewer = null,
                    Milestones = new List<Milestone>()
                }
            };

            var pagingRequest = new PagingRequest { PageIndex = 1, PageSize = 10 };

            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockProjectTaskRepository.Setup(x => x.FindWithIncludePagedAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<ProjectTask, bool>>>(),
                It.IsAny<System.Func<IQueryable<ProjectTask>, IQueryable<ProjectTask>>>(),
                It.IsAny<System.Func<IQueryable<ProjectTask>, IOrderedQueryable<ProjectTask>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>()
            )).ReturnsAsync(tasks);
            _mockProjectTaskRepository.Setup(x => x.CountAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProjectTask, bool>>>()))
                .ReturnsAsync(tasks.Count);

            var result = await _projectTaskService.GetTasksByProjectIdAsync(pagingRequest, projectId);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Items.Count());
            Assert.Equal("Task 1", result.Data.Items.First().Title);
            Assert.Equal("Task 2", result.Data.Items.Last().Title);
        }

        [Fact]
        public async Task GetTasksByProjectIdAsync_WithNonExistentProject_ReturnsErrorResponse()
        {
            var projectId = Guid.NewGuid();
            var pagingRequest = new PagingRequest { PageIndex = 1, PageSize = 10 };

            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync((Project)null);

            var result = await _projectTaskService.GetTasksByProjectIdAsync(pagingRequest, projectId);

            Assert.False(result.Success);
            Assert.Equal("Project not found", result.Message);
        }

        [Fact]
        public async Task GetTasksByProjectIdAsync_WithNoTasks_ReturnsErrorResponse()
        {
            var projectId = Guid.NewGuid();
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

            var pagingRequest = new PagingRequest { PageIndex = 1, PageSize = 10 };

            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockProjectTaskRepository.Setup(x => x.FindWithIncludePagedAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<ProjectTask, bool>>>(),
                It.IsAny<System.Func<IQueryable<ProjectTask>, IQueryable<ProjectTask>>>(),
                It.IsAny<System.Func<IQueryable<ProjectTask>, IOrderedQueryable<ProjectTask>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>()
            )).ReturnsAsync(new List<ProjectTask>());

            var result = await _projectTaskService.GetTasksByProjectIdAsync(pagingRequest, projectId);

            Assert.False(result.Success);
            Assert.Equal("No tasks found for the project", result.Message);
        }

        [Fact]
        public async Task GetTasksByProjectIdAsync_WithTasksAndReviewers_ReturnsTasksWithReviewerInfo()
        {
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var reviewerId = Guid.NewGuid();
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
            var reviewer = new User { Id = reviewerId, Email = "reviewer@example.com", FullName = "Reviewer User" };

            var tasks = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    UserId = userId,
                    ReviewerId = reviewerId,
                    Title = "Task with Reviewer",
                    Description = "Description",
                    Status = "ReadyToReview",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(5),
                    IsOverdue = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    User = user,
                    Reviewer = reviewer,
                    Milestones = new List<Milestone>()
                }
            };

            var pagingRequest = new PagingRequest { PageIndex = 1, PageSize = 10 };

            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockProjectTaskRepository.Setup(x => x.FindWithIncludePagedAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<ProjectTask, bool>>>(),
                It.IsAny<System.Func<IQueryable<ProjectTask>, IQueryable<ProjectTask>>>(),
                It.IsAny<System.Func<IQueryable<ProjectTask>, IOrderedQueryable<ProjectTask>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>()
            )).ReturnsAsync(tasks);
            _mockProjectTaskRepository.Setup(x => x.CountAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProjectTask, bool>>>()))
                .ReturnsAsync(tasks.Count);

            var result = await _projectTaskService.GetTasksByProjectIdAsync(pagingRequest, projectId);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.Items);
            Assert.NotNull(result.Data.Items.First().Reviewer);
            Assert.Equal("Reviewer User", result.Data.Items.First().Reviewer.FullName);
        }

        [Fact]
        public async Task GetTasksByProjectIdAsync_WithMilestones_ReturnsMilestoneInfo()
        {
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var milestoneId = Guid.NewGuid();
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
            var milestone = new Milestone
            {
                Id = milestoneId,
                ProjectId = projectId,
                Name = "Milestone 1",
                DueDate = DateTime.UtcNow.AddDays(10),
                IsDeleted = false
            };

            var tasks = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    UserId = userId,
                    ReviewerId = null,
                    Title = "Task with Milestone",
                    Description = "Description",
                    Status = "Todo",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(5),
                    IsOverdue = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    User = user,
                    Reviewer = null,
                    Milestones = new List<Milestone> { milestone }
                }
            };

            var pagingRequest = new PagingRequest { PageIndex = 1, PageSize = 10 };

            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockProjectTaskRepository.Setup(x => x.FindWithIncludePagedAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<ProjectTask, bool>>>(),
                It.IsAny<System.Func<IQueryable<ProjectTask>, IQueryable<ProjectTask>>>(),
                It.IsAny<System.Func<IQueryable<ProjectTask>, IOrderedQueryable<ProjectTask>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>()
            )).ReturnsAsync(tasks);
            _mockProjectTaskRepository.Setup(x => x.CountAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProjectTask, bool>>>()))
                .ReturnsAsync(tasks.Count);

            var result = await _projectTaskService.GetTasksByProjectIdAsync(pagingRequest, projectId);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.Items);
            Assert.NotNull(result.Data.Items.First().Milestones);
            Assert.Single(result.Data.Items.First().Milestones);
            Assert.Equal("Milestone 1", result.Data.Items.First().Milestones.First().Name);
        }

        [Fact]
        public async Task GetTasksByProjectIdAsync_ReturnsPagingInfo()
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

            var tasks = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    UserId = userId,
                    ReviewerId = null,
                    Title = "Task 1",
                    Description = "Description 1",
                    Status = "Todo",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(5),
                    IsOverdue = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    User = user,
                    Reviewer = null,
                    Milestones = new List<Milestone>()
                }
            };

            var pagingRequest = new PagingRequest { PageIndex = 2, PageSize = 5 };

            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockProjectTaskRepository.Setup(x => x.FindWithIncludePagedAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<ProjectTask, bool>>>(),
                It.IsAny<System.Func<IQueryable<ProjectTask>, IQueryable<ProjectTask>>>(),
                It.IsAny<System.Func<IQueryable<ProjectTask>, IOrderedQueryable<ProjectTask>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>()
            )).ReturnsAsync(tasks);
            _mockProjectTaskRepository.Setup(x => x.CountAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProjectTask, bool>>>()))
                .ReturnsAsync(15);

            var result = await _projectTaskService.GetTasksByProjectIdAsync(pagingRequest, projectId);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.PageIndex);
            Assert.Equal(5, result.Data.PageSize);
            Assert.Equal(15, result.Data.TotalItems);
        }
    }
}
