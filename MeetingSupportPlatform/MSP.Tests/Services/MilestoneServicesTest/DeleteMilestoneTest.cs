using Microsoft.AspNetCore.Identity;
using Moq;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Milestone;
using MSP.Application.Services.Interfaces.Milestone;
using MSP.Domain.Entities;
using MSP.Shared.Common;
using Xunit;

namespace MSP.Tests.Services.MilestoneServicesTest
{
    public class DeleteMilestoneTest
    {
        private readonly Mock<IMilestoneRepository> _mockMilestoneRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IProjectTaskRepository> _mockProjectTaskRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly IMilestoneService _milestoneService;

        public DeleteMilestoneTest()
        {
            _mockMilestoneRepository = new Mock<IMilestoneRepository>();
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockProjectTaskRepository = new Mock<IProjectTaskRepository>();
            _mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null
            );

            _milestoneService = new MilestoneService(
                _mockMilestoneRepository.Object,
                _mockProjectRepository.Object,
                _mockUserManager.Object,
                _mockProjectTaskRepository.Object
            );
        }

        [Fact]
        public async Task DeleteMilestoneAsync_WithValidMilestoneId_ReturnsSuccessResponse()
        {
            // Arrange
            var milestoneId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var milestone = new Milestone
            {
                Id = milestoneId,
                UserId = userId,
                ProjectId = projectId,
                Name = "Test Milestone",
                DueDate = DateTime.UtcNow.AddMonths(1),
                Description = "Test Description",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _mockProjectTaskRepository.Setup(x => x.GetTasksByMilestoneIdAsync(milestoneId))
                .ReturnsAsync((List<ProjectTask>)null);
            _mockMilestoneRepository.Setup(x => x.GetByIdAsync(milestoneId)).ReturnsAsync(milestone);
            _mockMilestoneRepository.Setup(x => x.SoftDeleteAsync(It.IsAny<Milestone>())).Returns(Task.CompletedTask);
            _mockMilestoneRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _milestoneService.DeleteMilestoneAsync(milestoneId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Null(result.Data);
            Assert.Equal("Milestone deleted successfully", result.Message);

            _mockProjectTaskRepository.Verify(x => x.GetTasksByMilestoneIdAsync(milestoneId), Times.Once);
            _mockMilestoneRepository.Verify(x => x.GetByIdAsync(milestoneId), Times.Once);
            _mockMilestoneRepository.Verify(x => x.SoftDeleteAsync(It.IsAny<Milestone>()), Times.Once);
            _mockMilestoneRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteMilestoneAsync_WithNonExistentMilestone_ReturnsErrorResponse()
        {
            // Arrange
            var milestoneId = Guid.NewGuid();

            _mockProjectTaskRepository.Setup(x => x.GetTasksByMilestoneIdAsync(milestoneId))
                .ReturnsAsync((List<ProjectTask>)null);
            _mockMilestoneRepository.Setup(x => x.GetByIdAsync(milestoneId)).ReturnsAsync((Milestone)null);

            // Act
            var result = await _milestoneService.DeleteMilestoneAsync(milestoneId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal("Milestone not found", result.Message);

            _mockProjectTaskRepository.Verify(x => x.GetTasksByMilestoneIdAsync(milestoneId), Times.Once);
            _mockMilestoneRepository.Verify(x => x.GetByIdAsync(milestoneId), Times.Once);
            _mockMilestoneRepository.Verify(x => x.SoftDeleteAsync(It.IsAny<Milestone>()), Times.Never);
        }

        [Fact]
        public async Task DeleteMilestoneAsync_WithAlreadyDeletedMilestone_ReturnsErrorResponse()
        {
            // Arrange
            var milestoneId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var deletedMilestone = new Milestone
            {
                Id = milestoneId,
                UserId = userId,
                ProjectId = projectId,
                Name = "Test Milestone",
                DueDate = DateTime.UtcNow.AddMonths(1),
                Description = "Test Description",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = true
            };

            _mockProjectTaskRepository.Setup(x => x.GetTasksByMilestoneIdAsync(milestoneId))
                .ReturnsAsync((List<ProjectTask>)null);
            _mockMilestoneRepository.Setup(x => x.GetByIdAsync(milestoneId)).ReturnsAsync(deletedMilestone);

            // Act
            var result = await _milestoneService.DeleteMilestoneAsync(milestoneId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal("Milestone not found", result.Message);

            _mockMilestoneRepository.Verify(x => x.SoftDeleteAsync(It.IsAny<Milestone>()), Times.Never);
        }

        [Fact]
        public async Task DeleteMilestoneAsync_WithRelatedTasks_RemovesTaskAssociationsAndDeletesMilestone()
        {
            // Arrange
            var milestoneId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var taskId1 = Guid.NewGuid();
            var taskId2 = Guid.NewGuid();

            var milestone = new Milestone
            {
                Id = milestoneId,
                UserId = userId,
                ProjectId = projectId,
                Name = "Test Milestone",
                DueDate = DateTime.UtcNow.AddMonths(1),
                Description = "Test Description",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var task1 = new ProjectTask
            {
                Id = taskId1,
                ProjectId = projectId,
                Title = "Task 1",
                Status = "Todo",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Milestones = new List<Milestone> { milestone }
            };

            var task2 = new ProjectTask
            {
                Id = taskId2,
                ProjectId = projectId,
                Title = "Task 2",
                Status = "Todo",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Milestones = new List<Milestone> { milestone }
            };

            var tasks = new List<ProjectTask> { task1, task2 };

            _mockProjectTaskRepository.Setup(x => x.GetTasksByMilestoneIdAsync(milestoneId))
                .ReturnsAsync(tasks);
            _mockProjectTaskRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockMilestoneRepository.Setup(x => x.GetByIdAsync(milestoneId)).ReturnsAsync(milestone);
            _mockMilestoneRepository.Setup(x => x.SoftDeleteAsync(It.IsAny<Milestone>())).Returns(Task.CompletedTask);
            _mockMilestoneRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _milestoneService.DeleteMilestoneAsync(milestoneId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Milestone deleted successfully", result.Message);

            _mockProjectTaskRepository.Verify(x => x.GetTasksByMilestoneIdAsync(milestoneId), Times.Once);
            _mockProjectTaskRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
            _mockMilestoneRepository.Verify(x => x.GetByIdAsync(milestoneId), Times.Once);
            _mockMilestoneRepository.Verify(x => x.SoftDeleteAsync(It.IsAny<Milestone>()), Times.Once);
            _mockMilestoneRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteMilestoneAsync_WithEmptyTaskList_DeletesMilestoneSuccessfully()
        {
            // Arrange
            var milestoneId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var milestone = new Milestone
            {
                Id = milestoneId,
                UserId = userId,
                ProjectId = projectId,
                Name = "Test Milestone",
                DueDate = DateTime.UtcNow.AddMonths(1),
                Description = "Test Description",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var emptyTaskList = new List<ProjectTask>();

            _mockProjectTaskRepository.Setup(x => x.GetTasksByMilestoneIdAsync(milestoneId))
                .ReturnsAsync(emptyTaskList);
            _mockMilestoneRepository.Setup(x => x.GetByIdAsync(milestoneId)).ReturnsAsync(milestone);
            _mockMilestoneRepository.Setup(x => x.SoftDeleteAsync(It.IsAny<Milestone>())).Returns(Task.CompletedTask);
            _mockMilestoneRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _milestoneService.DeleteMilestoneAsync(milestoneId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Milestone deleted successfully", result.Message);

            _mockProjectTaskRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
            _mockMilestoneRepository.Verify(x => x.SoftDeleteAsync(It.IsAny<Milestone>()), Times.Once);
        }

        [Fact]
        public async Task DeleteMilestoneAsync_WithTasksNotContainingMilestone_StillDeletesMilestone()
        {
            // Arrange
            var milestoneId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            var milestone = new Milestone
            {
                Id = milestoneId,
                UserId = userId,
                ProjectId = projectId,
                Name = "Test Milestone",
                DueDate = DateTime.UtcNow.AddMonths(1),
                Description = "Test Description",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var task = new ProjectTask
            {
                Id = taskId,
                ProjectId = projectId,
                Title = "Task 1",
                Status = "Todo",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Milestones = new List<Milestone>()
            };

            var tasks = new List<ProjectTask> { task };

            _mockProjectTaskRepository.Setup(x => x.GetTasksByMilestoneIdAsync(milestoneId))
                .ReturnsAsync(tasks);
            _mockProjectTaskRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockMilestoneRepository.Setup(x => x.GetByIdAsync(milestoneId)).ReturnsAsync(milestone);
            _mockMilestoneRepository.Setup(x => x.SoftDeleteAsync(It.IsAny<Milestone>())).Returns(Task.CompletedTask);
            _mockMilestoneRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _milestoneService.DeleteMilestoneAsync(milestoneId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Milestone deleted successfully", result.Message);

            _mockMilestoneRepository.Verify(x => x.SoftDeleteAsync(It.IsAny<Milestone>()), Times.Once);
        }

        [Fact]
        public async Task DeleteMilestoneAsync_WithRepositoryFailure_ThrowsException()
        {
            // Arrange
            var milestoneId = Guid.NewGuid();

            _mockProjectTaskRepository.Setup(x => x.GetTasksByMilestoneIdAsync(milestoneId))
                .ReturnsAsync((List<ProjectTask>)null);
            _mockMilestoneRepository.Setup(x => x.GetByIdAsync(milestoneId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _milestoneService.DeleteMilestoneAsync(milestoneId));
        }


    }
}
