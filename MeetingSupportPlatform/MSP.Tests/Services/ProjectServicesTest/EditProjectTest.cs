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
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var project = CreateValidProject(projectId, ownerId, "InProgress");

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Updated Project",
                Description = "Updated Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(2),
                Status = "InProgress"
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
            Assert.True(result.Success);
            Assert.Equal("Updated Project", result.Data.Name);
        }

        [Fact]
        public async Task UpdateProjectAsync_WithNonExistentProject_ReturnsErrorResponse()
        {
            var projectId = Guid.NewGuid();

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Updated Project",
                Description = "Updated Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "InProgress"
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync((Project)null);

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Project not found", result.Message);
        }

        [Fact]
        public async Task UpdateProjectAsync_UpdatesProjectProperties()
        {
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var newName = "Updated Project Name";
            var project = CreateValidProject(projectId, ownerId, "InProgress");

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = newName,
                Description = "Updated Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(2),
                Status = "InProgress"
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
            Assert.True(result.Success);
            Assert.Equal(newName, result.Data.Name);
        }

        [Fact]
        public async Task UpdateProjectAsync_WithStatusChange_ReturnsSuccess()
        {
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var project = CreateValidProject(projectId, ownerId, "InProgress");

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Completed"
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

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(ApiResponse<NotificationResponse>.SuccessResponse(new NotificationResponse()));

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Completed", result.Data.Status);
        }

        [Fact]
        public async Task UpdateProjectAsync_StatusToCompleted_WithActiveTasks_CancelsTasks()
        {
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var project = CreateValidProject(projectId, ownerId, "InProgress");

            var activeTask = CreateValidTask(taskId, projectId, "InProgress", userId);

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Completed"
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectTaskRepository
                .Setup(x => x.GetTasksByProjectIdAsync(projectId))
                .ReturnsAsync(new List<ProjectTask> { activeTask });

            _mockProjectRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockProjectTaskRepository
                .Setup(x => x.UpdateAsync(It.IsAny<ProjectTask>()))
                .Returns(Task.CompletedTask);

            _mockProjectTaskRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(ApiResponse<NotificationResponse>.SuccessResponse(new NotificationResponse()));

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Completed", result.Data.Status);
            _mockProjectTaskRepository.Verify(x => x.UpdateAsync(It.IsAny<ProjectTask>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task UpdateProjectAsync_StatusToCancelled_WithActiveTasks_CancelsTasks()
        {
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var project = CreateValidProject(projectId, ownerId, "InProgress");

            var activeTask = CreateValidTask(taskId, projectId, "InProgress", userId);

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Cancelled"
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectTaskRepository
                .Setup(x => x.GetTasksByProjectIdAsync(projectId))
                .ReturnsAsync(new List<ProjectTask> { activeTask });

            _mockProjectRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockProjectTaskRepository
                .Setup(x => x.UpdateAsync(It.IsAny<ProjectTask>()))
                .Returns(Task.CompletedTask);

            _mockProjectTaskRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(ApiResponse<NotificationResponse>.SuccessResponse(new NotificationResponse()));

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Cancelled", result.Data.Status);
            _mockProjectTaskRepository.Verify(x => x.UpdateAsync(It.IsAny<ProjectTask>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task UpdateProjectAsync_StatusToCompleted_WithDoneTasks_DoesNotCancelDoneTasks()
        {
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var project = CreateValidProject(projectId, ownerId, "InProgress");

            var doneTask = CreateValidTask(taskId, projectId, "Done", userId);

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Completed"
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectTaskRepository
                .Setup(x => x.GetTasksByProjectIdAsync(projectId))
                .ReturnsAsync(new List<ProjectTask> { doneTask });

            _mockProjectRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(ApiResponse<NotificationResponse>.SuccessResponse(new NotificationResponse()));

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Completed", result.Data.Status);
            _mockProjectTaskRepository.Verify(x => x.UpdateAsync(It.IsAny<ProjectTask>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProjectAsync_StatusToCompleted_WithTaskAndReviewer_SendsNotificationsToAssignedAndReviewer()
        {
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var reviewerId = Guid.NewGuid();
            var project = CreateValidProject(projectId, ownerId, "InProgress");

            var activeTask = CreateValidTask(taskId, projectId, "InProgress", userId, reviewerId);

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Completed"
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectTaskRepository
                .Setup(x => x.GetTasksByProjectIdAsync(projectId))
                .ReturnsAsync(new List<ProjectTask> { activeTask });

            _mockProjectRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockProjectTaskRepository
                .Setup(x => x.UpdateAsync(It.IsAny<ProjectTask>()))
                .Returns(Task.CompletedTask);

            _mockProjectTaskRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ReturnsAsync(ApiResponse<NotificationResponse>.SuccessResponse(new NotificationResponse()));

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert
            Assert.True(result.Success);
            _mockNotificationService.Verify(
                x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task UpdateProjectAsync_WithRepositoryFailure_ThrowsException()
        {
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var project = CreateValidProject(projectId, ownerId, "InProgress");

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Updated Project",
                Description = "Updated Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(2),
                Status = "InProgress"
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectTaskRepository
                .Setup(x => x.GetTasksByProjectIdAsync(projectId))
                .ReturnsAsync(new List<ProjectTask>());

            _mockProjectRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _projectService.UpdateProjectAsync(updateRequest));
        }

        [Fact]
        public async Task UpdateProjectAsync_StatusChangeWithNotificationFailure_StillSucceeds()
        {
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var project = CreateValidProject(projectId, ownerId, "InProgress");

            var activeTask = CreateValidTask(taskId, projectId, "InProgress", userId);

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Completed"
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectTaskRepository
                .Setup(x => x.GetTasksByProjectIdAsync(projectId))
                .ReturnsAsync(new List<ProjectTask> { activeTask });

            _mockProjectRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockProjectTaskRepository
                .Setup(x => x.UpdateAsync(It.IsAny<ProjectTask>()))
                .Returns(Task.CompletedTask);

            _mockProjectTaskRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .ThrowsAsync(new Exception("Notification service error"));

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert - Should still succeed because exception is caught
            Assert.True(result.Success);
            Assert.Equal("Completed", result.Data.Status);
        }

        [Fact]
        public async Task UpdateProjectAsync_VerifyUpdatedAtTimestampSet()
        {
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var project = CreateValidProject(projectId, ownerId, "InProgress");
            Project capturedProject = null;

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Updated Project",
                Description = "Updated Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(2),
                Status = "InProgress"
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectTaskRepository
                .Setup(x => x.GetTasksByProjectIdAsync(projectId))
                .ReturnsAsync(new List<ProjectTask>());

            _mockProjectRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Callback<Project>(p => capturedProject = p)
                .Returns(Task.CompletedTask);

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(capturedProject);
            Assert.NotEqual(DateTime.MinValue, capturedProject.UpdatedAt);
        }

        [Fact]
        public async Task UpdateProjectAsync_NoStatusChange_DoesNotTriggerTaskCancellation()
        {
            var projectId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var project = CreateValidProject(projectId, ownerId, "InProgress");

            var activeTask = CreateValidTask(taskId, projectId, "InProgress", userId);

            var updateRequest = new UpdateProjectRequest
            {
                Id = projectId,
                Name = "Updated Project",
                Description = "Updated Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(2),
                Status = "InProgress"
            };

            _mockProjectRepository
                .Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockProjectTaskRepository
                .Setup(x => x.GetTasksByProjectIdAsync(projectId))
                .ReturnsAsync(new List<ProjectTask> { activeTask });

            _mockProjectRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            _mockProjectRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _projectService.UpdateProjectAsync(updateRequest);

            // Assert
            Assert.True(result.Success);
            _mockProjectTaskRepository.Verify(x => x.UpdateAsync(It.IsAny<ProjectTask>()), Times.Never);
        }

        #endregion
    }
}
