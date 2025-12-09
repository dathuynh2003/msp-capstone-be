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
    public class GetListMilestoneTest
    {
        private readonly Mock<IMilestoneRepository> _mockMilestoneRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IProjectTaskRepository> _mockProjectTaskRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly IMilestoneService _milestoneService;

        public GetListMilestoneTest()
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
        public async Task GetMilestonesByProjectIdAsync_WithValidProjectId_ReturnsSuccessResponse()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ProjectId = projectId,
                    Name = "Milestone 1",
                    DueDate = DateTime.UtcNow.AddMonths(1),
                    Description = "First milestone",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Milestone
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ProjectId = projectId,
                    Name = "Milestone 2",
                    DueDate = DateTime.UtcNow.AddMonths(2),
                    Description = "Second milestone",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            };

            _mockMilestoneRepository.Setup(x => x.GetMilestonesByProjectIdAsync(projectId))
                .ReturnsAsync(milestones);

            // Act
            var result = await _milestoneService.GetMilestonesByProjectIdAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal("Milestone 1", result.Data[0].Name);
            Assert.Equal("Milestone 2", result.Data[1].Name);
            Assert.Equal("Milestones retrieved successfully", result.Message);

            _mockMilestoneRepository.Verify(x => x.GetMilestonesByProjectIdAsync(projectId), Times.Once);
        }

        [Fact]
        public async Task GetMilestonesByProjectIdAsync_WithNoMilestones_ReturnsErrorResponse()
        {
            // Arrange
            var projectId = Guid.NewGuid();

            _mockMilestoneRepository.Setup(x => x.GetMilestonesByProjectIdAsync(projectId))
                .ReturnsAsync((List<Milestone>)null);

            // Act
            var result = await _milestoneService.GetMilestonesByProjectIdAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal("No milestones found for the project", result.Message);

            _mockMilestoneRepository.Verify(x => x.GetMilestonesByProjectIdAsync(projectId), Times.Once);
        }

        [Fact]
        public async Task GetMilestonesByProjectIdAsync_WithEmptyMilestoneList_ReturnsErrorResponse()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var emptyList = new List<Milestone>();

            _mockMilestoneRepository.Setup(x => x.GetMilestonesByProjectIdAsync(projectId))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _milestoneService.GetMilestonesByProjectIdAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal("No milestones found for the project", result.Message);

            _mockMilestoneRepository.Verify(x => x.GetMilestonesByProjectIdAsync(projectId), Times.Once);
        }

        [Fact]
        public async Task GetMilestonesByProjectIdAsync_WithSingleMilestone_ReturnsSuccessResponse()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var milestoneId = Guid.NewGuid();

            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Id = milestoneId,
                    UserId = userId,
                    ProjectId = projectId,
                    Name = "Single Milestone",
                    DueDate = DateTime.UtcNow.AddMonths(1),
                    Description = "Only milestone",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            };

            _mockMilestoneRepository.Setup(x => x.GetMilestonesByProjectIdAsync(projectId))
                .ReturnsAsync(milestones);

            // Act
            var result = await _milestoneService.GetMilestonesByProjectIdAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal("Single Milestone", result.Data[0].Name);
            Assert.Equal(milestoneId, result.Data[0].Id);
        }

        [Fact]
        public async Task GetMilestonesByProjectIdAsync_WithMultipleMilestones_ReturnsMilestonesInOrder()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ProjectId = projectId,
                    Name = "Milestone A",
                    DueDate = DateTime.UtcNow.AddMonths(1),
                    Description = "First",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Milestone
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ProjectId = projectId,
                    Name = "Milestone B",
                    DueDate = DateTime.UtcNow.AddMonths(2),
                    Description = "Second",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Milestone
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ProjectId = projectId,
                    Name = "Milestone C",
                    DueDate = DateTime.UtcNow.AddMonths(3),
                    Description = "Third",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            };

            _mockMilestoneRepository.Setup(x => x.GetMilestonesByProjectIdAsync(projectId))
                .ReturnsAsync(milestones);

            // Act
            var result = await _milestoneService.GetMilestonesByProjectIdAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count);
            Assert.Equal("Milestone A", result.Data[0].Name);
            Assert.Equal("Milestone B", result.Data[1].Name);
            Assert.Equal("Milestone C", result.Data[2].Name);
        }

        [Fact]
        public async Task GetMilestonesByProjectIdAsync_WithMilestonesWithNullDueDate_ReturnsSuccessResponse()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ProjectId = projectId,
                    Name = "Milestone Without DueDate",
                    DueDate = null,
                    Description = "No due date",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            };

            _mockMilestoneRepository.Setup(x => x.GetMilestonesByProjectIdAsync(projectId))
                .ReturnsAsync(milestones);

            // Act
            var result = await _milestoneService.GetMilestonesByProjectIdAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal(DateTime.MinValue, result.Data[0].DueDate);
        }

        [Fact]
        public async Task GetMilestonesByProjectIdAsync_WithMilestonesWithNullDescription_ReturnsSuccessResponse()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ProjectId = projectId,
                    Name = "Milestone Without Description",
                    DueDate = DateTime.UtcNow.AddMonths(1),
                    Description = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            };

            _mockMilestoneRepository.Setup(x => x.GetMilestonesByProjectIdAsync(projectId))
                .ReturnsAsync(milestones);

            // Act
            var result = await _milestoneService.GetMilestonesByProjectIdAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal(string.Empty, result.Data[0].Description);
        }
    }
}
