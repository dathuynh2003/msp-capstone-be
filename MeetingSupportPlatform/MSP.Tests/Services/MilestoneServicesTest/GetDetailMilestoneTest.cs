using Microsoft.AspNetCore.Identity;
using Moq;
using MSP.Application.Models.Responses.Milestone;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Milestone;
using MSP.Application.Services.Interfaces.Milestone;
using MSP.Domain.Entities;
using MSP.Shared.Common;
using Xunit;

namespace MSP.Tests.Services.MilestoneServicesTest
{
    public class GetDetailMilestoneTest
    {
        private readonly Mock<IMilestoneRepository> _mockMilestoneRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IProjectTaskRepository> _mockProjectTaskRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly IMilestoneService _milestoneService;

        public GetDetailMilestoneTest()
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
        public async Task GetMilestoneByIdAsync_WithValidMilestoneId_ReturnsSuccessResponse()
        {
            // Arrange
            var milestoneId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow.AddDays(-5);
            var updatedAt = DateTime.UtcNow.AddDays(-1);

            var milestone = new Milestone
            {
                Id = milestoneId,
                UserId = userId,
                ProjectId = projectId,
                Name = "Test Milestone",
                DueDate = DateTime.UtcNow.AddMonths(1),
                Description = "Test Description",
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                IsDeleted = false
            };

            _mockMilestoneRepository.Setup(x => x.GetMilestoneByIdAsync(milestoneId))
                .ReturnsAsync(milestone);

            // Act
            var result = await _milestoneService.GetMilestoneByIdAsync(milestoneId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(milestoneId, result.Data.Id);
            Assert.Equal("Test Milestone", result.Data.Name);
            Assert.Equal(projectId, result.Data.ProjectId);
            Assert.Equal("Test Description", result.Data.Description);
            Assert.Equal(createdAt, result.Data.CreatedAt);
            Assert.Equal(updatedAt, result.Data.UpdatedAt);
            Assert.Equal("Milestone retrieved successfully", result.Message);

            _mockMilestoneRepository.Verify(x => x.GetMilestoneByIdAsync(milestoneId), Times.Once);
        }

        [Fact]
        public async Task GetMilestoneByIdAsync_WithNonExistentMilestone_ReturnsErrorResponse()
        {
            // Arrange
            var milestoneId = Guid.NewGuid();

            _mockMilestoneRepository.Setup(x => x.GetMilestoneByIdAsync(milestoneId))
                .ReturnsAsync((Milestone)null);

            // Act
            var result = await _milestoneService.GetMilestoneByIdAsync(milestoneId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal("Milestone not found", result.Message);

            _mockMilestoneRepository.Verify(x => x.GetMilestoneByIdAsync(milestoneId), Times.Once);
        }

        [Fact]
        public async Task GetMilestoneByIdAsync_WithDeletedMilestone_ReturnsErrorResponse()
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
                Name = "Deleted Milestone",
                DueDate = DateTime.UtcNow.AddMonths(1),
                Description = "This milestone is deleted",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = true
            };

            _mockMilestoneRepository.Setup(x => x.GetMilestoneByIdAsync(milestoneId))
                .ReturnsAsync(deletedMilestone);

            // Act
            var result = await _milestoneService.GetMilestoneByIdAsync(milestoneId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal("Milestone not found", result.Message);
        }


    }
}
