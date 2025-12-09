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
    public class CreateMilestoneTest
    {
        private readonly Mock<IMilestoneRepository> _mockMilestoneRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IProjectTaskRepository> _mockProjectTaskRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly IMilestoneService _milestoneService;

        public CreateMilestoneTest()
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
        public async Task CreateMilestoneAsync_WithValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var milestoneId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FullName = "Test User"
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

            var createRequest = new CreateMilestoneRequest
            {
                UserId = userId,
                ProjectId = projectId,
                Name = "Milestone 1",
                DueDate = DateTime.UtcNow.AddMonths(1),
                Description = "Test milestone"
            };

            var milestone = new Milestone
            {
                Id = milestoneId,
                UserId = userId,
                ProjectId = projectId,
                Name = createRequest.Name,
                DueDate = createRequest.DueDate,
                Description = createRequest.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockMilestoneRepository.Setup(x => x.AddAsync(It.IsAny<Milestone>())).ReturnsAsync(milestone);
            _mockMilestoneRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _milestoneService.CreateMilestoneAsync(createRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Milestone 1", result.Data.Name);
            Assert.Equal(projectId, result.Data.ProjectId);
            Assert.Equal("Test milestone", result.Data.Description);
            Assert.Equal("Milestone created successfully", result.Message);

            _mockUserManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _mockProjectRepository.Verify(x => x.GetByIdAsync(projectId), Times.Once);
            _mockMilestoneRepository.Verify(x => x.AddAsync(It.IsAny<Milestone>()), Times.Once);
            _mockMilestoneRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateMilestoneAsync_WithNonExistentUser_ReturnsErrorResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var createRequest = new CreateMilestoneRequest
            {
                UserId = userId,
                ProjectId = projectId,
                Name = "Milestone 1",
                DueDate = DateTime.UtcNow.AddMonths(1),
                Description = "Test milestone"
            };

            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((User)null);

            // Act
            var result = await _milestoneService.CreateMilestoneAsync(createRequest);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal("User not found", result.Message);

            _mockUserManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _mockProjectRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task CreateMilestoneAsync_WithNonExistentProject_ReturnsErrorResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FullName = "Test User"
            };

            var createRequest = new CreateMilestoneRequest
            {
                UserId = userId,
                ProjectId = projectId,
                Name = "Milestone 1",
                DueDate = DateTime.UtcNow.AddMonths(1),
                Description = "Test milestone"
            };

            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync((Project)null);

            // Act
            var result = await _milestoneService.CreateMilestoneAsync(createRequest);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal("Project not found", result.Message);

            _mockUserManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _mockProjectRepository.Verify(x => x.GetByIdAsync(projectId), Times.Once);
        }

        [Fact]
        public async Task CreateMilestoneAsync_WithDeletedProject_ReturnsErrorResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FullName = "Test User"
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

            var createRequest = new CreateMilestoneRequest
            {
                UserId = userId,
                ProjectId = projectId,
                Name = "Milestone 1",
                DueDate = DateTime.UtcNow.AddMonths(1),
                Description = "Test milestone"
            };

            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(deletedProject);

            // Act
            var result = await _milestoneService.CreateMilestoneAsync(createRequest);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal("Project not found", result.Message);
        }

        [Fact]
        public async Task CreateMilestoneAsync_WithEmptyName_ReturnsSuccessResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var milestoneId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FullName = "Test User"
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

            var createRequest = new CreateMilestoneRequest
            {
                UserId = userId,
                ProjectId = projectId,
                Name = string.Empty,
                DueDate = DateTime.UtcNow.AddMonths(1),
                Description = "Test milestone"
            };

            var milestone = new Milestone
            {
                Id = milestoneId,
                UserId = userId,
                ProjectId = projectId,
                Name = createRequest.Name,
                DueDate = createRequest.DueDate,
                Description = createRequest.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockMilestoneRepository.Setup(x => x.AddAsync(It.IsAny<Milestone>())).ReturnsAsync(milestone);
            _mockMilestoneRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _milestoneService.CreateMilestoneAsync(createRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(string.Empty, result.Data.Name);
        }

        [Fact]
        public async Task CreateMilestoneAsync_WithNullDescription_ReturnsSuccessResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var milestoneId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FullName = "Test User"
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

            var createRequest = new CreateMilestoneRequest
            {
                UserId = userId,
                ProjectId = projectId,
                Name = "Milestone 1",
                DueDate = DateTime.UtcNow.AddMonths(1),
                Description = null
            };

            var milestone = new Milestone
            {
                Id = milestoneId,
                UserId = userId,
                ProjectId = projectId,
                Name = createRequest.Name,
                DueDate = createRequest.DueDate,
                Description = createRequest.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _mockProjectRepository.Setup(x => x.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockMilestoneRepository.Setup(x => x.AddAsync(It.IsAny<Milestone>())).ReturnsAsync(milestone);
            _mockMilestoneRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _milestoneService.CreateMilestoneAsync(createRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(string.Empty, result.Data.Description);
        }
    }
}
