using Microsoft.AspNetCore.Identity;
using Moq;
using MSP.Application.Abstracts;
using MSP.Application.Models.Requests.Project;
using MSP.Application.Models.Responses.Project;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Project;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Services.Interfaces.Project;
using MSP.Domain.Entities;
using Xunit;

namespace MSP.Tests.Services.ProjectServicesTest
{
    public class CreateProjectTest
    {
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IProjectMemberRepository> _mockProjectMemberRepository;
        private readonly Mock<IProjectTaskRepository> _mockProjectTaskRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IMeetingRepository> _mockMeetingRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;

        private readonly IProjectService _projectService;

        public CreateProjectTest()
        {
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockProjectMemberRepository = new Mock<IProjectMemberRepository>();
            _mockProjectTaskRepository = new Mock<IProjectTaskRepository>();

            _mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null
            );

            _mockNotificationService = new Mock<INotificationService>();
            _mockMeetingRepository = new Mock<IMeetingRepository>();
            _mockUserRepository = new Mock<IUserRepository>();

            _projectService = new ProjectService(
                _mockProjectRepository.Object,
                _mockProjectMemberRepository.Object,
                _mockProjectTaskRepository.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );
        }

        // Helper: Create valid User with required fields
        private User CreateValidUser(Guid id, Guid? managedById = null)
        {
            return new User
            {
                Id = id,
                Email = "test@example.com",
                FullName = "Test User",
                ManagedById = managedById
            };
        }

        #region CreateProjectAsync Tests

        [Fact]
        public async Task CreateProjectAsync_WithValidRequest_ReturnsSuccessResponse()
        {
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var createRequest = new CreateProjectRequest
            {
                Name = "Test Project",
                Description = "Test project description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Active",
                CreatedById = userId
            };

            var user = CreateValidUser(userId, ownerId);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.AddAsync(It.IsAny<Project>()))
                .Callback<Project>(p => p.Id = projectId)
                .ReturnsAsync((Project p) => p);

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockProjectMemberRepository
                .Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .ReturnsAsync((ProjectMember pm) => pm);

            _mockProjectMemberRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var result = await _projectService.CreateProjectAsync(createRequest);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(createRequest.Name, result.Data.Name);
            Assert.Equal(ownerId, result.Data.OwnerId);
        }

        [Fact]
        public async Task CreateProjectAsync_WithInvalidUserId_ReturnsUserNotFoundError()
        {
            var userId = Guid.NewGuid();

            var createRequest = new CreateProjectRequest
            {
                Name = "Test Project",
                Description = "Test project description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Active",
                CreatedById = userId
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((User)null);

            var result = await _projectService.CreateProjectAsync(createRequest);

            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task CreateProjectAsync_WithUserWithoutBusinessOwner_ReturnsErrorResponse()
        {
            var userId = Guid.NewGuid();

            var createRequest = new CreateProjectRequest
            {
                Name = "Test Project",
                Description = "Test project description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Active",
                CreatedById = userId
            };

            var user = CreateValidUser(userId, null);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            var result = await _projectService.CreateProjectAsync(createRequest);

            Assert.False(result.Success);
            Assert.Equal("User does not have a Business Owner assigned", result.Message);
        }

        [Fact]
        public async Task CreateProjectAsync_WithNullDescription_ReturnsSuccessResponse()
        {
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var createRequest = new CreateProjectRequest
            {
                Name = "Test Project",
                Description = null,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Active",
                CreatedById = userId
            };

            var user = CreateValidUser(userId, ownerId);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.AddAsync(It.IsAny<Project>()))
                .Callback<Project>(p => p.Id = projectId)
                .ReturnsAsync((Project p) => p);

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockProjectMemberRepository
                .Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .ReturnsAsync((ProjectMember pm) => pm);

            _mockProjectMemberRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var result = await _projectService.CreateProjectAsync(createRequest);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Null(result.Data.Description);
        }

        [Fact]
        public async Task CreateProjectAsync_WithEmptyProjectName_ReturnsSuccessResponse()
        {
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var createRequest = new CreateProjectRequest
            {
                Name = string.Empty,
                Description = "Test project description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Active",
                CreatedById = userId
            };

            var user = CreateValidUser(userId, ownerId);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.AddAsync(It.IsAny<Project>()))
                .Callback<Project>(p => p.Id = projectId)
                .ReturnsAsync((Project p) => p);

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockProjectMemberRepository
                .Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .ReturnsAsync((ProjectMember pm) => pm);

            _mockProjectMemberRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var result = await _projectService.CreateProjectAsync(createRequest);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(string.Empty, result.Data.Name);
        }

        [Fact]
        public async Task CreateProjectAsync_WithInvalidDateRange_ReturnsSuccessResponse()
        {
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var startDate = DateTime.UtcNow.AddMonths(1);
            var endDate = DateTime.UtcNow;

            var createRequest = new CreateProjectRequest
            {
                Name = "Test Project",
                Description = "Test project description",
                StartDate = startDate,
                EndDate = endDate,
                Status = "Active",
                CreatedById = userId
            };

            var user = CreateValidUser(userId, ownerId);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.AddAsync(It.IsAny<Project>()))
                .Callback<Project>(p => p.Id = projectId)
                .ReturnsAsync((Project p) => p);

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockProjectMemberRepository
                .Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .ReturnsAsync((ProjectMember pm) => pm);

            _mockProjectMemberRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var result = await _projectService.CreateProjectAsync(createRequest);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(startDate, result.Data.StartDate);
            Assert.Equal(endDate, result.Data.EndDate);
        }

        [Fact]
        public async Task CreateProjectAsync_WithProjectRepositoryFailure_ThrowsException()
        {
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var createRequest = new CreateProjectRequest
            {
                Name = "Test Project",
                Description = "Test project description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Active",
                CreatedById = userId
            };

            var user = CreateValidUser(userId, ownerId);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.AddAsync(It.IsAny<Project>()))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _projectService.CreateProjectAsync(createRequest));
        }

        [Fact]
        public async Task CreateProjectAsync_WithProjectMemberRepositoryFailure_ThrowsException()
        {
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var createRequest = new CreateProjectRequest
            {
                Name = "Test Project",
                Description = "Test project description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Active",
                CreatedById = userId
            };

            var user = CreateValidUser(userId, ownerId);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.AddAsync(It.IsAny<Project>()))
                .Callback<Project>(p => p.Id = projectId)
                .ReturnsAsync((Project p) => p);

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockProjectMemberRepository
                .Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _projectService.CreateProjectAsync(createRequest));
        }

        [Fact]
        public async Task CreateProjectAsync_VerifyProjectMemberCreatedWithCorrectData()
        {
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            ProjectMember capturedProjectMember = null;

            var createRequest = new CreateProjectRequest
            {
                Name = "Test Project",
                Description = "Test project description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Active",
                CreatedById = userId
            };

            var user = CreateValidUser(userId, ownerId);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.AddAsync(It.IsAny<Project>()))
                .Callback<Project>(p => p.Id = projectId)
                .ReturnsAsync((Project p) => p);

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockProjectMemberRepository
                .Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .Callback<ProjectMember>(pm => capturedProjectMember = pm)
                .ReturnsAsync((ProjectMember pm) => pm);

            _mockProjectMemberRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var result = await _projectService.CreateProjectAsync(createRequest);

            Assert.True(result.Success);
            Assert.NotNull(capturedProjectMember);
            Assert.Equal(projectId, capturedProjectMember.ProjectId);
            Assert.Equal(userId, capturedProjectMember.MemberId);
        }

        [Fact]
        public async Task CreateProjectAsync_VerifyOwnerIdSetFromManagedById()
        {
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            Project capturedProject = null;

            var createRequest = new CreateProjectRequest
            {
                Name = "Test Project",
                Description = "Test project description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Active",
                CreatedById = userId
            };

            var user = CreateValidUser(userId, ownerId);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.AddAsync(It.IsAny<Project>()))
                .Callback<Project>(p => {
                    p.Id = projectId;
                    capturedProject = p;
                })
                .ReturnsAsync((Project p) => p);

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockProjectMemberRepository
                .Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .ReturnsAsync((ProjectMember pm) => pm);

            _mockProjectMemberRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var result = await _projectService.CreateProjectAsync(createRequest);

            Assert.True(result.Success);
            Assert.NotNull(capturedProject);
            Assert.Equal(ownerId, capturedProject.OwnerId);
        }

        #endregion
    }
}
