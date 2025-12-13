using Microsoft.AspNetCore.Identity;
using Moq;
using MSP.Application.Models.Requests.Notification;
using MSP.Application.Models.Requests.Project;
using MSP.Application.Models.Responses.Notification;
using MSP.Application.Models.Responses.Project;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Project;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Services.Interfaces.Project;
using MSP.Domain.Entities;
using MSP.Shared.Common;
using Xunit;

namespace MSP.Tests.Services.ProjectServicesTest
{
    public class AddMemberTest
    {
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IProjectMemberRepository> _mockProjectMemberRepository;
        private readonly Mock<IProjectTaskRepository> _mockProjectTaskRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<INotificationService> _mockNotificationService;

        private readonly IProjectService _projectService;

        public AddMemberTest()
        {
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockProjectMemberRepository = new Mock<IProjectMemberRepository>();
            _mockProjectTaskRepository = new Mock<IProjectTaskRepository>();

            _mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null
            );

            _mockNotificationService = new Mock<INotificationService>();

            _projectService = new ProjectService(
                _mockProjectRepository.Object,
                _mockProjectMemberRepository.Object,
                _mockProjectTaskRepository.Object,
                _mockNotificationService.Object,
                _mockUserManager.Object
            );
        }

        // Helper: Create valid User
        private User CreateValidUser(Guid id)
        {
            return new User
            {
                Id = id,
                Email = "user@example.com",
                FullName = "Test User"
            };
        }

        // Helper: Create valid Project
        private Project CreateValidProject(Guid id)
        {
            return new Project
            {
                Id = id,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Active",
                CreatedById = Guid.NewGuid(),
                OwnerId = Guid.NewGuid()
            };
        }

        #region AddProjectMemberAsync Tests

        [Fact]
        public async Task AddProjectMemberAsync_WithValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var projectMemberId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = userId,
                ProjectId = projectId
            };

            var user = CreateValidUser(userId);
            var project = CreateValidProject(projectId);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectMemberRepository
                .Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .Callback<ProjectMember>(pm => pm.Id = projectMemberId)
                .ReturnsAsync((ProjectMember pm) => pm);

            _mockProjectMemberRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(ApiResponse<NotificationResponse>.SuccessResponse(null));

            // Act
            var result = await _projectService.AddProjectMemberAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(projectId, result.Data.ProjectId);
            Assert.Equal(userId, result.Data.UserId);
        }

        [Fact]
        public async Task AddProjectMemberAsync_WithInvalidUserId_ReturnsUserNotFoundError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = userId,
                ProjectId = projectId
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _projectService.AddProjectMemberAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task AddProjectMemberAsync_WithInvalidProjectId_ReturnsProjectNotFoundError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = userId,
                ProjectId = projectId
            };

            var user = CreateValidUser(userId);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync((Project)null);

            // Act
            var result = await _projectService.AddProjectMemberAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Project not found", result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task AddProjectMemberAsync_WithValidRequest_CreatesProjectMemberWithCorrectData()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var projectMemberId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = userId,
                ProjectId = projectId
            };

            var user = CreateValidUser(userId);
            var project = CreateValidProject(projectId);

            ProjectMember capturedProjectMember = null;

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectMemberRepository
                .Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .Callback<ProjectMember>(pm =>
                {
                    pm.Id = projectMemberId;
                    capturedProjectMember = pm;
                })
                .ReturnsAsync((ProjectMember pm) => pm);

            _mockProjectMemberRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(ApiResponse<NotificationResponse>.SuccessResponse(null));

            // Act
            await _projectService.AddProjectMemberAsync(request);

            // Assert
            Assert.NotNull(capturedProjectMember);
            Assert.Equal(projectId, capturedProjectMember.ProjectId);
            Assert.Equal(userId, capturedProjectMember.MemberId);
            Assert.NotEqual(default(DateTime), capturedProjectMember.JoinedAt);
        }

        [Fact]
        public async Task AddProjectMemberAsync_WithRepositoryFailure_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = userId,
                ProjectId = projectId
            };

            var user = CreateValidUser(userId);
            var project = CreateValidProject(projectId);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectMemberRepository
                .Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _projectService.AddProjectMemberAsync(request));
        }

        [Fact]
        public async Task AddProjectMemberAsync_WithNotificationFailure_StillSucceeds()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var projectMemberId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = userId,
                ProjectId = projectId
            };

            var user = CreateValidUser(userId);
            var project = CreateValidProject(projectId);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectMemberRepository
                .Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .Callback<ProjectMember>(pm => pm.Id = projectMemberId)
                .ReturnsAsync((ProjectMember pm) => pm);

            _mockProjectMemberRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ThrowsAsync(new Exception("Notification service error"));

            // Act
            var result = await _projectService.AddProjectMemberAsync(request);

            // Assert - Should still succeed because exception is caught
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(projectId, result.Data.ProjectId);
            Assert.Equal(userId, result.Data.UserId);
        }

        [Fact]
        public async Task AddProjectMemberAsync_VerifyJoinedAtTimestampIsSet()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var projectMemberId = Guid.NewGuid();
            var beforeTime = DateTime.UtcNow;

            var request = new AddProjectMemeberRequest
            {
                UserId = userId,
                ProjectId = projectId
            };

            var user = CreateValidUser(userId);
            var project = CreateValidProject(projectId);

            ProjectMember capturedProjectMember = null;

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectMemberRepository
                .Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .Callback<ProjectMember>(pm =>
                {
                    pm.Id = projectMemberId;
                    capturedProjectMember = pm;
                })
                .ReturnsAsync((ProjectMember pm) => pm);

            _mockProjectMemberRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(ApiResponse<NotificationResponse>.SuccessResponse(null));

            // Act
            var result = await _projectService.AddProjectMemberAsync(request);
            var afterTime = DateTime.UtcNow;

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(capturedProjectMember);
            Assert.True(capturedProjectMember.JoinedAt >= beforeTime && capturedProjectMember.JoinedAt <= afterTime);
        }

        [Fact]
        public async Task AddProjectMemberAsync_VerifyNotificationIsSent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var projectMemberId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = userId,
                ProjectId = projectId
            };

            var user = CreateValidUser(userId);
            var project = CreateValidProject(projectId);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectMemberRepository
                .Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .Callback<ProjectMember>(pm => pm.Id = projectMemberId)
                .ReturnsAsync((ProjectMember pm) => pm);

            _mockProjectMemberRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(ApiResponse<NotificationResponse>.SuccessResponse(null));

            // Act
            var result = await _projectService.AddProjectMemberAsync(request);

            // Assert
            Assert.True(result.Success);
            _mockNotificationService.Verify(
                x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()),
                Times.Once);
        }

        [Fact]
        public async Task AddProjectMemberAsync_VerifyResponseContainsCorrectProjectMemberId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var projectMemberId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = userId,
                ProjectId = projectId
            };

            var user = CreateValidUser(userId);
            var project = CreateValidProject(projectId);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectMemberRepository
                .Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .Callback<ProjectMember>(pm => pm.Id = projectMemberId)
                .ReturnsAsync((ProjectMember pm) => pm);

            _mockProjectMemberRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(ApiResponse<NotificationResponse>.SuccessResponse(null));

            // Act
            var result = await _projectService.AddProjectMemberAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(projectMemberId, result.Data.Id);
        }

        #endregion
    }
}
