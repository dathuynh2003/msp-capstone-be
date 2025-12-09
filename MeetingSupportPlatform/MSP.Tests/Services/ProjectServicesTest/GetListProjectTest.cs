using Microsoft.AspNetCore.Identity;
using Moq;
using MSP.Application.Models.Responses.Auth;
using MSP.Application.Models.Responses.Project;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Project;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Domain.Entities;
using MSP.Shared.Common;
using System.Linq.Expressions;
using Xunit;

namespace MSP.Tests.Services.ProjectServicesTest
{
    public class GetListProjectTest
    {
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IProjectMemberRepository> _projectMemberRepositoryMock;
        private readonly Mock<IProjectTaskRepository> _projectTaskRepositoryMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly ProjectService _projectService;

        public GetListProjectTest()
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
        public async Task GetAllProjectsAsync_WithValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var request = new PagingRequest { PageIndex = 1, PageSize = 10 };
            var ownerId = Guid.NewGuid();
            var createdById = Guid.NewGuid();

            var owner = new User { Id = ownerId, FullName = "Owner User", Email = "owner@test.com" };
            var createdBy = new User { Id = createdById, FullName = "Creator User", Email = "creator@test.com" };

            var projects = new List<Domain.Entities.Project>
            {
                new Domain.Entities.Project
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Project 1",
                    Description = "Description 1",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(30),
                    OwnerId = ownerId,
                    CreatedById = createdById,
                    Status = "InProgress",
                    IsDeleted = false,
                    Owner = owner,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Domain.Entities.Project
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Project 2",
                    Description = "Description 2",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(60),
                    OwnerId = ownerId,
                    CreatedById = createdById,
                    Status = "InProgress",
                    IsDeleted = false,
                    Owner = owner,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _projectRepositoryMock.Setup(x => x.FindWithIncludePagedAsync(
                It.IsAny<Expression<Func<Domain.Entities.Project, bool>>>(),
                It.IsAny<Func<IQueryable<Domain.Entities.Project>, IQueryable<Domain.Entities.Project>>>(),
                It.IsAny<Func<IQueryable<Domain.Entities.Project>, IOrderedQueryable<Domain.Entities.Project>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>()
            )).ReturnsAsync(projects);

            _projectRepositoryMock.Setup(x => x.CountAsync(It.IsAny<Expression<Func<Domain.Entities.Project, bool>>>()))
                .ReturnsAsync(2);

            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string> { "BusinessOwner" });

            // Act
            var result = await _projectService.GetAllProjectsAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Items.Count()); 
            Assert.Equal(2, result.Data.TotalItems);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WithNoProjects_ReturnsErrorResponse()
        {
            // Arrange
            var request = new PagingRequest { PageIndex = 1, PageSize = 10 };

            _projectRepositoryMock.Setup(x => x.FindWithIncludePagedAsync(
                It.IsAny<Expression<Func<Domain.Entities.Project, bool>>>(),
                It.IsAny<Func<IQueryable<Domain.Entities.Project>, IQueryable<Domain.Entities.Project>>>(),
                It.IsAny<Func<IQueryable<Domain.Entities.Project>, IOrderedQueryable<Domain.Entities.Project>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new List<Domain.Entities.Project>());

            // Act
            var result = await _projectService.GetAllProjectsAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("No projects found", result.Message);
        }

        [Fact]
        public async Task GetProjectsByBOIdAsync_WithValidBOId_ReturnsSuccessResponse()
        {
            // Arrange
            var request = new PagingRequest { PageIndex = 1, PageSize = 10 };
            var boId = Guid.NewGuid();
            var createdById = Guid.NewGuid();

            var owner = new User { Id = boId, FullName = "Business Owner", Email = "bo@test.com" };
            var createdBy = new User { Id = createdById, FullName = "Creator User", Email = "creator@test.com" };

            var projects = new List<Domain.Entities.Project>
            {
                new Domain.Entities.Project
                {
                    Id = Guid.NewGuid(),
                    Name = "BO Project 1",
                    Description = "Description 1",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(30),
                    OwnerId = boId,
                    CreatedById = createdById,
                    Status = "InProgress",
                    IsDeleted = false,
                    Owner = owner,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(boId.ToString()))
                .ReturnsAsync(owner);

            _projectRepositoryMock.Setup(x => x.FindWithIncludePagedAsync(                      
                It.IsAny<Expression<Func<Domain.Entities.Project, bool>>>(),
                It.IsAny<Func<IQueryable<Domain.Entities.Project>, IQueryable<Domain.Entities.Project>>>(),
                It.IsAny<Func<IQueryable<Domain.Entities.Project>, IOrderedQueryable<Domain.Entities.Project>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>()))
                .ReturnsAsync(projects);

            _projectRepositoryMock.Setup(x => x.CountAsync(It.IsAny<Expression<Func<Domain.Entities.Project, bool>>>()))
                .ReturnsAsync(1);

            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string> { "BusinessOwner" });

            // Act
            var result = await _projectService.GetProjectsByBOIdAsync(request, boId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.Items);
            Assert.Equal("BO Project 1", result.Data.Items.First().Name);
        }

        [Fact]
        public async Task GetProjectsByBOIdAsync_WithNonExistentBO_ReturnsErrorResponse()
        {
            // Arrange
            var request = new PagingRequest { PageIndex = 1, PageSize = 10 };
            var boId = Guid.NewGuid();

            _userManagerMock.Setup(x => x.FindByIdAsync(boId.ToString()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _projectService.GetProjectsByBOIdAsync(request, boId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task GetProjectsByBOIdAsync_WithNoProjectsForBO_ReturnsErrorResponse()
        {
            // Arrange
            var request = new PagingRequest { PageIndex = 1, PageSize = 10 };
            var boId = Guid.NewGuid();
            var owner = new User { Id = boId, FullName = "Business Owner", Email = "bo@test.com" };

            _userManagerMock.Setup(x => x.FindByIdAsync(boId.ToString()))
                .ReturnsAsync(owner);

            _projectRepositoryMock.Setup(x => x.FindWithIncludePagedAsync(
                It.IsAny<Expression<Func<Domain.Entities.Project, bool>>>(),
                It.IsAny<Func<IQueryable<Domain.Entities.Project>, IQueryable<Domain.Entities.Project>>>(),
                It.IsAny<Func<IQueryable<Domain.Entities.Project>, IOrderedQueryable<Domain.Entities.Project>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new List<Domain.Entities.Project>());

            // Act
            var result = await _projectService.GetProjectsByBOIdAsync(request, boId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("No projects found for the specified Business Owner", result.Message);
        }

        [Fact]
        public async Task GetProjectsByManagerIdAsync_WithValidManagerId_ReturnsSuccessResponse()
        {
            // Arrange
            var request = new PagingRequest { PageIndex = 1, PageSize = 10 };
            var managerId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var manager = new User { Id = managerId, FullName = "Project Manager", Email = "pm@test.com" };
            var owner = new User { Id = ownerId, FullName = "Owner User", Email = "owner@test.com" };

            var project = new Domain.Entities.Project
            {
                Id = Guid.NewGuid(),
                Name = "PM Project 1",
                Description = "Description 1",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                OwnerId = ownerId,
                CreatedById = managerId,
                Status = "InProgress",
                IsDeleted = false,
                Owner = owner,
                CreatedBy = manager,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var projectMembers = new List<ProjectMember>
            {
                new ProjectMember
                {
                    Id = Guid.NewGuid(),
                    ProjectId = project.Id,
                    MemberId = managerId,
                    JoinedAt = DateTime.UtcNow,
                    Project = project
                }
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(managerId.ToString()))
                .ReturnsAsync(manager);

            _projectMemberRepositoryMock.Setup(x => x.FindWithIncludePagedAsync(
                It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                It.IsAny<Func<IQueryable<ProjectMember>, IQueryable<ProjectMember>>>(),
                It.IsAny<Func<IQueryable<ProjectMember>, IOrderedQueryable<ProjectMember>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>()))
                .ReturnsAsync(projectMembers);

            _projectMemberRepositoryMock.Setup(x => x.CountAsync(It.IsAny<Expression<Func<ProjectMember, bool>>>()))
                .ReturnsAsync(1);

            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string> { "ProjectManager" });

            // Act
            var result = await _projectService.GetProjectsByManagerIdAsync(request, managerId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.Items);
            Assert.Equal("PM Project 1", result.Data.Items.First().Name);
        }

        [Fact]
        public async Task GetProjectsByManagerIdAsync_WithNonExistentManager_ReturnsErrorResponse()
        {
            // Arrange
            var request = new PagingRequest { PageIndex = 1, PageSize = 10 };
            var managerId = Guid.NewGuid();

            _userManagerMock.Setup(x => x.FindByIdAsync(managerId.ToString()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _projectService.GetProjectsByManagerIdAsync(request, managerId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task GetProjectsByManagerIdAsync_WithNoProjectsForManager_ReturnsErrorResponse()
        {
            // Arrange
            var request = new PagingRequest { PageIndex = 1, PageSize = 10 };
            var managerId = Guid.NewGuid();
            var manager = new User { Id = managerId, FullName = "Project Manager", Email = "pm@test.com" };

            _userManagerMock.Setup(x => x.FindByIdAsync(managerId.ToString()))
                .ReturnsAsync(manager);

            _projectMemberRepositoryMock.Setup(x => x.FindWithIncludePagedAsync(
                It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                It.IsAny<Func<IQueryable<ProjectMember>, IQueryable<ProjectMember>>>(),
                It.IsAny<Func<IQueryable<ProjectMember>, IOrderedQueryable<ProjectMember>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new List<ProjectMember>());

            // Act
            var result = await _projectService.GetProjectsByManagerIdAsync(request, managerId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("No projects found for the specified manager", result.Message);
        }

        [Fact]
        public async Task GetProjectsByMemberIdAsync_WithValidMemberId_ReturnsSuccessResponse()
        {
            // Arrange
            var request = new PagingRequest { PageIndex = 1, PageSize = 10 };
            var memberId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var createdById = Guid.NewGuid();

            var member = new User { Id = memberId, FullName = "Member User", Email = "member@test.com" };
            var owner = new User { Id = ownerId, FullName = "Owner User", Email = "owner@test.com" };
            var createdBy = new User { Id = createdById, FullName = "Creator User", Email = "creator@test.com" };

            var project = new Domain.Entities.Project
            {
                Id = Guid.NewGuid(),
                Name = "Member Project 1",
                Description = "Description 1",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                OwnerId = ownerId,
                CreatedById = createdById,
                Status = "InProgress",
                IsDeleted = false,
                Owner = owner,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var projectMembers = new List<ProjectMember>
            {
                new ProjectMember
                {
                    Id = Guid.NewGuid(),
                    ProjectId = project.Id,
                    MemberId = memberId,
                    JoinedAt = DateTime.UtcNow,
                    Project = project
                }
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(memberId.ToString()))
                .ReturnsAsync(member);

            _projectMemberRepositoryMock.Setup(x => x.FindWithIncludePagedAsync(
                It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                It.IsAny<Func<IQueryable<ProjectMember>, IQueryable<ProjectMember>>>(),
                It.IsAny<Func<IQueryable<ProjectMember>, IOrderedQueryable<ProjectMember>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>()))
                .ReturnsAsync(projectMembers);

            _projectMemberRepositoryMock.Setup(x => x.CountAsync(It.IsAny<Expression<Func<ProjectMember, bool>>>()))
                .ReturnsAsync(1);

            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string> { "Member" });

            // Act
            var result = await _projectService.GetProjectsByMemberIdAsync(request, memberId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.Items);
            Assert.Equal("Member Project 1", result.Data.Items.First().Name);
        }

        [Fact]
        public async Task GetProjectsByMemberIdAsync_WithNonExistentMember_ReturnsErrorResponse()
        {
            // Arrange
            var request = new PagingRequest { PageIndex = 1, PageSize = 10 };
            var memberId = Guid.NewGuid();

            _userManagerMock.Setup(x => x.FindByIdAsync(memberId.ToString()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _projectService.GetProjectsByMemberIdAsync(request, memberId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task GetProjectsByMemberIdAsync_WithNoProjectsForMember_ReturnsErrorResponse()
        {
            // Arrange
            var request = new PagingRequest { PageIndex = 1, PageSize = 10 };
            var memberId = Guid.NewGuid();
            var member = new User { Id = memberId, FullName = "Member User", Email = "member@test.com" };

            _userManagerMock.Setup(x => x.FindByIdAsync(memberId.ToString()))
                .ReturnsAsync(member);

            _projectMemberRepositoryMock.Setup(x => x.FindWithIncludePagedAsync(
                It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                It.IsAny<Func<IQueryable<ProjectMember>, IQueryable<ProjectMember>>>(),
                It.IsAny<Func<IQueryable<ProjectMember>, IOrderedQueryable<ProjectMember>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new List<ProjectMember>());

            // Act
            var result = await _projectService.GetProjectsByMemberIdAsync(request, memberId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("No projects found for the specified member", result.Message);
        }

        [Fact]
        public async Task GetProjectsByMemberIdAsync_WithDeletedProjects_FiltersOutDeletedProjects()
        {
            // Arrange
            var request = new PagingRequest { PageIndex = 1, PageSize = 10 };
            var memberId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var createdById = Guid.NewGuid();

            var member = new User { Id = memberId, FullName = "Member User", Email = "member@test.com" };
            var owner = new User { Id = ownerId, FullName = "Owner User", Email = "owner@test.com" };
            var createdBy = new User { Id = createdById, FullName = "Creator User", Email = "creator@test.com" };

            var InProgressProject = new Domain.Entities.Project
            {
                Id = Guid.NewGuid(),
                Name = "InProgress Project",
                Description = "InProgress",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                OwnerId = ownerId,
                CreatedById = createdById,
                Status = "InProgress",
                IsDeleted = false,
                Owner = owner,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var deletedProject = new Domain.Entities.Project
            {
                Id = Guid.NewGuid(),
                Name = "Deleted Project",
                Description = "Deleted",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                OwnerId = ownerId,
                CreatedById = createdById,
                Status = "Deleted",
                IsDeleted = true,
                Owner = owner,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var projectMembers = new List<ProjectMember>
            {
                new ProjectMember
                {
                    Id = Guid.NewGuid(),
                    ProjectId = InProgressProject.Id,
                    MemberId = memberId,
                    JoinedAt = DateTime.UtcNow,
                    Project = InProgressProject
                },
                new ProjectMember
                {
                    Id = Guid.NewGuid(),
                    ProjectId = deletedProject.Id,
                    MemberId = memberId,
                    JoinedAt = DateTime.UtcNow,
                    Project = deletedProject
                }
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(memberId.ToString()))
                .ReturnsAsync(member);

            _projectMemberRepositoryMock.Setup(x => x.FindWithIncludePagedAsync(
                It.IsAny<Expression<Func<ProjectMember, bool>>>(),
                It.IsAny<Func<IQueryable<ProjectMember>, IQueryable<ProjectMember>>>(),
                It.IsAny<Func<IQueryable<ProjectMember>, IOrderedQueryable<ProjectMember>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>()))
                .ReturnsAsync(projectMembers);

            _projectMemberRepositoryMock.Setup(x => x.CountAsync(It.IsAny<Expression<Func<ProjectMember, bool>>>()))
                .ReturnsAsync(1);

            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string> { "Member" });

            // Act
            var result = await _projectService.GetProjectsByMemberIdAsync(request, memberId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.Items);
            Assert.Equal("InProgress Project", result.Data.Items.First().Name);
        }

        [Fact]
        public async Task GetAllProjectsAsync_VerifiesRoleRetrievalForAllUsers()
        {
            // Arrange
            var request = new PagingRequest { PageIndex = 1, PageSize = 10 };
            var ownerId = Guid.NewGuid();
            var createdById = Guid.NewGuid();

            var owner = new User { Id = ownerId, FullName = "Owner User", Email = "owner@test.com" };
            var createdBy = new User { Id = createdById, FullName = "Creator User", Email = "creator@test.com" };

            var projects = new List<Domain.Entities.Project>
            {
                new Domain.Entities.Project
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Project",
                    Description = "Description",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(30),
                    OwnerId = ownerId,
                    CreatedById = createdById,
                    Status = "InProgress",
                    IsDeleted = false,
                    Owner = owner,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _projectRepositoryMock.Setup(x => x.FindWithIncludePagedAsync(
                It.IsAny<Expression<Func<Project, bool>>>(),
                It.IsAny<Func<IQueryable<Project>, IQueryable<Project>>>(),
                It.IsAny<Func<IQueryable<Project>, IOrderedQueryable<Project>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>()
            )).ReturnsAsync(projects);

            _projectRepositoryMock.Setup(x => x.CountAsync(It.IsAny<Expression<Func<Domain.Entities.Project, bool>>>()))
                .ReturnsAsync(1);

            _userManagerMock.Setup(x => x.GetRolesAsync(owner))
                .ReturnsAsync(new List<string> { "BusinessOwner" });

            _userManagerMock.Setup(x => x.GetRolesAsync(createdBy))
                .ReturnsAsync(new List<string> { "ProjectManager" });

            // Act
            var result = await _projectService.GetAllProjectsAsync(request);

            // Assert
            Assert.True(result.Success);
            _userManagerMock.Verify(x => x.GetRolesAsync(owner), Times.Once);
            _userManagerMock.Verify(x => x.GetRolesAsync(createdBy), Times.Once);
            Assert.Equal("BusinessOwner", result.Data!.Items.First().Owner!.Role);
            Assert.Equal("ProjectManager", result.Data!.Items.First().CreatedBy!.Role);
        }
    }
}
