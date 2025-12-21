using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MockQueryable;
using Moq;
using MSP.Application.Abstracts;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Users;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Domain.Entities;
using Xunit;

namespace MSP.Tests.Services.UserServicesTest
{
    public class GetUserDetailTest
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IOrganizationInviteRepository> _mockOrganizationInviteRepository;
        private readonly Mock<IProjectMemberRepository> _mockProjectMemberRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IProjectTaskRepository> _mockProjectTaskRepository;
        private readonly Mock<ISubscriptionRepository> _mockSubscriptionRepository;
        private readonly Mock<IPackageRepository> _mockPackageRepository;
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;
        public GetUserDetailTest()
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
            _mockProjectTaskRepository = new Mock<IProjectTaskRepository>();
            _mockSubscriptionRepository = new Mock<ISubscriptionRepository>();
            _mockPackageRepository = new Mock<IPackageRepository>();
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
                _mockProjectTaskRepository.Object,
                _mockSubscriptionRepository.Object,
                _mockPackageRepository.Object,
                _configuration
            );
        }

        #region TC_GetDetail_01 - User not found
        [Fact]
        public async Task GetUserDetailByIdAsync_UserNotFound_ReturnsError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var emptyUsers = new List<User>();

            // ✅ Sử dụng BuildMock() từ MockQueryable
            _mockUserManager
                .Setup(x => x.Users)
                .Returns(emptyUsers.AsQueryable().BuildMock());

            // Act
            var result = await _userService.GetUserDetailByIdAsync(userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Message);
            Assert.Null(result.Data);
        }
        #endregion

        #region TC_GetDetail_02 - Success with BusinessOwner (has manager)
        [Fact]
        public async Task GetUserDetailByIdAsync_BusinessOwnerWithManager_ReturnsSuccessWithDetails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var managerId = Guid.NewGuid();

            var manager = new User
            {
                Id = managerId,
                FullName = "John Manager",
                Email = "manager@example.com"
            };

            var user = new User
            {
                Id = userId,
                FullName = "Jane Owner",
                Email = "owner@example.com",
                PhoneNumber = "0123456789",
                AvatarUrl = "avatar.png",
                Organization = "ABC Corp",
                ManagedById = managerId,
                ManagedBy = manager,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };

            var users = new List<User> { user };

            // ✅ Sử dụng BuildMock()
            _mockUserManager
                .Setup(x => x.Users)
                .Returns(users.AsQueryable().BuildMock());

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "BusinessOwner" });

            // Act
            var result = await _userService.GetUserDetailByIdAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("User details retrieved successfully.", result.Message);
            Assert.NotNull(result.Data);

            var data = result.Data;
            Assert.Equal(userId, data.Id);
            Assert.Equal("Jane Owner", data.FullName);
            Assert.Equal("owner@example.com", data.Email);
            Assert.Equal("0123456789", data.PhoneNumber);
            Assert.Equal("avatar.png", data.AvatarUrl);
            Assert.Equal("ABC Corp", data.Organization);
            Assert.Equal(managerId, data.ManagedBy);
            Assert.Equal("John Manager", data.ManagerName);
            Assert.Equal("BusinessOwner", data.RoleName);
        }
        #endregion

        #region TC_GetDetail_03 - Success with Member (no manager)
        [Fact]
        public async Task GetUserDetailByIdAsync_MemberWithoutManager_ReturnsSuccessWithNoManager()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                FullName = "John Member",
                Email = "member@example.com",
                PhoneNumber = "0987654321",
                AvatarUrl = null,
                Organization = null,
                ManagedById = null,
                ManagedBy = null,
                CreatedAt = DateTime.UtcNow
            };

            var users = new List<User> { user };

            // ✅ BuildMock()
            _mockUserManager
                .Setup(x => x.Users)
                .Returns(users.AsQueryable().BuildMock());

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Member" });

            // Act
            var result = await _userService.GetUserDetailByIdAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);

            var data = result.Data;
            Assert.Equal("John Member", data.FullName);
            Assert.Equal("member@example.com", data.Email);
            Assert.Null(data.ManagedBy);
            Assert.Null(data.ManagerName);
            Assert.Null(data.Organization);
            Assert.Equal("Member", data.RoleName);
        }
        #endregion

        #region TC_GetDetail_04 - User has no roles (defaults to Member)
        [Fact]
        public async Task GetUserDetailByIdAsync_UserWithNoRoles_DefaultsToMember()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                FullName = "Test User",
                Email = "test@example.com",
                CreatedAt = DateTime.UtcNow
            };

            var users = new List<User> { user };

            // ✅ BuildMock()
            _mockUserManager
                .Setup(x => x.Users)
                .Returns(users.AsQueryable().BuildMock());

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            // Act
            var result = await _userService.GetUserDetailByIdAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Member", result.Data.RoleName);
        }
        #endregion

        #region TC_GetDetail_05 - User with multiple roles (returns first)
        [Fact]
        public async Task GetUserDetailByIdAsync_UserWithMultipleRoles_ReturnsFirstRole()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                FullName = "Multi Role User",
                Email = "multi@example.com",
                CreatedAt = DateTime.UtcNow
            };

            var users = new List<User> { user };

            // ✅ BuildMock()
            _mockUserManager
                .Setup(x => x.Users)
                .Returns(users.AsQueryable().BuildMock());

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin", "BusinessOwner", "Member" });

            // Act
            var result = await _userService.GetUserDetailByIdAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Admin", result.Data.RoleName);
        }
        #endregion

        #region TC_GetDetail_06 - Verify all fields mapping
        [Fact]
        public async Task GetUserDetailByIdAsync_ValidUser_MapsAllFieldsCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var managerId = Guid.NewGuid();
            var createdDate = DateTime.UtcNow.AddMonths(-2);

            var manager = new User
            {
                Id = managerId,
                FullName = "Manager Name"
            };

            var user = new User
            {
                Id = userId,
                FullName = "Complete User",
                Email = "complete@example.com",
                PhoneNumber = "0111222333",
                AvatarUrl = "http://example.com/avatar.jpg",
                Organization = "Complete Org",
                ManagedById = managerId,
                ManagedBy = manager,
                CreatedAt = createdDate
            };

            var users = new List<User> { user };

            // ✅ BuildMock()
            _mockUserManager
                .Setup(x => x.Users)
                .Returns(users.AsQueryable().BuildMock());

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "ProjectManager" });

            // Act
            var result = await _userService.GetUserDetailByIdAsync(userId);

            // Assert
            var data = result.Data;
            Assert.Equal(userId, data.Id);
            Assert.Equal("Complete User", data.FullName);
            Assert.Equal("complete@example.com", data.Email);
            Assert.Equal("0111222333", data.PhoneNumber);
            Assert.Equal("http://example.com/avatar.jpg", data.AvatarUrl);
            Assert.Equal("Complete Org", data.Organization);
            Assert.Equal(managerId, data.ManagedBy);
            Assert.Equal("Manager Name", data.ManagerName);
            Assert.Equal(createdDate, data.CreatedAt);
            Assert.Equal("ProjectManager", data.RoleName);
        }
        #endregion

        #region TC_GetDetail_07 - GetRolesAsync throws exception
        [Fact]
        public async Task GetUserDetailByIdAsync_GetRolesThrowsException_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                FullName = "Test User",
                Email = "test@example.com",
                CreatedAt = DateTime.UtcNow
            };

            var users = new List<User> { user };

            // ✅ BuildMock()
            _mockUserManager
                .Setup(x => x.Users)
                .Returns(users.AsQueryable().BuildMock());

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ThrowsAsync(new Exception("Role service unavailable"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                async () => await _userService.GetUserDetailByIdAsync(userId));
        }
        #endregion

        #region TC_GetDetail_08 - Empty Guid (invalid ID)
        [Fact]
        public async Task GetUserDetailByIdAsync_EmptyGuid_ReturnsUserNotFound()
        {
            // Arrange
            var userId = Guid.Empty;
            var emptyUsers = new List<User>();

            // ✅ BuildMock()
            _mockUserManager
                .Setup(x => x.Users)
                .Returns(emptyUsers.AsQueryable().BuildMock());

            // Act
            var result = await _userService.GetUserDetailByIdAsync(userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Message);
            Assert.Null(result.Data);
        }
        #endregion
    }
}
