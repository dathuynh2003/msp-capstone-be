using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using MSP.Application.Abstracts;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Users;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Services.Interfaces.Users;
using MSP.Domain.Entities;
using MSP.Shared.Enums;
using Xunit;

namespace MSP.Tests.Services.OrganizationServicesTest
{
    public class GetMemberDetailTest
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

        public GetMemberDetailTest()
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

            _userService = new UserService(
                _mockUserManager.Object,
                _mockUserRepository.Object,
                _mockNotificationService.Object,
                _mockOrganizationInviteRepository.Object,
                _mockProjectMemberRepository.Object,
                _mockProjectRepository.Object,
                _mockSubscriptionRepository.Object,
                _mockPackageRepository.Object
            );
        }

        [Fact]
        public async Task GetUserDetailByIdAsync_WithValidUserId_ReturnsSuccessResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var managerId = Guid.NewGuid();

            var manager = new User
            {
                Id = managerId,
                Email = "manager@example.com",
                FullName = "Manager User"
            };

            var user = new User
            {
                Id = userId,
                Email = "user@example.com",
                FullName = "Test User",
                PhoneNumber = "1234567890",
                AvatarUrl = "https://example.com/avatar.jpg",
                Organization = "Test Organization",
                ManagedById = managerId,
                ManagedBy = manager,
                CreatedAt = DateTime.UtcNow
            };

            var users = new List<User> { user }.AsQueryable();

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(MockDbSet(users));

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.Member.ToString() });

            // Act
            var result = await _userService.GetUserDetailByIdAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("User details retrieved successfully.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(userId, result.Data.Id);
            Assert.Equal("Test User", result.Data.FullName);
            Assert.Equal("user@example.com", result.Data.Email);
            Assert.Equal("1234567890", result.Data.PhoneNumber);
            Assert.Equal("https://example.com/avatar.jpg", result.Data.AvatarUrl);
            Assert.Equal("Test Organization", result.Data.Organization);
            Assert.Equal(managerId, result.Data.ManagedBy);
            Assert.Equal("Manager User", result.Data.ManagerName);
            Assert.Equal(UserRoleEnum.Member.ToString(), result.Data.RoleName);
        }

        [Fact]
        public async Task GetUserDetailByIdAsync_WithNonExistentUserId_ReturnsErrorResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var users = new List<User>().AsQueryable();

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(MockDbSet(users));

            // Act
            var result = await _userService.GetUserDetailByIdAsync(userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Message);
            Assert.Null(result.Data);

            _mockUserManager.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task GetUserDetailByIdAsync_WithUserWithoutManager_ReturnsSuccessResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "user@example.com",
                FullName = "Independent User",
                PhoneNumber = "1234567890",
                AvatarUrl = null,
                Organization = null,
                ManagedById = null,
                ManagedBy = null,
                CreatedAt = DateTime.UtcNow
            };

            var users = new List<User> { user }.AsQueryable();

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(MockDbSet(users));

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.Member.ToString() });

            // Act
            var result = await _userService.GetUserDetailByIdAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Null(result.Data.ManagedBy);
            Assert.Null(result.Data.ManagerName);
            Assert.Null(result.Data.Organization);
            Assert.Null(result.Data.AvatarUrl);
        }

        [Fact]
        public async Task GetUserDetailByIdAsync_WithBusinessOwner_ReturnsCorrectRole()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "owner@example.com",
                FullName = "Business Owner",
                Organization = "My Business",
                ManagedById = null,
                CreatedAt = DateTime.UtcNow
            };

            var users = new List<User> { user }.AsQueryable();

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(MockDbSet(users));

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.BusinessOwner.ToString() });

            // Act
            var result = await _userService.GetUserDetailByIdAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(UserRoleEnum.BusinessOwner.ToString(), result.Data.RoleName);
            Assert.Equal("My Business", result.Data.Organization);
            Assert.Null(result.Data.ManagedBy);
        }

        [Fact]
        public async Task GetUserDetailByIdAsync_WithProjectManager_ReturnsCorrectRole()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var managerId = Guid.NewGuid();

            var manager = new User
            {
                Id = managerId,
                FullName = "Business Owner"
            };

            var user = new User
            {
                Id = userId,
                Email = "pm@example.com",
                FullName = "Project Manager",
                Organization = "Test Organization",
                ManagedById = managerId,
                ManagedBy = manager,
                CreatedAt = DateTime.UtcNow
            };

            var users = new List<User> { user }.AsQueryable();

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(MockDbSet(users));

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.ProjectManager.ToString() });

            // Act
            var result = await _userService.GetUserDetailByIdAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(UserRoleEnum.ProjectManager.ToString(), result.Data.RoleName);
            Assert.Equal(managerId, result.Data.ManagedBy);
            Assert.Equal("Business Owner", result.Data.ManagerName);
        }

        [Fact]
        public async Task GetUserDetailByIdAsync_WithUserWithNoRoles_ReturnsDefaultRole()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "user@example.com",
                FullName = "No Role User",
                CreatedAt = DateTime.UtcNow
            };

            var users = new List<User> { user }.AsQueryable();

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(MockDbSet(users));

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            // Act
            var result = await _userService.GetUserDetailByIdAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Member", result.Data.RoleName);
        }

        [Fact]
        public async Task GetUserDetailByIdAsync_WithUserHavingMultipleRoles_ReturnsFirstRole()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "user@example.com",
                FullName = "Multi Role User",
                CreatedAt = DateTime.UtcNow
            };

            var users = new List<User> { user }.AsQueryable();

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(MockDbSet(users));

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> 
                { 
                    UserRoleEnum.ProjectManager.ToString(),
                    UserRoleEnum.Member.ToString() 
                });

            // Act
            var result = await _userService.GetUserDetailByIdAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(UserRoleEnum.ProjectManager.ToString(), result.Data.RoleName);
        }

        [Fact]
        public async Task GetUserDetailByIdAsync_IncludesManagedByRelationship()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var managerId = Guid.NewGuid();

            var manager = new User
            {
                Id = managerId,
                Email = "manager@example.com",
                FullName = "Manager Name",
                Organization = "Manager Org"
            };

            var user = new User
            {
                Id = userId,
                Email = "user@example.com",
                FullName = "User Name",
                ManagedById = managerId,
                ManagedBy = manager,
                CreatedAt = DateTime.UtcNow
            };

            var users = new List<User> { user }.AsQueryable();

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(MockDbSet(users));

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.Member.ToString() });

            // Act
            var result = await _userService.GetUserDetailByIdAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(managerId, result.Data.ManagedBy);
            Assert.Equal("Manager Name", result.Data.ManagerName);
        }

        [Fact]
        public async Task GetUserDetailByIdAsync_ReturnsAllUserProperties()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow.AddDays(-30);

            var user = new User
            {
                Id = userId,
                Email = "complete@example.com",
                FullName = "Complete User",
                PhoneNumber = "9876543210",
                AvatarUrl = "https://example.com/complete.jpg",
                Organization = "Complete Organization",
                CreatedAt = createdAt
            };

            var users = new List<User> { user }.AsQueryable();

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(MockDbSet(users));

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.Member.ToString() });

            // Act
            var result = await _userService.GetUserDetailByIdAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(userId, result.Data.Id);
            Assert.Equal("Complete User", result.Data.FullName);
            Assert.Equal("complete@example.com", result.Data.Email);
            Assert.Equal("9876543210", result.Data.PhoneNumber);
            Assert.Equal("https://example.com/complete.jpg", result.Data.AvatarUrl);
            Assert.Equal("Complete Organization", result.Data.Organization);
            Assert.Equal(createdAt, result.Data.CreatedAt);
        }

        // Helper method to mock DbSet
        private static DbSet<T> MockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet.Object;
        }
    }
}
