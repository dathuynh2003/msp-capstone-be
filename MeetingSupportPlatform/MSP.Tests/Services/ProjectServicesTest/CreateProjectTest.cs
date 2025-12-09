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
        public async Task CreateProjectAsync_WithValidRequest_CreatesProjectWithCorrectData()
        {
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var createRequest = new CreateProjectRequest
            {
                Name = "Enterprise Project",
                Description = "Large scale enterprise project",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(3),
                Status = "Planning",
                CreatedById = userId
            };

            var user = CreateValidUser(userId, ownerId);

            Project capturedProject = null;

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.AddAsync(It.IsAny<Project>()))
                .Callback<Project>(p =>
                {
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

            await _projectService.CreateProjectAsync(createRequest);

            Assert.NotNull(capturedProject);
            Assert.Equal(createRequest.Name, capturedProject.Name);
            Assert.Equal(ownerId, capturedProject.OwnerId);
        }

        [Fact]
        public async Task CreateProjectAsync_WithValidRequest_CreatesProjectMemberForCreator()
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

            ProjectMember capturedProjectMember = null;

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.AddAsync(It.IsAny<Project>()))
                .Callback<Project>(p => p.Id = projectId)
                .ReturnsAsync((Project p) => p);

            _mockProjectMemberRepository
                .Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .Callback<ProjectMember>(pm => capturedProjectMember = pm)
                .ReturnsAsync((ProjectMember pm) => pm);

            _mockProjectMemberRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            await _projectService.CreateProjectAsync(createRequest);

            Assert.NotNull(capturedProjectMember);
            Assert.Equal(projectId, capturedProjectMember.ProjectId);
            Assert.Equal(userId, capturedProjectMember.MemberId);
        }

        #endregion
    }
}
