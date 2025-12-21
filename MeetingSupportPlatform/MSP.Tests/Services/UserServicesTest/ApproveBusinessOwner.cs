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
    public class ApproveBusinessOwnerTest
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
        public ApproveBusinessOwnerTest()
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

        #region TC_Approve_01 - User not found
        [Fact]
        public async Task ApproveBusinessOwnerAsync_UserNotFound_ReturnsError()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.ApproveBusinessOwnerAsync(userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Message);
            Assert.Null(result.Data);

            _mockUserManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _mockUserManager.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Never);
            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }
        #endregion

        #region TC_Approve_02 - User is not BusinessOwner
        [Fact]
        public async Task ApproveBusinessOwnerAsync_UserNotBusinessOwner_ReturnsError()
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
                .ReturnsAsync(new List<string> { "Member" }); // Not BusinessOwner

            // Act
            var result = await _userService.ApproveBusinessOwnerAsync(userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User is not a BusinessOwner.", result.Message);

            _mockUserManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _mockUserManager.Verify(x => x.GetRolesAsync(user), Times.Once);
            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }
        #endregion

        #region TC_Approve_03 - User already approved
        [Fact]
        public async Task ApproveBusinessOwnerAsync_AlreadyApproved_ReturnsSuccess()
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
            var result = await _userService.ApproveBusinessOwnerAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("User is already approved.", result.Message);
            Assert.Equal("User is already approved.", result.Data);

            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
            _mockNotificationService.Verify(
                x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()),
                Times.Never);
        }
        #endregion

        #region TC_Approve_04 - Success approval with free package
        [Fact]
        public async Task ApproveBusinessOwnerAsync_ValidUser_ApprovesSuccessfully()
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

            var freePackage = new Package
            {
                Id = Guid.NewGuid(),
                Name = "Free Package",
                BillingCycle = 30
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.BusinessOwner.ToString() });

            _mockUserManager
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            _mockPackageRepository
                .Setup(x => x.GetFreePackageAsync())
                .ReturnsAsync(freePackage);

            // ✅ Sửa lại phần này - trả về Task<Subscription>
            _mockSubscriptionRepository
                .Setup(x => x.AddAsync(It.IsAny<Subscription>()))
                .ReturnsAsync((Subscription s) => s); // Trả về subscription

            _mockSubscriptionRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask); // Hoặc .ReturnsAsync(1) nếu trả về Task<int>

            // Act
            var result = await _userService.ApproveBusinessOwnerAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("User has been approved as BusinessOwner successfully.", result.Message);
            Assert.Equal("BusinessOwner approved successfully!", result.Data);
            Assert.True(user.IsApproved);

            _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);

            // Verify notification sent
            _mockNotificationService.Verify(
                x => x.CreateInAppNotificationAsync(It.Is<CreateNotificationRequest>(n =>
                    n.UserId == userId &&
                    n.Title == "Account approved" &&
                    n.Type == NotificationTypeEnum.InApp.ToString())),
                Times.Once);

            // Verify email sent
            _mockNotificationService.Verify(
                x => x.SendEmailNotification(
                    user.Email,
                    "BusinessOwner Account Approved - Meeting Support Platform",
                    It.IsAny<string>()),
                Times.Once);

            // Verify subscription created
            _mockSubscriptionRepository.Verify(
                x => x.AddAsync(It.Is<Subscription>(s =>
                    s.UserId == userId &&
                    s.PackageId == freePackage.Id &&
                    s.TotalPrice == 0 &&
                    s.TransactionID == "FREE_PACKAGE" &&
                    s.IsActive == true &&
                    s.Status == PaymentEnum.Paid.ToString().ToUpper())),
                Times.Once);

            _mockSubscriptionRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
        #endregion

        #region TC_Approve_05 - Update fails
        [Fact]
        public async Task ApproveBusinessOwnerAsync_UpdateFails_ReturnsError()
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

            _mockUserManager
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));

            // Act
            var result = await _userService.ApproveBusinessOwnerAsync(userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to approve user.", result.Message);

            _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
            _mockNotificationService.Verify(
                x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()),
                Times.Never);
            _mockNotificationService.Verify(
                x => x.SendEmailNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }
        #endregion

        #region TC_Approve_06 - No free package available
        [Fact]
        public async Task ApproveBusinessOwnerAsync_NoFreePackage_StillApprovesSuccessfully()
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

            _mockUserManager
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            _mockPackageRepository
                .Setup(x => x.GetFreePackageAsync())
                .ReturnsAsync((Package)null); // No free package

            // Act
            var result = await _userService.ApproveBusinessOwnerAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("BusinessOwner approved successfully!", result.Data);

            // Notification and email still sent
            _mockNotificationService.Verify(
                x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()),
                Times.Once);
            _mockNotificationService.Verify(
                x => x.SendEmailNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);

            // But no subscription created
            _mockSubscriptionRepository.Verify(
                x => x.AddAsync(It.IsAny<Subscription>()),
                Times.Never);
        }
        #endregion

        #region TC_Approve_07 - Verify subscription details
        [Fact]
        public async Task ApproveBusinessOwnerAsync_ValidUser_CreatesCorrectSubscription()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var packageId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                FullName = "Test Owner",
                Email = "test@example.com",
                IsApproved = false
            };

            var freePackage = new Package
            {
                Id = packageId,
                Name = "Free Plan",
                BillingCycle = 60
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { UserRoleEnum.BusinessOwner.ToString() });

            _mockUserManager
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            _mockPackageRepository
                .Setup(x => x.GetFreePackageAsync())
                .ReturnsAsync(freePackage);

            Subscription capturedSubscription = null;

            // ✅ Sửa lại phần này
            _mockSubscriptionRepository
                .Setup(x => x.AddAsync(It.IsAny<Subscription>()))
                .Callback((Subscription s) => capturedSubscription = s)
                .ReturnsAsync((Subscription s) => s); // ✅ Trả về Task<Subscription>

            _mockSubscriptionRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.ApproveBusinessOwnerAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(capturedSubscription);
            Assert.Equal(userId, capturedSubscription.UserId);
            Assert.Equal(packageId, capturedSubscription.PackageId);
            Assert.Equal(0, capturedSubscription.TotalPrice);
            Assert.Equal("FREE_PACKAGE", capturedSubscription.TransactionID);
            Assert.Equal("System", capturedSubscription.PaymentMethod);
            Assert.True(capturedSubscription.IsActive);
            Assert.Equal(PaymentEnum.Paid.ToString().ToUpper(), capturedSubscription.Status);

            //// Verify date range
            //var now = DateTime.UtcNow;
            //Assert.True((now - capturedSubscription.StartDate).TotalMinutes < 1);
            //Assert.True((capturedSubscription.EndDate - now.AddDays(60)).TotalMinutes < 1);
        }
        #endregion

        #region TC_Approve_08 - Empty Guid
        [Fact]
        public async Task ApproveBusinessOwnerAsync_EmptyGuid_ReturnsUserNotFound()
        {
            // Arrange
            var userId = Guid.Empty;

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.ApproveBusinessOwnerAsync(userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Message);
        }
        #endregion
    }
}
