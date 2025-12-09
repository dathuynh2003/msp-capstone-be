using Microsoft.AspNetCore.Identity;
using Moq;
using MSP.Application.Abstracts;
using MSP.Application.Models.Responses.Limitation;
using MSP.Application.Models.Responses.Package;
using MSP.Application.Models.Responses.Subscription;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.SubscriptionService;
using MSP.Application.Services.Interfaces.Payment;
using MSP.Application.Services.Interfaces.Subscription;
using MSP.Domain.Entities;
using Xunit;

namespace MSP.Tests.Services.SubscriptionServicesTest
{
    public class UgradeSubscriptionTest
    {
        private readonly Mock<IPaymentService> _mockPaymentService;
        private readonly Mock<IPackageRepository> _mockPackageRepository;
        private readonly Mock<ISubscriptionRepository> _mockSubscriptionRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IMeetingRepository> _mockMeetingRepository;
        private readonly Mock<IOrganizationInviteRepository> _mockOrganizationInviteRepository;

        private readonly ISubscriptionService _subscriptionService;

        public UgradeSubscriptionTest()
        {
            _mockPaymentService = new Mock<IPaymentService>();
            _mockPackageRepository = new Mock<IPackageRepository>();
            _mockSubscriptionRepository = new Mock<ISubscriptionRepository>();
            _mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null
            );
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockMeetingRepository = new Mock<IMeetingRepository>();
            _mockOrganizationInviteRepository = new Mock<IOrganizationInviteRepository>();

            _subscriptionService = new SubscriptionService(
                _mockPaymentService.Object,
                _mockPackageRepository.Object,
                _mockSubscriptionRepository.Object,
                _mockUserManager.Object,
                _mockProjectRepository.Object,
                _mockUserRepository.Object,
                _mockMeetingRepository.Object,
                _mockOrganizationInviteRepository.Object
            );
        }

        #region GetActiveSubscriptionByUserIdAsync Tests

        [Fact]
        public async Task GetActiveSubscriptionByUserIdAsync_WithValidUserId_ReturnsSuccessResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var subscriptionId = Guid.NewGuid();
            var packageId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FullName = "Test User",
                CreatedAt = DateTime.UtcNow
            };

            var limitation = new Limitation
            {
                Id = Guid.NewGuid(),
                Name = "Projects",
                Description = "Number of projects",
                IsUnlimited = false,
                LimitValue = 5,
                LimitUnit = "projects",
                IsDeleted = false
            };

            var package = new Package
            {
                Id = packageId,
                Name = "Premium",
                Description = "Premium package",
                Price = 100000,
                BillingCycle = 1,
                Currency = "VND",
                Limitations = new List<Limitation> { limitation }
            };

            var subscription = new Subscription
            {
                Id = subscriptionId,
                UserId = userId,
                PackageId = packageId,
                TotalPrice = 100000,
                IsActive = true,
                Status = "ACTIVE",
                PaymentMethod = "PayOS",
                TransactionID = "TXN123456",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(30),
                PaidAt = DateTime.UtcNow.AddDays(-30),
                User = user,
                Package = package
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockSubscriptionRepository
                .Setup(x => x.GetActiveSubscriptionByUserIdAsync(userId))
                .ReturnsAsync(subscription);

            // Act
            var result = await _subscriptionService.GetActiveSubscriptionByUserIdAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Get active subscription successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(subscriptionId, result.Data.Id);
            Assert.Equal(userId, result.Data.UserId);
            Assert.Equal(packageId, result.Data.PackageId);
            Assert.Equal(100000, result.Data.TotalPrice);
            Assert.True(result.Data.IsActive);
            Assert.Equal("ACTIVE", result.Data.Status);
            Assert.Equal("PayOS", result.Data.PaymentMethod);
            Assert.Equal("TXN123456", result.Data.TransactionID);

            _mockUserManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _mockSubscriptionRepository.Verify(x => x.GetActiveSubscriptionByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetActiveSubscriptionByUserIdAsync_WithInvalidUserId_ReturnsUserNotFoundError()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _subscriptionService.GetActiveSubscriptionByUserIdAsync(userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
            Assert.Null(result.Data);

            _mockUserManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _mockSubscriptionRepository.Verify(x => x.GetActiveSubscriptionByUserIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetActiveSubscriptionByUserIdAsync_WithNoActiveSubscription_ReturnsErrorResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FullName = "Test User",
                CreatedAt = DateTime.UtcNow
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockSubscriptionRepository
                .Setup(x => x.GetActiveSubscriptionByUserIdAsync(userId))
                .ReturnsAsync((Subscription)null);

            // Act
            var result = await _subscriptionService.GetActiveSubscriptionByUserIdAsync(userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("No active subscription found for the user", result.Message);
            Assert.Null(result.Data);

            _mockUserManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _mockSubscriptionRepository.Verify(x => x.GetActiveSubscriptionByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetActiveSubscriptionByUserIdAsync_WithValidSubscription_MapsUserDataCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var subscriptionId = Guid.NewGuid();
            var packageId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "john.doe@example.com",
                FullName = "John Doe",
                CreatedAt = DateTime.UtcNow.AddYears(-1)
            };

            var package = new Package
            {
                Id = packageId,
                Name = "Enterprise",
                Description = "Enterprise package",
                Price = 10000,
                BillingCycle = 1,
                Currency = "VND",
                Limitations = new List<Limitation>()
            };

            var subscription = new Subscription
            {
                Id = subscriptionId,
                UserId = userId,
                PackageId = packageId,
                TotalPrice = 100000,
                IsActive = true,
                Status = "ACTIVE",
                PaymentMethod = "PayOS",
                TransactionID = "TXN999999",
                StartDate = DateTime.UtcNow.AddDays(-365),
                EndDate = DateTime.UtcNow.AddDays(365),
                PaidAt = DateTime.UtcNow.AddDays(-365),
                User = user,
                Package = package
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockSubscriptionRepository
                .Setup(x => x.GetActiveSubscriptionByUserIdAsync(userId))
                .ReturnsAsync(subscription);

            // Act
            var result = await _subscriptionService.GetActiveSubscriptionByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result.Data.User);
            Assert.Equal(userId, result.Data.User.Id);
            Assert.Equal("john.doe@example.com", result.Data.User.Email);
            Assert.Equal("John Doe", result.Data.User.FullName);
        }

        [Fact]
        public async Task GetActiveSubscriptionByUserIdAsync_WithValidSubscription_MapsPackageDataCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var subscriptionId = Guid.NewGuid();
            var packageId = Guid.NewGuid();
            var limitationId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FullName = "Test User",
                CreatedAt = DateTime.UtcNow
            };

            var limitation = new Limitation
            {
                Id = limitationId,
                Name = "Meetings",
                Description = "Number of meetings per month",
                IsUnlimited = false,
                LimitValue = 100,
                LimitUnit = "meetings",
                IsDeleted = false
            };

            var package = new Package
            {
                Id = packageId,
                Name = "Standard",
                Description = "Standard package",
                Price = 100000,
                BillingCycle = 1,
                Currency = "VND",
                Limitations = new List<Limitation> { limitation }
            };

            var subscription = new Subscription
            {
                Id = subscriptionId,
                UserId = userId,
                PackageId = packageId,
                TotalPrice = 100000,
                IsActive = true,
                Status = "ACTIVE",
                PaymentMethod = "PayOS",
                TransactionID = "TXN123456",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                PaidAt = DateTime.UtcNow,
                User = user,
                Package = package
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockSubscriptionRepository
                .Setup(x => x.GetActiveSubscriptionByUserIdAsync(userId))
                .ReturnsAsync(subscription);

            // Act
            var result = await _subscriptionService.GetActiveSubscriptionByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result.Data.Package);
            Assert.Equal(packageId, result.Data.Package.Id);
            Assert.Equal("Standard", result.Data.Package.Name);
            Assert.Equal("Standard package", result.Data.Package.Description);
            Assert.Equal(100000, result.Data.Package.Price);
            Assert.Equal("VND", result.Data.Package.Currency);
            Assert.Single(result.Data.Package.Limitations);
            Assert.Equal(limitationId, result.Data.Package.Limitations.First().Id);
            Assert.Equal("Meetings", result.Data.Package.Limitations.First().Name);
        }

        [Fact]
        public async Task GetActiveSubscriptionByUserIdAsync_WithMultipleLimitations_MapsAllLimitationsCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var subscriptionId = Guid.NewGuid();
            var packageId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FullName = "Test User",
                CreatedAt = DateTime.UtcNow
            };

            var limitations = new List<Limitation>
            {
                new Limitation
                {
                    Id = Guid.NewGuid(),
                    Name = "Projects",
                    Description = "Number of projects",
                    IsUnlimited = false,
                    LimitValue = 10,
                    LimitUnit = "projects",
                    IsDeleted = false
                },
                new Limitation
                {
                    Id = Guid.NewGuid(),
                    Name = "Members",
                    Description = "Team members",
                    IsUnlimited = true,
                    LimitValue = null,
                    LimitUnit = "members",
                    IsDeleted = false
                },
            };

            var package = new Package
            {
                Id = packageId,
                Name = "Premium",
                Description = "Premium package",
                Price = 200000,
                BillingCycle = 1,
                Currency = "VND",
                Limitations = limitations
            };

            var subscription = new Subscription
            {
                Id = subscriptionId,
                UserId = userId,
                PackageId = packageId,
                TotalPrice = 200000,
                IsActive = true,
                Status = "ACTIVE",
                PaymentMethod = "PayOS",
                TransactionID = "TXN123456",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                PaidAt = DateTime.UtcNow,
                User = user,
                Package = package
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockSubscriptionRepository
                .Setup(x => x.GetActiveSubscriptionByUserIdAsync(userId))
                .ReturnsAsync(subscription);

            // Act
            var result = await _subscriptionService.GetActiveSubscriptionByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result.Data.Package.Limitations);
            Assert.Equal(2, result.Data.Package.Limitations.Count);
            
            var projectLimit = result.Data.Package.Limitations.First(l => l.Name == "Projects");
            Assert.Equal(10, projectLimit.LimitValue);
            Assert.False(projectLimit.IsUnlimited);

            var memberLimit = result.Data.Package.Limitations.First(l => l.Name == "Members");
            Assert.True(memberLimit.IsUnlimited);

        }

        #endregion
    }
}
