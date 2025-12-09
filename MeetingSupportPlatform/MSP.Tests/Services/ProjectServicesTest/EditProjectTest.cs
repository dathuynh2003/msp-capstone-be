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
using MSP.Shared.Enums;
using Xunit;

namespace MSP.Tests.Services.ProjectServicesTest
{
    public class EditProjectTest
    {
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IProjectMemberRepository> _mockProjectMemberRepository;
        private readonly Mock<IProjectTaskRepository> _mockProjectTaskRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<INotificationService> _mockNotificationService;

        private readonly IProjectService _projectService;

        public EditProjectTest()
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

        // Helper: Create valid User with required fields
        private User CreateValidUser(Guid id, string email = "test@example.com", string fullName = "Test User")
        {
            return new User
            {
                Id = id,
                Email = email,
                FullName = fullName
            };
        }

        // Helper: Create valid Project
        private Project CreateValidProject(Guid projectId, Guid ownerId, string status = "Active")
        {
            return new Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = status,
                OwnerId = ownerId,
                CreatedById = ownerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Owner = CreateValidUser(ownerId, "owner@example.com", "Project Owner"),
                ProjectMembers = new List<ProjectMember>()
            };
        }

        // Helper: Create valid ProjectTask
        private ProjectTask CreateValidTask(Guid taskId, Guid projectId, string status = "InProgress", Guid? userId = null, Guid? reviewerId = null)
        {
            return new ProjectTask
            {
                Id = taskId,
                ProjectId = projectId,
                Title = "Test Task",
                Description = "Test Task Description",
                Status = status,
                UserId = userId,
                ReviewerId = reviewerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        #region UpdateProjectAsync Tests

        [Fact]
        public async Task UpdateProjectAsync_WithValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var project = CreateValidProject(projectId, ownerId, "Active");

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Updated Project",
                Description = "Updated Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(2),
                Status = "Active"
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectTaskRepository
                .Setup(x => x.GetTasksByProjectIdAsync(projectId))
                .ReturnsAsync(new List<ProjectTask>());

            _mockProjectRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Updated Project", result.Data.Name);
            Assert.Equal("Updated Description", result.Data.Description);

            _mockProjectRepository.Verify(x => x.UpdateAsync(It.IsAny<Project>()), Times.Once);
            _mockProjectRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateProjectAsync_WithNonExistentProject_ReturnsErrorResponse()
        {
            // Arrange
            var projectId = Guid.NewGuid();

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Updated Project",
                Description = "Updated Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Active"
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync((Project)null);

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("Project not found", result.Message);

            _mockProjectRepository.Verify(x => x.UpdateAsync(It.IsAny<Project>()), Times.Never);
        }


        [Fact]
        public async Task UpdateProjectAsync_TaskWithAssignedUser_SendsNotificationToUser()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var project = CreateValidProject(projectId, ownerId, "Active");
            var task = CreateValidTask(taskId, projectId, "InProgress", userId);

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = ProjectStatusEnum.Completed.ToString()
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectTaskRepository
                .Setup(x => x.GetTasksByProjectIdAsync(projectId))
                .ReturnsAsync(new List<ProjectTask> { task });

            _mockProjectRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            _mockProjectTaskRepository
                .Setup(x => x.UpdateAsync(It.IsAny<ProjectTask>()))
                .Returns(Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(ApiResponse<NotificationResponse>.SuccessResponse(new NotificationResponse()));

            _mockProjectTaskRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            _mockNotificationService.Verify(
                x => x.CreateInAppNotificationAsync(It.Is<CreateNotificationRequest>(n => n.UserId == userId)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateProjectAsync_TaskWithReviewer_SendsNotificationToReviewer()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var reviewerId = Guid.NewGuid();

            var project = CreateValidProject(projectId, ownerId, "Active");
            var task = CreateValidTask(taskId, projectId, "InProgress", null, reviewerId);

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = ProjectStatusEnum.Completed.ToString()
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectTaskRepository
                .Setup(x => x.GetTasksByProjectIdAsync(projectId))
                .ReturnsAsync(new List<ProjectTask> { task });

            _mockProjectRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            _mockProjectTaskRepository
                .Setup(x => x.UpdateAsync(It.IsAny<ProjectTask>()))
                .Returns(Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(ApiResponse<NotificationResponse>.SuccessResponse(new NotificationResponse()));

            _mockProjectTaskRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            _mockNotificationService.Verify(
                x => x.CreateInAppNotificationAsync(It.Is<CreateNotificationRequest>(n => n.UserId == reviewerId)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateProjectAsync_DoesNotCancelDoneTasks()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            var project = CreateValidProject(projectId, ownerId, "Active");
            var task = CreateValidTask(taskId, projectId, TaskEnum.Done.ToString());

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = ProjectStatusEnum.Completed.ToString()
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectTaskRepository
                .Setup(x => x.GetTasksByProjectIdAsync(projectId))
                .ReturnsAsync(new List<ProjectTask> { task });

            _mockProjectRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            _mockProjectTaskRepository.Verify(x => x.UpdateAsync(It.IsAny<ProjectTask>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProjectAsync_DoesNotCancelAlreadyCancelledTasks()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            var project = CreateValidProject(projectId, ownerId, "Active");
            var task = CreateValidTask(taskId, projectId, TaskEnum.Cancelled.ToString());

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = ProjectStatusEnum.Completed.ToString()
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectTaskRepository
                .Setup(x => x.GetTasksByProjectIdAsync(projectId))
                .ReturnsAsync(new List<ProjectTask> { task });

            _mockProjectRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            _mockProjectTaskRepository.Verify(x => x.UpdateAsync(It.IsAny<ProjectTask>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProjectAsync_StatusChangedToCompleted_SendsNotificationToOwner()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var project = CreateValidProject(projectId, ownerId, "Active");

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = ProjectStatusEnum.Completed.ToString()
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectTaskRepository
                .Setup(x => x.GetTasksByProjectIdAsync(projectId))
                .ReturnsAsync(new List<ProjectTask>());

            _mockProjectRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(ApiResponse<NotificationResponse>.SuccessResponse(new NotificationResponse()));

            _mockNotificationService
                .Setup(x => x.SendEmailNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            _mockNotificationService.Verify(
                x => x.CreateInAppNotificationAsync(It.Is<CreateNotificationRequest>(n => n.UserId == ownerId)),
                Times.Once);

            _mockNotificationService.Verify(
                x => x.SendEmailNotification(
                    It.Is<string>(e => e == project.Owner.Email),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateProjectAsync_StatusChangedToCompleted_SendsNotificationToActiveMembers()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var memberId1 = Guid.NewGuid();
            var memberId2 = Guid.NewGuid();

            var member1 = CreateValidUser(memberId1, "member1@example.com", "Member 1");
            var member2 = CreateValidUser(memberId2, "member2@example.com", "Member 2");

            var projectMembers = new List<ProjectMember>
            {
                new ProjectMember { MemberId = memberId1, Member = member1, LeftAt = null },
                new ProjectMember { MemberId = memberId2, Member = member2, LeftAt = null }
            };

            var project = CreateValidProject(projectId, ownerId, "Active");
            project.ProjectMembers = projectMembers;

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = ProjectStatusEnum.Completed.ToString()
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectTaskRepository
                .Setup(x => x.GetTasksByProjectIdAsync(projectId))
                .ReturnsAsync(new List<ProjectTask>());

            _mockProjectRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(ApiResponse<NotificationResponse>.SuccessResponse(new NotificationResponse()));

            _mockNotificationService
                .Setup(x => x.SendEmailNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            // Verify notifications sent to both members (owner + 2 members = 3 total)
            _mockNotificationService.Verify(
                x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()),
                Times.Exactly(3));
        }

        [Fact]
        public async Task UpdateProjectAsync_StatusChangedToCancelled_SendsNotificationToOwner()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var project = CreateValidProject(projectId, ownerId, "Active");

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = ProjectStatusEnum.Cancelled.ToString()
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectTaskRepository
                .Setup(x => x.GetTasksByProjectIdAsync(projectId))
                .ReturnsAsync(new List<ProjectTask>());

            _mockProjectRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(ApiResponse<NotificationResponse>.SuccessResponse(new NotificationResponse()));

            _mockNotificationService
                .Setup(x => x.SendEmailNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            _mockNotificationService.Verify(
                x => x.CreateInAppNotificationAsync(It.Is<CreateNotificationRequest>(n => n.UserId == ownerId)),
                Times.Once);

            _mockNotificationService.Verify(
                x => x.SendEmailNotification(
                    It.Is<string>(e => e == project.Owner.Email),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateProjectAsync_NoStatusChange_DoesNotCancelTasks()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            var project = CreateValidProject(projectId, ownerId, "Active");
            var task = CreateValidTask(taskId, projectId, "InProgress");

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Updated Project",
                Description = "Updated Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Active"
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectTaskRepository
                .Setup(x => x.GetTasksByProjectIdAsync(projectId))
                .ReturnsAsync(new List<ProjectTask> { task });

            _mockProjectRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            _mockProjectTaskRepository.Verify(x => x.UpdateAsync(It.IsAny<ProjectTask>()), Times.Never);
            _mockNotificationService.Verify(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProjectAsync_ExceptionInTaskCancellation_DoesNotFailUpdate()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            var project = CreateValidProject(projectId, ownerId, "Active");
            var task = CreateValidTask(taskId, projectId, "InProgress");

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = ProjectStatusEnum.Completed.ToString()
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectTaskRepository
                .Setup(x => x.GetTasksByProjectIdAsync(projectId))
                .ReturnsAsync(new List<ProjectTask> { task });

            _mockProjectRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            _mockProjectTaskRepository
                .Setup(x => x.UpdateAsync(It.IsAny<ProjectTask>()))
                .ThrowsAsync(new Exception("Database error"));

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            _mockProjectRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateProjectAsync_UpdatesProjectProperties()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var newName = "New Project Name";
            var newDescription = "New Description";
            var newStartDate = DateTime.UtcNow.AddDays(1);
            var newEndDate = DateTime.UtcNow.AddMonths(3);

            var project = CreateValidProject(projectId, ownerId, "Active");

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = newName,
                Description = newDescription,
                StartDate = newStartDate,
                EndDate = newEndDate,
                Status = "Active"
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectTaskRepository
                .Setup(x => x.GetTasksByProjectIdAsync(projectId))
                .ReturnsAsync(new List<ProjectTask>());

            _mockProjectRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(newName, result.Data.Name);
            Assert.Equal(newDescription, result.Data.Description);
        }

        [Fact]
        public async Task UpdateProjectAsync_WithMultipleTasks_CancelsAllPendingTasks()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var task1Id = Guid.NewGuid();
            var task2Id = Guid.NewGuid();
            var task3Id = Guid.NewGuid();

            var project = CreateValidProject(projectId, ownerId, "Active");
            var task1 = CreateValidTask(task1Id, projectId, "InProgress");
            var task2 = CreateValidTask(task2Id, projectId, "InProgress");
            var task3 = CreateValidTask(task3Id, projectId, TaskEnum.Done.ToString());

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = ProjectStatusEnum.Completed.ToString()
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectTaskRepository
                .Setup(x => x.GetTasksByProjectIdAsync(projectId))
                .ReturnsAsync(new List<ProjectTask> { task1, task2, task3 });

            _mockProjectRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            _mockProjectTaskRepository
                .Setup(x => x.UpdateAsync(It.IsAny<ProjectTask>()))
                .Returns(Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(ApiResponse<NotificationResponse>.SuccessResponse(new NotificationResponse()));

            _mockProjectTaskRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            // Only 2 tasks should be updated (task1 and task2, not task3 which is Done)
            _mockProjectTaskRepository.Verify(x => x.UpdateAsync(It.IsAny<ProjectTask>()), Times.Exactly(2));
        }

        [Fact]
        public async Task UpdateProjectAsync_IgnoresInactiveMembers()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var activeMemberId = Guid.NewGuid();
            var inactiveMemberId = Guid.NewGuid();

            var activeMember = CreateValidUser(activeMemberId, "active@example.com", "Active Member");
            var inactiveMember = CreateValidUser(inactiveMemberId, "inactive@example.com", "Inactive Member");

            var projectMembers = new List<ProjectMember>
            {
                new ProjectMember { MemberId = activeMemberId, Member = activeMember, LeftAt = null },
                new ProjectMember { MemberId = inactiveMemberId, Member = inactiveMember, LeftAt = DateTime.UtcNow.AddDays(-1) }
            };

            var project = CreateValidProject(projectId, ownerId, "Active");
            project.ProjectMembers = projectMembers;

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = ProjectStatusEnum.Completed.ToString()
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectTaskRepository
                .Setup(x => x.GetTasksByProjectIdAsync(projectId))
                .ReturnsAsync(new List<ProjectTask>());

            _mockProjectRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(ApiResponse<NotificationResponse>.SuccessResponse(new NotificationResponse()));

            _mockNotificationService
                .Setup(x => x.SendEmailNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);

            // Only active member should receive notification (owner + 1 active member = 2)
            _mockNotificationService.Verify(
                x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()),
                Times.Exactly(2));

            // Inactive member should not receive email
            _mockNotificationService.Verify(
                x => x.SendEmailNotification(
                    It.Is<string>(e => e == inactiveMember.Email),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        #endregion
    }
}
