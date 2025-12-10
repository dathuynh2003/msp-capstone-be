using Microsoft.AspNetCore.Identity;
using Moq;
using MSP.Application.Models.Requests.Notification;
using MSP.Application.Models.Requests.Project;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Project;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Domain.Entities;
using Xunit;

namespace MSP.Tests.Services.ProjectServicesTest
{
    public class AssignPMTest
    {
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IProjectMemberRepository> _projectMemberRepositoryMock;
        private readonly Mock<IProjectTaskRepository> _projectTaskRepositoryMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly ProjectService _projectService;

        public AssignPMTest()
        {
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _projectMemberRepositoryMock = new Mock<IProjectMemberRepository>();
            _projectTaskRepositoryMock = new Mock<IProjectTaskRepository>();
            _notificationServiceMock = new Mock<INotificationService>();

            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _projectService = new ProjectService(
                _projectRepositoryMock.Object,
                _projectMemberRepositoryMock.Object,
                _projectTaskRepositoryMock.Object,
                _notificationServiceMock.Object,
                _userManagerMock.Object);
        }

        [Fact]
        public async Task AddProjectMemberAsync_BusinessOwnerAssignsProjectManager_ReturnsSuccessResponse()
        {
            // Arrange
            var projectManagerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var businessOwnerId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = projectManagerId,
                ProjectId = projectId
            };

            var projectManager = new User
            {
                Id = projectManagerId,
                FullName = "John Smith",
                Email = "pm@company.com",
                AvatarUrl = "pm-avatar.jpg"
            };

            var businessOwner = new User
            {
                Id = businessOwnerId,
                FullName = "Business Owner",
                Email = "bo@company.com"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "E-Commerce Platform",
                Description = "Build a new e-commerce platform",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(90),
                Status = "Active",
                OwnerId = businessOwnerId,
                Owner = businessOwner,
                CreatedById = businessOwnerId,
                CreatedBy = businessOwner
            };

            var projectMember = new ProjectMember
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                MemberId = projectManagerId,
                JoinedAt = DateTime.UtcNow
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(projectManagerId.ToString()))
                .ReturnsAsync(projectManager);

            _userManagerMock.Setup(x => x.GetRolesAsync(projectManager))
                .ReturnsAsync(new List<string> { "ProjectManager" });

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _projectMemberRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .ReturnsAsync(projectMember);

            _projectMemberRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(new Shared.Common.ApiResponse<Application.Models.Responses.Notification.NotificationResponse>(true, "Notification created", null, null));

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
            Assert.Equal("Member added to project successfully", result.Message);
            Assert.Equal(projectManagerId, result.Data.UserId);
            Assert.Equal(projectManager.FullName, result.Data.Member.FullName);
            
            _projectMemberRepositoryMock.Verify(x => x.AddAsync(It.Is<ProjectMember>(pm =>
                pm.ProjectId == projectId &&
                pm.MemberId == projectManagerId)), Times.Once);
            
            _projectMemberRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddProjectMemberAsync_WithNonExistentProjectManager_ReturnsErrorResponse()
        {
            // Arrange
            var projectManagerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = projectManagerId,
                ProjectId = projectId
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(projectManagerId.ToString()))
                .ReturnsAsync((User?)null);

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
            var projectManagerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = projectManagerId,
                ProjectId = projectId
            };

            var projectManager = new User
            {
                Id = projectManagerId,
                FullName = "Project Manager",
                Email = "pm@company.com"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(projectManagerId.ToString()))
                .ReturnsAsync(projectManager);

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync((Domain.Entities.Project?)null);

            // Act
            var result = await _projectService.AddProjectMemberAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Project not found", result.Message);
            _projectMemberRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ProjectMember>()), Times.Never);
        }

        [Fact]
        public async Task AddProjectMemberAsync_VerifiesProjectManagerIsAddedWithCorrectRole()
        {
            // Arrange
            var projectManagerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var businessOwnerId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = projectManagerId,
                ProjectId = projectId
            };

            var projectManager = new User
            {
                Id = projectManagerId,
                FullName = "Project Manager",
                Email = "pm@company.com"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                OwnerId = businessOwnerId,
                CreatedById = businessOwnerId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = "Active"
            };

            ProjectMember? capturedMember = null;

            _userManagerMock.Setup(x => x.FindByIdAsync(projectManagerId.ToString()))
                .ReturnsAsync(projectManager);

            _userManagerMock.Setup(x => x.GetRolesAsync(projectManager))
                .ReturnsAsync(new List<string> { "ProjectManager" });

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _projectMemberRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .Callback<ProjectMember>(pm => capturedMember = pm)
                .ReturnsAsync((ProjectMember pm) => pm);

            _projectMemberRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(new Shared.Common.ApiResponse<Application.Models.Responses.Notification.NotificationResponse>(true, "Notification created", null, null));

            _notificationServiceMock.Setup(x => x.SendEmailNotification(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Verifiable();

            // Act
            await _projectService.AddProjectMemberAsync(request);

            // Assert
            Assert.NotNull(capturedMember);
            Assert.Equal(projectId, capturedMember.ProjectId);
            Assert.Equal(projectManagerId, capturedMember.MemberId);
            
            // Verify the user has ProjectManager role
            _userManagerMock.Verify(x => x.GetRolesAsync(projectManager), Times.Once);
        }

        [Fact]
        public async Task AddProjectMemberAsync_VerifiesJoinedAtTimestamp()
        {
            // Arrange
            var projectManagerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var beforeTime = DateTime.UtcNow;

            var request = new AddProjectMemeberRequest
            {
                UserId = projectManagerId,
                ProjectId = projectId
            };

            var projectManager = new User
            {
                Id = projectManagerId,
                FullName = "Project Manager",
                Email = "pm@company.com"
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

            ProjectMember? capturedMember = null;

            _userManagerMock.Setup(x => x.FindByIdAsync(projectManagerId.ToString()))
                .ReturnsAsync(projectManager);

            _userManagerMock.Setup(x => x.GetRolesAsync(projectManager))
                .ReturnsAsync(new List<string> { "ProjectManager" });

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _projectMemberRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .Callback<ProjectMember>(pm => capturedMember = pm)
                .ReturnsAsync((ProjectMember pm) => pm);

            _projectMemberRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(new Shared.Common.ApiResponse<Application.Models.Responses.Notification.NotificationResponse>(true, "Notification created", null, null));

            _notificationServiceMock.Setup(x => x.SendEmailNotification(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Verifiable();

            // Act
            await _projectService.AddProjectMemberAsync(request);
            var afterTime = DateTime.UtcNow;

            // Assert
            Assert.NotNull(capturedMember);
            Assert.True(capturedMember.JoinedAt >= beforeTime && capturedMember.JoinedAt <= afterTime,
                $"JoinedAt {capturedMember.JoinedAt} should be between {beforeTime} and {afterTime}");
        }

        [Fact]
        public async Task AddProjectMemberAsync_SendsNotificationToProjectManager()
        {
            // Arrange
            var projectManagerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var businessOwnerId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = projectManagerId,
                ProjectId = projectId
            };

            var projectManager = new User
            {
                Id = projectManagerId,
                FullName = "Project Manager",
                Email = "pm@company.com"
            };

            var businessOwner = new User
            {
                Id = businessOwnerId,
                FullName = "Business Owner",
                Email = "bo@company.com"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Mobile App Development",
                Description = "Develop a mobile application for iOS and Android",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(60),
                Status = "Active",
                OwnerId = businessOwnerId,
                Owner = businessOwner,
                CreatedById = businessOwnerId,
                CreatedBy = businessOwner
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(projectManagerId.ToString()))
                .ReturnsAsync(projectManager);

            _userManagerMock.Setup(x => x.GetRolesAsync(projectManager))
                .ReturnsAsync(new List<string> { "ProjectManager" });

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _projectMemberRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .ReturnsAsync(new ProjectMember 
                { 
                    Id = Guid.NewGuid(), 
                    ProjectId = projectId, 
                    MemberId = projectManagerId,
                    JoinedAt = DateTime.UtcNow
                });

            _projectMemberRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(new Shared.Common.ApiResponse<Application.Models.Responses.Notification.NotificationResponse>(true, "Notification created", null, null));

            _notificationServiceMock.Setup(x => x.SendEmailNotification(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Verifiable();

            // Act
            var result = await _projectService.AddProjectMemberAsync(request);

            // Assert
            Assert.True(result.Success);
            
            // Verify in-app notification was sent to Project Manager
            _notificationServiceMock.Verify(x => x.CreateInAppNotificationAsync(
                It.Is<CreateNotificationRequest>(n =>
                    n.UserId == projectManagerId &&
                    n.Title == "👥 Added to Project" &&
                    n.Message.Contains(project.Name) &&
                    n.Type == "ProjectUpdate")), Times.Once);
        }

        [Fact]
        public async Task AddProjectMemberAsync_SendsEmailNotificationToProjectManager()
        {
            // Arrange
            var projectManagerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var businessOwnerId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = projectManagerId,
                ProjectId = projectId
            };

            var projectManager = new User
            {
                Id = projectManagerId,
                FullName = "Jane Doe",
                Email = "jane.doe@company.com"
            };

            var businessOwner = new User
            {
                Id = businessOwnerId,
                FullName = "Business Owner"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Website Redesign",
                Description = "Redesign company website",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(45),
                Status = "Planning",
                OwnerId = businessOwnerId,
                Owner = businessOwner,
                CreatedById = businessOwnerId,
                CreatedBy = businessOwner
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(projectManagerId.ToString()))
                .ReturnsAsync(projectManager);

            _userManagerMock.Setup(x => x.GetRolesAsync(projectManager))
                .ReturnsAsync(new List<string> { "ProjectManager" });

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _projectMemberRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .ReturnsAsync(new ProjectMember 
                { 
                    Id = Guid.NewGuid(), 
                    ProjectId = projectId, 
                    MemberId = projectManagerId,
                    JoinedAt = DateTime.UtcNow
                });

            _projectMemberRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(new Shared.Common.ApiResponse<Application.Models.Responses.Notification.NotificationResponse>(true, "Notification created", null, null));

            _notificationServiceMock.Setup(x => x.SendEmailNotification(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Verifiable();

            // Act
            var result = await _projectService.AddProjectMemberAsync(request);

            // Assert
            Assert.True(result.Success);
            
            // Verify email notification was sent with correct details
            _notificationServiceMock.Verify(x => x.SendEmailNotification(
                projectManager.Email!,
                "Added to Project",
                It.Is<string>(body => 
                    body.Contains(projectManager.FullName) &&
                    body.Contains(project.Name) &&
                    body.Contains(project.Description!) &&
                    body.Contains(project.Status))), Times.Once);
        }

        [Fact]
        public async Task AddProjectMemberAsync_ContinuesOperationWhenNotificationFails()
        {
            // Arrange
            var projectManagerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = projectManagerId,
                ProjectId = projectId
            };

            var projectManager = new User
            {
                Id = projectManagerId,
                FullName = "Project Manager",
                Email = "pm@company.com"
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

            _userManagerMock.Setup(x => x.FindByIdAsync(projectManagerId.ToString()))
                .ReturnsAsync(projectManager);

            _userManagerMock.Setup(x => x.GetRolesAsync(projectManager))
                .ReturnsAsync(new List<string> { "ProjectManager" });

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _projectMemberRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .ReturnsAsync(new ProjectMember 
                { 
                    Id = Guid.NewGuid(), 
                    ProjectId = projectId, 
                    MemberId = projectManagerId,
                    JoinedAt = DateTime.UtcNow
                });

            _projectMemberRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Setup notification service to throw exception
            _notificationServiceMock.Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ThrowsAsync(new Exception("Notification service unavailable"));

            // Act
            var result = await _projectService.AddProjectMemberAsync(request);

            // Assert
            // Operation should still succeed even if notification fails
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Member added to project successfully", result.Message);
            
            // Verify project member was still added
            _projectMemberRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ProjectMember>()), Times.Once);
            _projectMemberRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddProjectMemberAsync_WithProjectNullDescription_HandlesGracefully()
        {
            // Arrange
            var projectManagerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = projectManagerId,
                ProjectId = projectId
            };

            var projectManager = new User
            {
                Id = projectManagerId,
                FullName = "Project Manager",
                Email = "pm@company.com"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Project Without Description",
                Description = null, // Null description
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = "Active"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(projectManagerId.ToString()))
                .ReturnsAsync(projectManager);

            _userManagerMock.Setup(x => x.GetRolesAsync(projectManager))
                .ReturnsAsync(new List<string> { "ProjectManager" });

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _projectMemberRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .ReturnsAsync(new ProjectMember 
                { 
                    Id = Guid.NewGuid(), 
                    ProjectId = projectId, 
                    MemberId = projectManagerId,
                    JoinedAt = DateTime.UtcNow
                });

            _projectMemberRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(new Shared.Common.ApiResponse<Application.Models.Responses.Notification.NotificationResponse>(true, "Notification created", null, null));

            _notificationServiceMock.Setup(x => x.SendEmailNotification(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Verifiable();

            // Act
            var result = await _projectService.AddProjectMemberAsync(request);

            // Assert
            Assert.True(result.Success);
            
            // Verify email contains "No description" fallback text
            _notificationServiceMock.Verify(x => x.SendEmailNotification(
                projectManager.Email!,
                "Added to Project",
                It.Is<string>(body => body.Contains("No description"))), Times.Once);
        }

        [Fact]
        public async Task AddProjectMemberAsync_VerifiesProjectMemberDataCorrectness()
        {
            // Arrange
            var projectManagerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = projectManagerId,
                ProjectId = projectId
            };

            var projectManager = new User
            {
                Id = projectManagerId,
                FullName = "Alice Johnson",
                Email = "alice@company.com",
                AvatarUrl = "alice-avatar.jpg"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "API Development",
                Description = "Develop REST API",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = "InProgress"
            };

            ProjectMember? capturedMember = null;

            _userManagerMock.Setup(x => x.FindByIdAsync(projectManagerId.ToString()))
                .ReturnsAsync(projectManager);

            _userManagerMock.Setup(x => x.GetRolesAsync(projectManager))
                .ReturnsAsync(new List<string> { "ProjectManager" });

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _projectMemberRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .Callback<ProjectMember>(pm => capturedMember = pm)
                .ReturnsAsync((ProjectMember pm) => pm);

            _projectMemberRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(new Shared.Common.ApiResponse<Application.Models.Responses.Notification.NotificationResponse>(true, "Notification created", null, null));

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
            
            // Verify all ProjectMember fields are set correctly
            Assert.Equal(projectId, capturedMember.ProjectId);
            Assert.Equal(projectManagerId, capturedMember.MemberId);
            Assert.NotEqual(default(DateTime), capturedMember.JoinedAt);
            Assert.True(capturedMember.JoinedAt <= DateTime.UtcNow);
        }

        [Fact]
        public async Task AddProjectMemberAsync_VerifiesNotificationDataStructure()
        {
            // Arrange
            var projectManagerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var businessOwnerId = Guid.NewGuid();

            var request = new AddProjectMemeberRequest
            {
                UserId = projectManagerId,
                ProjectId = projectId
            };

            var projectManager = new User
            {
                Id = projectManagerId,
                FullName = "Project Manager",
                Email = "pm@company.com"
            };

            var businessOwner = new User
            {
                Id = businessOwnerId,
                FullName = "Business Owner"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Data Migration Project",
                Description = "Migrate data to new system",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = "Active",
                OwnerId = businessOwnerId,
                Owner = businessOwner,
                CreatedById = businessOwnerId,
                CreatedBy = businessOwner
            };

            CreateNotificationRequest? capturedNotification = null;

            _userManagerMock.Setup(x => x.FindByIdAsync(projectManagerId.ToString()))
                .ReturnsAsync(projectManager);

            _userManagerMock.Setup(x => x.GetRolesAsync(projectManager))
                .ReturnsAsync(new List<string> { "ProjectManager" });

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _projectMemberRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ProjectMember>()))
                .ReturnsAsync(new ProjectMember 
                { 
                    Id = Guid.NewGuid(), 
                    ProjectId = projectId, 
                    MemberId = projectManagerId, 
                    JoinedAt = DateTime.UtcNow
                });

            _projectMemberRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _notificationServiceMock.Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .Callback<CreateNotificationRequest>(n => capturedNotification = n)
                .ReturnsAsync(new Shared.Common.ApiResponse<Application.Models.Responses.Notification.NotificationResponse>(true, "Notification created", null, null));

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
            
            // Verify notification structure
            Assert.Equal(projectManagerId, capturedNotification.UserId);
            Assert.Equal("👥 Added to Project", capturedNotification.Title);
            Assert.Contains(project.Name, capturedNotification.Message);
            Assert.Contains("Welcome to the team", capturedNotification.Message);
            Assert.Equal("ProjectUpdate", capturedNotification.Type);
            Assert.Equal(project.Id.ToString(), capturedNotification.EntityId);
            Assert.NotNull(capturedNotification.Data);
            
            // Verify JSON data contains expected fields
            Assert.Contains("ProjectId", capturedNotification.Data);
            Assert.Contains("ProjectName", capturedNotification.Data);
            Assert.Contains("JoinedAt", capturedNotification.Data);
            Assert.Contains("MemberAdded", capturedNotification.Data);
        }
    }
}
