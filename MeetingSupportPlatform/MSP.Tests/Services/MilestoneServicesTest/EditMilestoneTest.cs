using Microsoft.AspNetCore.Identity;
using Moq;
using MSP.Application.Models.Requests.Milestone;
using MSP.Application.Models.Responses.Milestone;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Milestone;
using MSP.Application.Services.Interfaces.Milestone;
using MSP.Domain.Entities;
using MSP.Shared.Common;
using Xunit;

namespace MSP.Tests.Services.MilestoneServicesTest
{
    public class EditMilestoneTest
    {
        private readonly Mock<IMilestoneRepository> _mockMilestoneRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IProjectTaskRepository> _mockProjectTaskRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly IMilestoneService _milestoneService;

        public EditMilestoneTest()
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
        public async Task UpdateMilestoneAsync_WithValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var milestoneId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var existingMilestone = new Milestone
            {
                Id = milestoneId,
                UserId = userId,
                ProjectId = projectId,
                Name = "Old Name",
                DueDate = DateTime.UtcNow.AddMonths(1),
                Description = "Old Description",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                IsDeleted = false
            };

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Active",
                OwnerId = userId,
                CreatedById = userId,
                IsDeleted = false
            };

            var updateRequest = new UpdateMilestoneRequest
            {
                Id = milestoneId,
                Name = "Updated Name",
                DueDate = DateTime.UtcNow.AddMonths(2),
                Description = "Updated Description"
            };

            _mockMilestoneRepository.Setup(x => x.GetByIdAsync(milestoneId)).ReturnsAsync(existingMilestone);
            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockMilestoneRepository.Setup(x => x.UpdateAsync(It.IsAny<Milestone>())).Returns(Task.CompletedTask);
            _mockMilestoneRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _milestoneService.UpdateMilestoneAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Updated Name", result.Data.Name);
            Assert.Equal("Updated Description", result.Data.Description);
            Assert.Equal("Milestone updated successfully", result.Message);

            _mockMilestoneRepository.Verify(x => x.GetByIdAsync(milestoneId), Times.Once);
            _mockProjectRepository.Verify(x => x.GetByIdAsync(projectId), Times.Once);
            _mockMilestoneRepository.Verify(x => x.UpdateAsync(It.IsAny<Milestone>()), Times.Once);
            _mockMilestoneRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateMilestoneAsync_WithNonExistentMilestone_ReturnsErrorResponse()
        {
            // Arrange
            var milestoneId = Guid.NewGuid();

            var updateRequest = new UpdateMilestoneRequest
            {
                Id = milestoneId,
                Name = "Updated Name",
                DueDate = DateTime.UtcNow.AddMonths(2),
                Description = "Updated Description"
            };

            _mockMilestoneRepository.Setup(x => x.GetByIdAsync(milestoneId)).ReturnsAsync((Milestone)null);

            // Act
            var result = await _milestoneService.UpdateMilestoneAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal("Milestone not found", result.Message);

            _mockMilestoneRepository.Verify(x => x.GetByIdAsync(milestoneId), Times.Once);
            _mockProjectRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdateMilestoneAsync_WithDeletedMilestone_ReturnsErrorResponse()
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
                Name = "Old Name",
                DueDate = DateTime.UtcNow.AddMonths(1),
                Description = "Old Description",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                IsDeleted = true
            };

            var updateRequest = new UpdateMilestoneRequest
            {
                Id = milestoneId,
                Name = "Updated Name",
                DueDate = DateTime.UtcNow.AddMonths(2),
                Description = "Updated Description"
            };

            _mockMilestoneRepository.Setup(x => x.GetByIdAsync(milestoneId)).ReturnsAsync(deletedMilestone);

            // Act
            var result = await _milestoneService.UpdateMilestoneAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal("Milestone not found", result.Message);
        }

        [Fact]
        public async Task UpdateMilestoneAsync_WithDeletedProject_ReturnsErrorResponse()
        {
            // Arrange
            var milestoneId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var existingMilestone = new Milestone
            {
                Id = milestoneId,
                UserId = userId,
                ProjectId = projectId,
                Name = "Old Name",
                DueDate = DateTime.UtcNow.AddMonths(1),
                Description = "Old Description",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                IsDeleted = false
            };

            var deletedProject = new Project
            {
                Id = projectId,
                Name = "Deleted Project",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Active",
                OwnerId = userId,
                CreatedById = userId,
                IsDeleted = true
            };

            var updateRequest = new UpdateMilestoneRequest
            {
                Id = milestoneId,
                Name = "Updated Name",
                DueDate = DateTime.UtcNow.AddMonths(2),
                Description = "Updated Description"
            };

            _mockMilestoneRepository.Setup(x => x.GetByIdAsync(milestoneId)).ReturnsAsync(existingMilestone);
            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(deletedProject);

            // Act
            var result = await _milestoneService.UpdateMilestoneAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal("Project not found", result.Message);
        }

        [Fact]
        public async Task UpdateMilestoneAsync_WithOnlyNameChange_ReturnsSuccessResponse()
        {
            // Arrange
            var milestoneId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var originalDueDate = DateTime.UtcNow.AddMonths(1);
            var originalDescription = "Original Description";

            var existingMilestone = new Milestone
            {
                Id = milestoneId,
                UserId = userId,
                ProjectId = projectId,
                Name = "Old Name",
                DueDate = originalDueDate,
                Description = originalDescription,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                IsDeleted = false
            };

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Active",
                OwnerId = userId,
                CreatedById = userId,
                IsDeleted = false
            };

            var updateRequest = new UpdateMilestoneRequest
            {
                Id = milestoneId,
                Name = "New Name",
                DueDate = originalDueDate,
                Description = originalDescription
            };

            _mockMilestoneRepository.Setup(x => x.GetByIdAsync(milestoneId)).ReturnsAsync(existingMilestone);
            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockMilestoneRepository.Setup(x => x.UpdateAsync(It.IsAny<Milestone>())).Returns(Task.CompletedTask);
            _mockMilestoneRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _milestoneService.UpdateMilestoneAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("New Name", result.Data.Name);
        }

        [Fact]
        public async Task UpdateMilestoneAsync_WithEmptyName_ReturnsSuccessResponse()
        {
            // Arrange
            var milestoneId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var existingMilestone = new Milestone
            {
                Id = milestoneId,
                UserId = userId,
                ProjectId = projectId,
                Name = "Old Name",
                DueDate = DateTime.UtcNow.AddMonths(1),
                Description = "Old Description",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                IsDeleted = false
            };

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Active",
                OwnerId = userId,
                CreatedById = userId,
                IsDeleted = false
            };

            var updateRequest = new UpdateMilestoneRequest
            {
                Id = milestoneId,
                Name = string.Empty,
                DueDate = DateTime.UtcNow.AddMonths(2),
                Description = "Updated Description"
            };

            _mockMilestoneRepository.Setup(x => x.GetByIdAsync(milestoneId)).ReturnsAsync(existingMilestone);
            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockMilestoneRepository.Setup(x => x.UpdateAsync(It.IsAny<Milestone>())).Returns(Task.CompletedTask);
            _mockMilestoneRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _milestoneService.UpdateMilestoneAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task UpdateMilestoneAsync_WithNullDescription_ReturnsSuccessResponse()
        {
            // Arrange
            var milestoneId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var existingMilestone = new Milestone
            {
                Id = milestoneId,
                UserId = userId,
                ProjectId = projectId,
                Name = "Old Name",
                DueDate = DateTime.UtcNow.AddMonths(1),
                Description = "Old Description",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                IsDeleted = false
            };

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "Active",
                OwnerId = userId,
                CreatedById = userId,
                IsDeleted = false
            };

            var updateRequest = new UpdateMilestoneRequest
            {
                Id = milestoneId,
                Name = "Updated Name",
                DueDate = DateTime.UtcNow.AddMonths(2),
                Description = null
            };

            _mockMilestoneRepository.Setup(x => x.GetByIdAsync(milestoneId)).ReturnsAsync(existingMilestone);
            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockMilestoneRepository.Setup(x => x.UpdateAsync(It.IsAny<Milestone>())).Returns(Task.CompletedTask);
            _mockMilestoneRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _milestoneService.UpdateMilestoneAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }
    }
}
