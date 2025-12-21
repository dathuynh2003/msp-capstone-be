using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using MSP.Application.Abstracts;
using MSP.Application.Models.Requests.User;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Users;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Services.Interfaces.Users;
using MSP.Domain.Entities;
using MSP.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MSP.Tests.Services.OrganizationServicesTest
{
    public class ReassignRoleTest
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IOrganizationInviteRepository> _mockOrganizationInviteRepository;
        private readonly Mock<IProjectMemberRepository> _mockProjectMemberRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<ISubscriptionRepository> _mockSubscriptionRepository;
        private readonly Mock<IPackageRepository> _mockPackageRepository;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        public ReassignRoleTest()
        {
            _mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null
            );
            _mockUserRepository = new Mock<IUserRepository>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockOrganizationInviteRepository = new Mock<IOrganizationInviteRepository>();
            _mockProjectMemberRepository = new Mock<IProjectMemberRepository>();
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockSubscriptionRepository = new Mock<ISubscriptionRepository>();
            _mockPackageRepository = new Mock<IPackageRepository>();

            // Add a mock for IProjectTaskRepository
            var mockProjectTaskRepository = new Mock<IProjectTaskRepository>();
            _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:ClientUrl"] = "http://localhost:3000"
            })
            .Build();
            _userService = new UserService(
                _mockUserManager.Object,
                _mockUserRepository.Object,
                _mockNotificationService.Object,
                _mockOrganizationInviteRepository.Object,
                _mockProjectMemberRepository.Object,
                _mockProjectRepository.Object,
                mockProjectTaskRepository.Object, // Pass the mock here
                _mockSubscriptionRepository.Object,
                _mockPackageRepository.Object,
                _configuration
            );
        }

        [Fact]
        public async Task ReAssignRoleAsync_WithValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var request = new ReAssignRoleRequest
            {
                BusinessOwnerId = businessOwnerId,
                UserId = userId,
                NewRole = UserRoleEnum.ProjectManager.ToString()
            };

            var user = new User
            {
                Id = userId,
                Email = "user@example.com",
                FullName = "Test User",
                ManagedById = businessOwnerId
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.Member.ToString() });

            _mockUserManager
                .Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager
                .Setup(x => x.AddToRoleAsync(user, request.NewRole))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.ReAssignRoleAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("User role reassigned successfully.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(userId, result.Data.UserId);
            Assert.Equal(UserRoleEnum.ProjectManager.ToString(), result.Data.NewRole);

            _mockUserManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _mockUserManager.Verify(x => x.GetRolesAsync(user), Times.Once);
            _mockUserManager.Verify(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()), Times.Once);
            _mockUserManager.Verify(x => x.AddToRoleAsync(user, request.NewRole), Times.Once);
        }

        [Fact]
        public async Task ReAssignRoleAsync_WithNonExistentUser_ReturnsErrorResponse()
        {
            // Arrange
            var request = new ReAssignRoleRequest
            {
                BusinessOwnerId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                NewRole = UserRoleEnum.ProjectManager.ToString()
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(request.UserId.ToString()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.ReAssignRoleAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Message);
            Assert.Null(result.Data);

            _mockUserManager.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Never);
            _mockUserManager.Verify(x => x.RemoveFromRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        [Fact]
        public async Task ReAssignRoleAsync_WithUnauthorizedBusinessOwner_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var wrongBusinessOwnerId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var request = new ReAssignRoleRequest
            {
                BusinessOwnerId = wrongBusinessOwnerId,
                UserId = userId,
                NewRole = UserRoleEnum.ProjectManager.ToString()
            };

            var user = new User
            {
                Id = userId,
                Email = "user@example.com",
                FullName = "Test User",
                ManagedById = businessOwnerId
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.ReAssignRoleAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You do not have permission to change this user's role.", result.Message);
            Assert.Null(result.Data);

            _mockUserManager.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task ReAssignRoleAsync_WithUserNotManagedByBusinessOwner_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var request = new ReAssignRoleRequest
            {
                BusinessOwnerId = businessOwnerId,
                UserId = userId,
                NewRole = UserRoleEnum.ProjectManager.ToString()
            };

            var user = new User
            {
                Id = userId,
                Email = "user@example.com",
                FullName = "Test User",
                ManagedById = null // Not managed by anyone
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.ReAssignRoleAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You do not have permission to change this user's role.", result.Message);
        }

        [Fact]
        public async Task ReAssignRoleAsync_WithInvalidRole_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var request = new ReAssignRoleRequest
            {
                BusinessOwnerId = businessOwnerId,
                UserId = userId,
                NewRole = "InvalidRole"
            };

            var user = new User
            {
                Id = userId,
                Email = "user@example.com",
                FullName = "Test User",
                ManagedById = businessOwnerId
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.ReAssignRoleAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid role specified. Only ProjectManager and Member roles are allowed.", result.Message);
            Assert.Null(result.Data);

            _mockUserManager.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task ReAssignRoleAsync_WithBusinessOwnerRole_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var request = new ReAssignRoleRequest
            {
                BusinessOwnerId = businessOwnerId,
                UserId = userId,
                NewRole = UserRoleEnum.BusinessOwner.ToString()
            };

            var user = new User
            {
                Id = userId,
                Email = "user@example.com",
                FullName = "Test User",
                ManagedById = businessOwnerId
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.ReAssignRoleAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid role specified. Only ProjectManager and Member roles are allowed.", result.Message);
        }

        [Fact]
        public async Task ReAssignRoleAsync_WhenRemoveRoleFails_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var request = new ReAssignRoleRequest
            {
                BusinessOwnerId = businessOwnerId,
                UserId = userId,
                NewRole = UserRoleEnum.ProjectManager.ToString()
            };

            var user = new User
            {
                Id = userId,
                Email = "user@example.com",
                FullName = "Test User",
                ManagedById = businessOwnerId
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.Member.ToString() });

            _mockUserManager
                .Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Failed to remove role" }));

            // Act
            var result = await _userService.ReAssignRoleAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to remove user from current roles.", result.Message);

            _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ReAssignRoleAsync_WhenAddRoleFails_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var request = new ReAssignRoleRequest
            {
                BusinessOwnerId = businessOwnerId,
                UserId = userId,
                NewRole = UserRoleEnum.ProjectManager.ToString()
            };

            var user = new User
            {
                Id = userId,
                Email = "user@example.com",
                FullName = "Test User",
                ManagedById = businessOwnerId
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.Member.ToString() });

            _mockUserManager
                .Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager
                .Setup(x => x.AddToRoleAsync(user, request.NewRole))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Failed to add role" }));

            // Act
            var result = await _userService.ReAssignRoleAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to assign new role to user.", result.Message);
        }

        [Fact]
        public async Task ReAssignRoleAsync_FromMemberToProjectManager_ReturnsSuccessResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var request = new ReAssignRoleRequest
            {
                BusinessOwnerId = businessOwnerId,
                UserId = userId,
                NewRole = UserRoleEnum.ProjectManager.ToString()
            };

            var user = new User
            {
                Id = userId,
                Email = "member@example.com",
                FullName = "Member User",
                ManagedById = businessOwnerId
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.Member.ToString() });

            _mockUserManager
                .Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager
                .Setup(x => x.AddToRoleAsync(user, UserRoleEnum.ProjectManager.ToString()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.ReAssignRoleAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(UserRoleEnum.ProjectManager.ToString(), result.Data.NewRole);
        }

        [Fact]
        public async Task ReAssignRoleAsync_FromProjectManagerToMember_ReturnsSuccessResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var request = new ReAssignRoleRequest
            {
                BusinessOwnerId = businessOwnerId,
                UserId = userId,
                NewRole = UserRoleEnum.Member.ToString()
            };

            var user = new User
            {
                Id = userId,
                Email = "pm@example.com",
                FullName = "PM User",
                ManagedById = businessOwnerId
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.ProjectManager.ToString() });

            _mockUserManager
                .Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager
                .Setup(x => x.AddToRoleAsync(user, UserRoleEnum.Member.ToString()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.ReAssignRoleAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(UserRoleEnum.Member.ToString(), result.Data.NewRole);
        }
    }
}
