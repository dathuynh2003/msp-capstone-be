using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using MSP.Application.Abstracts;
using MSP.Application.Models.Requests.User;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Users;
// Import các namespace chứa Interface Repository/Service của bạn
using MSP.Application.Services.Interfaces.Notification;
using MSP.Domain.Entities;
using Xunit;

namespace MSP.Tests.Services.UserServicesTest
{
    public class UpdateUserProfileTest
    {
        // 1. Khai báo Mock cho tất cả dependency
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IOrganizationInviteRepository> _mockOrganizationInviteRepository;
        private readonly Mock<IProjectMemberRepository> _mockProjectMemberRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IProjectTaskRepository> _mockProjectTaskRepository;
        private readonly Mock<ISubscriptionRepository> _mockSubscriptionRepository;
        private readonly Mock<IPackageRepository> _mockPackageRepository;
        private readonly IConfiguration _configuration;
        private readonly UserService _userService;

        public UpdateUserProfileTest()
        {
            // 2. Setup Mock UserManager (Phức tạp nhất vì nó là class cụ thể, không phải interface)
            _mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null
            );

            // 3. Khởi tạo các Mock còn lại (Đơn giản hơn vì là Interface)
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
            // 4. Khởi tạo UserService với đầy đủ tham số
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
        [Fact]
        public async Task UpdateUserProfileAsync_UserNotFound_ReturnsError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new UpdateUserProfileRequest { FullName = "New Name" };

            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.UpdateUserProfileAsync(userId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Message);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_UpdateFailed_ReturnsError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, FullName = "Old Name" };
            var request = new UpdateUserProfileRequest { FullName = "New Name" };

            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

            // Act
            var result = await _userService.UpdateUserProfileAsync(userId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to update user profile.", result.Message);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_Success_UpdatesAllFields()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, FullName = "Old", PhoneNumber = "000", AvatarUrl = "old.png" };
            var request = new UpdateUserProfileRequest { FullName = "New", PhoneNumber = "123", AvatarUrl = "new.png" };

            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.UpdateUserProfileAsync(userId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("New", user.FullName);
            Assert.Equal("123", user.PhoneNumber);
            Assert.Equal("new.png", user.AvatarUrl);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_Success_PartialUpdate()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, FullName = "Old", PhoneNumber = "000" };
            var request = new UpdateUserProfileRequest { FullName = "New", PhoneNumber = null }; // Chỉ update tên

            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.UpdateUserProfileAsync(userId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("New", user.FullName); // Đổi
            Assert.Equal("000", user.PhoneNumber); // Giữ nguyên
        }
    }
}