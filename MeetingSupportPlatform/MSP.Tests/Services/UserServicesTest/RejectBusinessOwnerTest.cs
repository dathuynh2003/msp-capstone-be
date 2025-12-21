using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using MSP.Application.Abstracts;
using MSP.Application.Models.Requests.Notification;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Users;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Domain.Entities;
using MSP.Shared.Enums;
using Xunit;

namespace MSP.Tests.Services.UserServicesTest
{
    public class RejectBusinessOwnerTest
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
        public RejectBusinessOwnerTest()
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

        #region TC_Reject_01 - User not found
        [Fact]
        public async Task RejectBusinessOwnerAsync_UserNotFound_ReturnsError()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.RejectBusinessOwnerAsync(userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Message);
            Assert.Null(result.Data);

            _mockUserManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _mockUserManager.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Never);
            _mockNotificationService.Verify(
                x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()),
                Times.Never);
        }
        #endregion

        #region TC_Reject_02 - User is not BusinessOwner
        [Fact]
        public async Task RejectBusinessOwnerAsync_UserNotBusinessOwner_ReturnsError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                FullName = "John Member",
                Email = "member@example.com",
                IsApproved = false
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Member" });

            // Act
            var result = await _userService.RejectBusinessOwnerAsync(userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User is not a BusinessOwner.", result.Message);

            _mockUserManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _mockUserManager.Verify(x => x.GetRolesAsync(user), Times.Once);
            _mockNotificationService.Verify(
                x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()),
                Times.Never);
        }
        #endregion

        #region TC_Reject_03 - User already approved
        [Fact]
        public async Task RejectBusinessOwnerAsync_UserAlreadyApproved_ReturnsError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                FullName = "Jane Owner",
                Email = "owner@example.com",
                IsApproved = true // Already approved
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.BusinessOwner.ToString() });

            // Act
            var result = await _userService.RejectBusinessOwnerAsync(userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Cannot reject an already approved user.", result.Message);

            _mockUserManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _mockUserManager.Verify(x => x.GetRolesAsync(user), Times.Once);
            _mockNotificationService.Verify(
                x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()),
                Times.Never);
        }
        #endregion

        #region TC_Reject_04 - Success rejection with notifications
        [Fact]
        public async Task RejectBusinessOwnerAsync_ValidUser_RejectsSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                FullName = "Jane Owner",
                Email = "owner@example.com",
                IsApproved = false
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.BusinessOwner.ToString() });

            // Act
            var result = await _userService.RejectBusinessOwnerAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("User BusinessOwner request has been rejected successfully.", result.Message);
            Assert.Equal("BusinessOwner rejected successfully!", result.Data);

            // Verify in-app notification sent
            _mockNotificationService.Verify(
                x => x.CreateInAppNotificationAsync(It.Is<CreateNotificationRequest>(n =>
                    n.UserId == userId &&
                    n.Title == "Account rejected" &&
                    n.Message.Contains("your BusinessOwner account has been rejected") &&
                    n.Type == NotificationTypeEnum.InApp.ToString())),
                Times.Once);

            // Verify email notification sent
            _mockNotificationService.Verify(
                x => x.SendEmailNotification(
                    user.Email,
                    "BusinessOwner Request Not Approved - Meeting Support Platform",
                    It.IsAny<string>()),
                Times.Once);
        }
        #endregion

        #region TC_Reject_05 - Empty Guid
        [Fact]
        public async Task RejectBusinessOwnerAsync_EmptyGuid_ReturnsUserNotFound()
        {
            // Arrange
            var userId = Guid.Empty;

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.RejectBusinessOwnerAsync(userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Message);
        }
        #endregion
    }
}
