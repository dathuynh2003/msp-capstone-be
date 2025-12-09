using Microsoft.AspNetCore.Identity;
using Moq;
using MSP.Application.Models.Responses.Project;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Project;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Domain.Entities;
using MSP.Shared.Enums;
using Xunit;

namespace MSP.Tests.Services.ProjectServicesTest
{
    public class GetProjectDetailTest
    {
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<IProjectMemberRepository> _projectMemberRepositoryMock;
        private readonly Mock<IProjectTaskRepository> _projectTaskRepositoryMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly ProjectService _projectService;

        public GetProjectDetailTest()
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
        public async Task GetProjectDetail_AsProjectManager_ReturnsAllTasksSuccessfully()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();

            var user = new User { Id = userId, FullName = "Project Manager", Email = "pm@test.com" };
            var owner = new User { Id = ownerId, FullName = "Owner", Email = "owner@test.com" };
            var member = new User { Id = memberId, FullName = "Member", Email = "member@test.com" };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = "InProgress",
                Owner = owner,
                CreatedBy = user,
                ProjectMembers = new List<ProjectMember>
                {
                    new ProjectMember
                    {
                        Member = member,
                        JoinedAt = DateTime.UtcNow
                    }
                },
                ProjectTasks = new List<ProjectTask>
                {
                    new ProjectTask
                    {
                        Id = Guid.NewGuid(),
                        Title = "Task 1",
                        Description = "Task 1 Description",
                        Status = TaskEnum.InProgress.ToString(),
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddDays(10),
                        UserId = memberId,
                        User = member
                    },
                    new ProjectTask
                    {
                        Id = Guid.NewGuid(),
                        Title = "Task 2",
                        Description = "Task 2 Description",
                        Status = TaskEnum.Done.ToString(),
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddDays(5),
                        UserId = userId,
                        User = user
                    }
                }
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.ProjectManager.ToString() });

            _projectRepositoryMock.Setup(x => x.GetProjectDetailWithPMAsync(projectId, userId))
                .ReturnsAsync(project);

            // Act
            var result = await _projectService.GetProjectDetail(projectId, userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(projectId, result.Data.ProjectId);
            Assert.Equal("Test Project", result.Data.Name);
            Assert.Equal(2, result.Data.Tasks.Count); // PM sees all tasks
            Assert.Single(result.Data.Members);
        }

        [Fact]
        public async Task GetProjectDetail_AsMember_ReturnsOnlyAssignedTasks()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var pmId = Guid.NewGuid();

            var user = new User { Id = userId, FullName = "Member", Email = "member@test.com" };
            var owner = new User { Id = ownerId, FullName = "Owner", Email = "owner@test.com" };
            var pm = new User { Id = pmId, FullName = "Project Manager", Email = "pm@test.com" };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = "InProgress",
                Owner = owner,
                CreatedBy = pm,
                ProjectMembers = new List<ProjectMember>
                {
                    new ProjectMember
                    {
                        Member = user,
                        JoinedAt = DateTime.UtcNow
                    }
                },
                ProjectTasks = new List<ProjectTask>
                {
                    new ProjectTask
                    {
                        Id = Guid.NewGuid(),
                        Title = "My Task",
                        Description = "Task assigned to me",
                        Status = TaskEnum.InProgress.ToString(),
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddDays(10),
                        UserId = userId,
                        User = user
                    },
                    new ProjectTask
                    {
                        Id = Guid.NewGuid(),
                        Title = "Other Task",
                        Description = "Task assigned to PM",
                        Status = TaskEnum.Done.ToString(),
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddDays(5),
                        UserId = pmId,
                        User = pm
                    }
                }
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.Member.ToString() });

            _projectRepositoryMock.Setup(x => x.GetProjectDetailWithMemberAsync(projectId, userId))
                .ReturnsAsync(project);

            // Act
            var result = await _projectService.GetProjectDetail(projectId, userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(projectId, result.Data.ProjectId);
            Assert.Single(result.Data.Tasks); // Member only sees their tasks
            Assert.Equal("My Task", result.Data.Tasks.First().Title);
            Assert.Equal(userId, result.Data.Tasks.First().Assignee.UserId);
        }

        [Fact]
        public async Task GetProjectDetail_WithNonExistentProject_ReturnsErrorResponse()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, FullName = "User", Email = "user@test.com" };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.Member.ToString() });

            _projectRepositoryMock.Setup(x => x.GetProjectDetailWithMemberAsync(projectId, userId))
                .ReturnsAsync((Domain.Entities.Project)null);

            // Act
            var result = await _projectService.GetProjectDetail(projectId, userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Project not found", result.Message);
        }

        [Fact]
        public async Task GetProjectDetail_WithOverdueTasks_MarksThemCorrectly()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var user = new User { Id = userId, FullName = "Project Manager", Email = "pm@test.com" };
            var owner = new User { Id = ownerId, FullName = "Owner", Email = "owner@test.com" };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = "InProgress",
                Owner = owner,
                CreatedBy = user,
                ProjectMembers = new List<ProjectMember>(),
                ProjectTasks = new List<ProjectTask>
                {
                    new ProjectTask
                    {
                        Id = Guid.NewGuid(),
                        Title = "Overdue Task",
                        Description = "This task is overdue",
                        Status = TaskEnum.InProgress.ToString(),
                        StartDate = DateTime.UtcNow.AddDays(-20),
                        EndDate = DateTime.UtcNow.AddDays(-5), // Past end date
                        UserId = userId,
                        User = user
                    },
                    new ProjectTask
                    {
                        Id = Guid.NewGuid(),
                        Title = "Completed Overdue Task",
                        Description = "This task is done despite being past due",
                        Status = TaskEnum.Done.ToString(),
                        StartDate = DateTime.UtcNow.AddDays(-20),
                        EndDate = DateTime.UtcNow.AddDays(-5),
                        UserId = userId,
                        User = user
                    },
                    new ProjectTask
                    {
                        Id = Guid.NewGuid(),
                        Title = "On Time Task",
                        Description = "This task is on time",
                        Status = TaskEnum.InProgress.ToString(),
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddDays(10),
                        UserId = userId,
                        User = user
                    }
                }
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.ProjectManager.ToString() });

            _projectRepositoryMock.Setup(x => x.GetProjectDetailWithPMAsync(projectId, userId))
                .ReturnsAsync(project);

            // Act
            var result = await _projectService.GetProjectDetail(projectId, userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Tasks.Count);

            var overdueTasks = result.Data.Tasks.Where(t => t.IsOverdue).ToList();
            var notOverdueTasks = result.Data.Tasks.Where(t => !t.IsOverdue).ToList();

            Assert.Single(overdueTasks); // Only the InProgress overdue task
            Assert.Equal("Overdue Task", overdueTasks.First().Title);
            Assert.Equal(2, notOverdueTasks.Count); // Done task and on-time task
        }

        [Fact]
        public async Task GetProjectDetail_WithNoTasks_ReturnsEmptyTaskList()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var user = new User { Id = userId, FullName = "Project Manager", Email = "pm@test.com" };
            var owner = new User { Id = ownerId, FullName = "Owner", Email = "owner@test.com" };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = "InProgress",
                Owner = owner,
                CreatedBy = user,
                ProjectMembers = new List<ProjectMember>(),
                ProjectTasks = new List<ProjectTask>()
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.ProjectManager.ToString() });

            _projectRepositoryMock.Setup(x => x.GetProjectDetailWithPMAsync(projectId, userId))
                .ReturnsAsync(project);

            // Act
            var result = await _projectService.GetProjectDetail(projectId, userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.Tasks);
        }

        [Fact]
        public async Task GetProjectDetail_WithNoMembers_ReturnsEmptyMemberList()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var user = new User { Id = userId, FullName = "Project Manager", Email = "pm@test.com" };
            var owner = new User { Id = ownerId, FullName = "Owner", Email = "owner@test.com" };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = "InProgress",
                Owner = owner,
                CreatedBy = user,
                ProjectMembers = new List<ProjectMember>(),
                ProjectTasks = new List<ProjectTask>()
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.ProjectManager.ToString() });

            _projectRepositoryMock.Setup(x => x.GetProjectDetailWithPMAsync(projectId, userId))
                .ReturnsAsync(project);

            // Act
            var result = await _projectService.GetProjectDetail(projectId, userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.Members);
        }

        [Fact]
        public async Task GetProjectDetail_WithTaskWithoutAssignee_HandlesNullUser()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var user = new User { Id = userId, FullName = "Project Manager", Email = "pm@test.com" };
            var owner = new User { Id = ownerId, FullName = "Owner", Email = "owner@test.com" };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = "InProgress",
                Owner = owner,
                CreatedBy = user,
                ProjectMembers = new List<ProjectMember>(),
                ProjectTasks = new List<ProjectTask>
                {
                    new ProjectTask
                    {
                        Id = Guid.NewGuid(),
                        Title = "Unassigned Task",
                        Description = "Task without assignee",
                        Status = TaskEnum.Todo.ToString(),
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddDays(10),
                        UserId = null,
                        User = null
                    }
                }
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.ProjectManager.ToString() });

            _projectRepositoryMock.Setup(x => x.GetProjectDetailWithPMAsync(projectId, userId))
                .ReturnsAsync(project);

            // Act
            var result = await _projectService.GetProjectDetail(projectId, userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.Tasks);
            Assert.Null(result.Data.Tasks.First().Assignee);
        }

        [Fact]
        public async Task GetProjectDetail_WithMultipleMembers_ReturnsAllMembers()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var member1Id = Guid.NewGuid();
            var member2Id = Guid.NewGuid();
            var member3Id = Guid.NewGuid();

            var user = new User { Id = userId, FullName = "Project Manager", Email = "pm@test.com" };
            var owner = new User { Id = ownerId, FullName = "Owner", Email = "owner@test.com" };
            var member1 = new User { Id = member1Id, FullName = "Member 1", Email = "member1@test.com" };
            var member2 = new User { Id = member2Id, FullName = "Member 2", Email = "member2@test.com" };
            var member3 = new User { Id = member3Id, FullName = "Member 3", Email = "member3@test.com" };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = "InProgress",
                Owner = owner,
                CreatedBy = user,
                ProjectMembers = new List<ProjectMember>
                {
                    new ProjectMember { Member = member1, JoinedAt = DateTime.UtcNow },
                    new ProjectMember { Member = member2, JoinedAt = DateTime.UtcNow },
                    new ProjectMember { Member = member3, JoinedAt = DateTime.UtcNow }
                },
                ProjectTasks = new List<ProjectTask>()
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.ProjectManager.ToString() });

            _projectRepositoryMock.Setup(x => x.GetProjectDetailWithPMAsync(projectId, userId))
                .ReturnsAsync(project);

            // Act
            var result = await _projectService.GetProjectDetail(projectId, userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Members.Count);
            Assert.Contains(result.Data.Members, m => m.FullName == "Member 1");
            Assert.Contains(result.Data.Members, m => m.FullName == "Member 2");
            Assert.Contains(result.Data.Members, m => m.FullName == "Member 3");
        }

        [Fact]
        public async Task GetProjectDetail_AsProjectManager_UsesCorrectRepositoryMethod()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var user = new User { Id = userId, FullName = "Project Manager", Email = "pm@test.com" };
            var owner = new User { Id = ownerId, FullName = "Owner", Email = "owner@test.com" };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = "InProgress",
                Owner = owner,
                CreatedBy = user,
                ProjectMembers = new List<ProjectMember>(),
                ProjectTasks = new List<ProjectTask>()
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.ProjectManager.ToString() });

            _projectRepositoryMock.Setup(x => x.GetProjectDetailWithPMAsync(projectId, userId))
                .ReturnsAsync(project);

            // Act
            var result = await _projectService.GetProjectDetail(projectId, userId);

            // Assert
            Assert.True(result.Success);
            _projectRepositoryMock.Verify(x => x.GetProjectDetailWithPMAsync(projectId, userId), Times.Once);
            _projectRepositoryMock.Verify(x => x.GetProjectDetailWithMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetProjectDetail_AsMember_UsesCorrectRepositoryMethod()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var pmId = Guid.NewGuid();

            var user = new User { Id = userId, FullName = "Member", Email = "member@test.com" };
            var owner = new User { Id = ownerId, FullName = "Owner", Email = "owner@test.com" };
            var pm = new User { Id = pmId, FullName = "PM", Email = "pm@test.com" };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = "InProgress",
                Owner = owner,
                CreatedBy = pm,
                ProjectMembers = new List<ProjectMember>(),
                ProjectTasks = new List<ProjectTask>()
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.Member.ToString() });

            _projectRepositoryMock.Setup(x => x.GetProjectDetailWithMemberAsync(projectId, userId))
                .ReturnsAsync(project);

            // Act
            var result = await _projectService.GetProjectDetail(projectId, userId);

            // Assert
            Assert.True(result.Success);
            _projectRepositoryMock.Verify(x => x.GetProjectDetailWithMemberAsync(projectId, userId), Times.Once);
            _projectRepositoryMock.Verify(x => x.GetProjectDetailWithPMAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetProjectDetail_VerifiesCorrectResponseMapping()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var startDate = DateTime.UtcNow;
            var endDate = DateTime.UtcNow.AddDays(30);

            var user = new User 
            { 
                Id = userId, 
                FullName = "Project Manager", 
                Email = "pm@test.com",
                AvatarUrl = "pm-avatar.jpg"
            };
            var owner = new User 
            { 
                Id = ownerId, 
                FullName = "Owner", 
                Email = "owner@test.com",
                AvatarUrl = "owner-avatar.jpg"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                Description = "Test Description",
                StartDate = startDate,
                EndDate = endDate,
                Status = "InProgress",
                Owner = owner,
                CreatedBy = user,
                ProjectMembers = new List<ProjectMember>(),
                ProjectTasks = new List<ProjectTask>()
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.ProjectManager.ToString() });

            _projectRepositoryMock.Setup(x => x.GetProjectDetailWithPMAsync(projectId, userId))
                .ReturnsAsync(project);

            // Act
            var result = await _projectService.GetProjectDetail(projectId, userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(projectId, result.Data.ProjectId);
            Assert.Equal("Test Project", result.Data.Name);
            Assert.Equal("Test Description", result.Data.Description);
            Assert.Equal(startDate, result.Data.StartDate);
            Assert.Equal(endDate, result.Data.EndDate);
            Assert.Equal("InProgress", result.Data.Status);
            
            // Verify Owner mapping
            Assert.Equal(ownerId, result.Data.Owner.UserId);
            Assert.Equal("Owner", result.Data.Owner.FullName);
            Assert.Equal("owner@test.com", result.Data.Owner.Email);
            Assert.Equal("owner-avatar.jpg", result.Data.Owner.AvatarUrl);

            // Verify ProjectManager mapping
            Assert.Equal(userId, result.Data.ProjectManager.UserId);
            Assert.Equal("Project Manager", result.Data.ProjectManager.FullName);
            Assert.Equal("pm@test.com", result.Data.ProjectManager.Email);
            Assert.Equal("pm-avatar.jpg", result.Data.ProjectManager.AvatarUrl);
        }
    }
}
