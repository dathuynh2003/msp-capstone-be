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
    public class GetTransactionHistoryTest
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

        public GetTransactionHistoryTest()
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

        #region GetSubscriptionsByUserIdAsync Tests

        [Fact]
        public async Task GetSubscriptionsByUserIdAsync_WithValidUserId_ReturnsSuccessResponse()
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

            var subscriptions = new List<Subscription> { subscription };

            _mockSubscriptionRepository
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(subscriptions);

            // Act
            var result = await _subscriptionService.GetSubscriptionsByUserIdAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Get subscriptions successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);

            var firstSubscription = result.Data.First();
            Assert.Equal(subscriptionId, firstSubscription.Id);
            Assert.Equal(userId, firstSubscription.UserId);
            Assert.Equal(packageId, firstSubscription.PackageId);
            Assert.Equal(100000, firstSubscription.TotalPrice);
            Assert.True(firstSubscription.IsActive);
            Assert.Equal("ACTIVE", firstSubscription.Status);

            _mockSubscriptionRepository.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetSubscriptionsByUserIdAsync_WithNoSubscriptions_ReturnsEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var emptySubscriptions = new List<Subscription>();

            _mockSubscriptionRepository
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(emptySubscriptions);

            // Act
            var result = await _subscriptionService.GetSubscriptionsByUserIdAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Get subscriptions successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);

            _mockSubscriptionRepository.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetSubscriptionsByUserIdAsync_WithMultipleSubscriptions_ReturnsAllSubscriptions()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var packageId1 = Guid.NewGuid();
            var packageId2 = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FullName = "Test User",
                CreatedAt = DateTime.UtcNow
            };

            var package1 = new Package
            {
                Id = packageId1,
                Name = "Standard",
                Description = "Standard package",
                Price = 50000,
                BillingCycle = 1,
                Currency = "VND",
                Limitations = new List<Limitation>()
            };

            var package2 = new Package
            {
                Id = packageId2,
                Name = "Premium",
                Description = "Premium package",
                Price = 100000,
                BillingCycle = 1,
                Currency = "VND",
                Limitations = new List<Limitation>()
            };

            var subscription1 = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PackageId = packageId1,
                TotalPrice = 50000,
                IsActive = false,
                Status = "EXPIRED",
                PaymentMethod = "PayOS",
                TransactionID = "TXN111111",
                StartDate = DateTime.UtcNow.AddDays(-90),
                EndDate = DateTime.UtcNow.AddDays(-30),
                PaidAt = DateTime.UtcNow.AddDays(-90),
                User = user,
                Package = package1
            };

            var subscription2 = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PackageId = packageId2,
                TotalPrice = 100000,
                IsActive = true,
                Status = "ACTIVE",
                PaymentMethod = "PayOS",
                TransactionID = "TXN222222",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(30),
                PaidAt = DateTime.UtcNow.AddDays(-30),
                User = user,
                Package = package2
            };

            var subscriptions = new List<Subscription> { subscription1, subscription2 };

            _mockSubscriptionRepository
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(subscriptions);

            // Act
            var result = await _subscriptionService.GetSubscriptionsByUserIdAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());

            var resultList = result.Data.ToList();
            
            // First subscription (expired)
            Assert.Equal("TXN111111", resultList[0].TransactionID);
            Assert.False(resultList[0].IsActive);
            Assert.Equal("EXPIRED", resultList[0].Status);

            // Second subscription (active)
            Assert.Equal("TXN222222", resultList[1].TransactionID);
            Assert.True(resultList[1].IsActive);
            Assert.Equal("ACTIVE", resultList[1].Status);

            _mockSubscriptionRepository.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetSubscriptionsByUserIdAsync_WithValidSubscription_MapsUserDataCorrectly()
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
                Price = 200000,
                BillingCycle = 1,
                Currency = "VND",
                Limitations = new List<Limitation>()
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
                TransactionID = "TXN999999",
                StartDate = DateTime.UtcNow.AddDays(-365),
                EndDate = DateTime.UtcNow.AddDays(365),
                PaidAt = DateTime.UtcNow.AddDays(-365),
                User = user,
                Package = package
            };

            var subscriptions = new List<Subscription> { subscription };

            _mockSubscriptionRepository
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(subscriptions);

            // Act
            var result = await _subscriptionService.GetSubscriptionsByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result.Data);
            var firstSubscription = result.Data.First();
            Assert.NotNull(firstSubscription.User);
            Assert.Equal(userId, firstSubscription.User.Id);
            Assert.Equal("john.doe@example.com", firstSubscription.User.Email);
            Assert.Equal("John Doe", firstSubscription.User.FullName);
        }

        [Fact]
        public async Task GetSubscriptionsByUserIdAsync_WithValidSubscription_MapsPackageDataCorrectly()
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

            var subscriptions = new List<Subscription> { subscription };

            _mockSubscriptionRepository
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(subscriptions);

            // Act
            var result = await _subscriptionService.GetSubscriptionsByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result.Data);
            var firstSubscription = result.Data.First();
            Assert.NotNull(firstSubscription.Package);
            Assert.Equal(packageId, firstSubscription.Package.Id);
            Assert.Equal("Standard", firstSubscription.Package.Name);
            Assert.Equal("Standard package", firstSubscription.Package.Description);
            Assert.Equal(100000, firstSubscription.Package.Price);
            Assert.Equal("VND", firstSubscription.Package.Currency);
            Assert.Single(firstSubscription.Package.Limitations);
            Assert.Equal(limitationId, firstSubscription.Package.Limitations.First().Id);
            Assert.Equal("Meetings", firstSubscription.Package.Limitations.First().Name);
        }

        [Fact]
        public async Task GetSubscriptionsByUserIdAsync_WithMultipleLimitations_MapsAllLimitationsCorrectly()
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
                }
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

            var subscriptions = new List<Subscription> { subscription };

            _mockSubscriptionRepository
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(subscriptions);

            // Act
            var result = await _subscriptionService.GetSubscriptionsByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result.Data);
            var firstSubscription = result.Data.First();
            Assert.NotNull(firstSubscription.Package.Limitations);
            Assert.Equal(2, firstSubscription.Package.Limitations.Count);
            
            var projectLimit = firstSubscription.Package.Limitations.First(l => l.Name == "Projects");
            Assert.Equal(10, projectLimit.LimitValue);
            Assert.False(projectLimit.IsUnlimited);

            var memberLimit = firstSubscription.Package.Limitations.First(l => l.Name == "Members");
            Assert.True(memberLimit.IsUnlimited);
        }

        [Fact]
        public async Task GetSubscriptionsByUserIdAsync_WithDifferentSubscriptionStatuses_MapsAllStatusesCorrectly()
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

            var package = new Package
            {
                Id = Guid.NewGuid(),
                Name = "Test Package",
                Description = "Test package",
                Price = 100000,
                BillingCycle = 1,
                Currency = "VND",
                Limitations = new List<Limitation>()
            };

            var subscription1 = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PackageId = package.Id,
                TotalPrice = 100000,
                IsActive = true,
                Status = "ACTIVE",
                PaymentMethod = "PayOS",
                TransactionID = "TXN001",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(30),
                PaidAt = DateTime.UtcNow.AddDays(-30),
                User = user,
                Package = package
            };

            var subscription2 = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PackageId = package.Id,
                TotalPrice = 100000,
                IsActive = false,
                Status = "EXPIRED",
                PaymentMethod = "PayOS",
                TransactionID = "TXN002",
                StartDate = DateTime.UtcNow.AddDays(-90),
                EndDate = DateTime.UtcNow.AddDays(-30),
                PaidAt = DateTime.UtcNow.AddDays(-90),
                User = user,
                Package = package
            };

            var subscription3 = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PackageId = package.Id,
                TotalPrice = 100000,
                IsActive = false,
                Status = "PENDING",
                PaymentMethod = "",
                TransactionID = "TXN003",
                StartDate = null,
                EndDate = null,
                PaidAt = null,
                User = user,
                Package = package
            };

            var subscriptions = new List<Subscription> { subscription1, subscription2, subscription3 };

            _mockSubscriptionRepository
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(subscriptions);

            // Act
            var result = await _subscriptionService.GetSubscriptionsByUserIdAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3, result.Data.Count());

            var resultList = result.Data.ToList();
            
            Assert.Equal("ACTIVE", resultList[0].Status);
            Assert.True(resultList[0].IsActive);
            
            Assert.Equal("EXPIRED", resultList[1].Status);
            Assert.False(resultList[1].IsActive);
            
            Assert.Equal("PENDING", resultList[2].Status);
            Assert.False(resultList[2].IsActive);
        }

        #endregion
    }
}
