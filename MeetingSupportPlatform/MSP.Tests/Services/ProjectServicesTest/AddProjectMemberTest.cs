using Microsoft.AspNetCore.Identity;
using Moq;
using MSP.Application.Models.Requests.Notification;
using MSP.Application.Models.Requests.Project;
using MSP.Application.Models.Responses.Project;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Project;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Domain.Entities;
using Xunit;

namespace MSP.Tests.Services.ProjectServicesTest
{
    public class AddProjectMemberTest
    {
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IProjectMemberRepository> _projectMemberRepositoryMock;
        private readonly Mock<IProjectTaskRepository> _projectTaskRepositoryMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly ProjectService _projectService;

        public AddProjectMemberTest()
        {
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _projectMemberRepositoryMock = new Mock<IProjectMemberRepository>();
            _projectTaskRepositoryMock = new Mock<IProjectTaskRepository>();
            _notificationServiceMock = new Mock<INotificationService>();

            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _projectService = new ProjectService(
                _projectRepositoryMock.Object,
                _projectMemberRepositoryMock.Object,
                _projectTaskRepositoryMock.Object,
                _notificationServiceMock.Object,
                _userManagerMock.Object);
        }

        [Fact]
        public async Task AddProjectMemberAsync_WithValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = userId,
                ProjectId = projectId
            };

            var user = new User
            {
                Id = userId,
                FullName = "Test User",
                Email = "test@test.com",
                AvatarUrl = "avatar.jpg"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = "Active"
            };

            var projectMember = new ProjectMember
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                MemberId = userId,
                JoinedAt = DateTime.UtcNow
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _projectMemberRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .ReturnsAsync(projectMember);

            _projectMemberRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .Returns((Task<Shared.Common.ApiResponse<Application.Models.Responses.Notification.NotificationResponse>>)Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.SendEmailNotification(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Verifiable();

            // Act
            var result = await _projectService.AddProjectMemberAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            _projectMemberRepositoryMock.Verify(x => x.AddAsync(It.Is<ProjectMember>(pm =>
                pm.ProjectId == projectId &&
                pm.MemberId == userId)), Times.Once);
            _projectMemberRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddProjectMemberAsync_WithNonExistentUser_ReturnsErrorResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = userId,
                ProjectId = projectId
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _projectService.AddProjectMemberAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
            _projectMemberRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ProjectMember>()), Times.Never);
        }

        [Fact]
        public async Task AddProjectMemberAsync_WithNonExistentProject_ReturnsErrorResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = userId,
                ProjectId = projectId
            };

            var user = new User
            {
                Id = userId,
                FullName = "Test User",
                Email = "test@test.com"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync((Domain.Entities.Project)null);

            // Act
            var result = await _projectService.AddProjectMemberAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Project not found", result.Message);
            _projectMemberRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ProjectMember>()), Times.Never);
        }

        [Fact]
        public async Task AddProjectMemberAsync_VerifiesJoinedAtTimestamp()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var beforeTime = DateTime.UtcNow;

            var request = new AddProjectMemeberRequest
            {
                UserId = userId,
                ProjectId = projectId
            };

            var user = new User
            {
                Id = userId,
                FullName = "Test User",
                Email = "test@test.com"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description"
            };

            ProjectMember capturedMember = null;

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _projectMemberRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .Callback<ProjectMember>(pm => capturedMember = pm)
                .ReturnsAsync((ProjectMember pm) => pm);

            _projectMemberRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .Returns((Task<Shared.Common.ApiResponse<Application.Models.Responses.Notification.NotificationResponse>>)Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.SendEmailNotification(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Verifiable();

            // Act
            var result = await _projectService.AddProjectMemberAsync(request);
            var afterTime = DateTime.UtcNow;

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(capturedMember);
            Assert.True(capturedMember.JoinedAt >= beforeTime && capturedMember.JoinedAt <= afterTime);
        }

        [Fact]
        public async Task AddProjectMemberAsync_SendsNotificationToNewMember()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = userId,
                ProjectId = projectId
            };

            var user = new User
            {
                Id = userId,
                FullName = "Test User",
                Email = "test@test.com"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = "Active"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _projectMemberRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .ReturnsAsync(new ProjectMember { Id = Guid.NewGuid(), ProjectId = projectId, MemberId = userId });

            _projectMemberRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .Returns((Task<Shared.Common.ApiResponse<Application.Models.Responses.Notification.NotificationResponse>>)Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.SendEmailNotification(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Verifiable();

            // Act
            var result = await _projectService.AddProjectMemberAsync(request);

            // Assert
            Assert.True(result.Success);
            _notificationServiceMock.Verify(x => x.CreateInAppNotificationAsync(
                It.Is<CreateNotificationRequest>(n =>
                    n.UserId == userId &&
                    n.Title == "👥 Added to Project" &&
                    n.Type == "ProjectUpdate")), Times.Once);
        }

        [Fact]
        public async Task AddProjectMemberAsync_SendsEmailNotificationToNewMember()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = userId,
                ProjectId = projectId
            };

            var user = new User
            {
                Id = userId,
                FullName = "Test User",
                Email = "test@test.com"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = "Active"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _projectMemberRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .ReturnsAsync(new ProjectMember { Id = Guid.NewGuid(), ProjectId = projectId, MemberId = userId });

            _projectMemberRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .Returns((Task<Shared.Common.ApiResponse<Application.Models.Responses.Notification.NotificationResponse>>)Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.SendEmailNotification(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Verifiable();

            // Act
            var result = await _projectService.AddProjectMemberAsync(request);

            // Assert
            Assert.True(result.Success);
            _notificationServiceMock.Verify(x => x.SendEmailNotification(
                user.Email!,
                "Added to Project",
                It.Is<string>(body => body.Contains(project.Name) && body.Contains(user.FullName))), Times.Once);
        }

        [Fact]
        public async Task AddProjectMemberAsync_ContinuesOnNotificationFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = userId,
                ProjectId = projectId
            };

            var user = new User
            {
                Id = userId,
                FullName = "Test User",
                Email = "test@test.com"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = "Active"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _projectMemberRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .ReturnsAsync(new ProjectMember { Id = Guid.NewGuid(), ProjectId = projectId, MemberId = userId });

            _projectMemberRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ThrowsAsync(new Exception("Notification service error"));

            // Act
            var result = await _projectService.AddProjectMemberAsync(request);

            // Assert
            Assert.True(result.Success); // Should still succeed despite notification failure
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task AddProjectMemberAsync_WithProjectNullDescription_HandlesGracefully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = userId,
                ProjectId = projectId
            };

            var user = new User
            {
                Id = userId,
                FullName = "Test User",
                Email = "test@test.com"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = null, // Null description
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = "Active"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _projectMemberRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .ReturnsAsync(new ProjectMember { Id = Guid.NewGuid(), ProjectId = projectId, MemberId = userId });

            _projectMemberRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .Returns((Task<Shared.Common.ApiResponse<Application.Models.Responses.Notification.NotificationResponse>>)Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.SendEmailNotification(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Verifiable();

            // Act
            var result = await _projectService.AddProjectMemberAsync(request);

            // Assert
            Assert.True(result.Success);
            _notificationServiceMock.Verify(x => x.SendEmailNotification(
                user.Email!,
                "Added to Project",
                It.Is<string>(body => body.Contains("No description"))), Times.Once);
        }

        [Fact]
        public async Task AddProjectMemberAsync_VerifiesCorrectProjectMemberData()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = userId,
                ProjectId = projectId
            };

            var user = new User
            {
                Id = userId,
                FullName = "Test User",
                Email = "test@test.com"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description"
            };

            ProjectMember capturedMember = null;

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _projectMemberRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .Callback<ProjectMember>(pm => capturedMember = pm)
                .ReturnsAsync((ProjectMember pm) => pm);

            _projectMemberRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .Returns((Task<Shared.Common.ApiResponse<Application.Models.Responses.Notification.NotificationResponse>>)Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.SendEmailNotification(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Verifiable();

            // Act
            var result = await _projectService.AddProjectMemberAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(capturedMember);
            Assert.Equal(projectId, capturedMember.ProjectId);
            Assert.Equal(userId, capturedMember.MemberId);
            Assert.True(capturedMember.JoinedAt > DateTime.MinValue);
        }

        [Fact]
        public async Task AddProjectMemberAsync_VerifiesNotificationDataStructure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = userId,
                ProjectId = projectId
            };

            var user = new User
            {
                Id = userId,
                FullName = "Test User",
                Email = "test@test.com"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = "Active"
            };

            CreateNotificationRequest capturedNotification = null;

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _projectMemberRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .ReturnsAsync(new ProjectMember { Id = Guid.NewGuid(), ProjectId = projectId, MemberId = userId, JoinedAt = DateTime.UtcNow });

            _projectMemberRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .Callback<CreateNotificationRequest>(n => capturedNotification = n)
                .Returns((Task<Shared.Common.ApiResponse<Application.Models.Responses.Notification.NotificationResponse>>)Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.SendEmailNotification(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Verifiable();

            // Act
            var result = await _projectService.AddProjectMemberAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(capturedNotification);
            Assert.Equal(userId, capturedNotification.UserId);
            Assert.Equal("Added to Project", capturedNotification.Title);
            Assert.Contains(project.Name, capturedNotification.Message);
            Assert.Equal("ProjectUpdate", capturedNotification.Type);
            Assert.Equal(project.Id.ToString(), capturedNotification.EntityId);
            Assert.NotNull(capturedNotification.Data);
            Assert.Contains("MemberAdded", capturedNotification.Data);
        }
    }
}
