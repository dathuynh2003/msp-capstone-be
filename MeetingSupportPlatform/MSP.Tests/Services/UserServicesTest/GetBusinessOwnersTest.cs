using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using MSP.Application.Abstracts;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Users;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Domain.Entities;
using Xunit;

namespace MSP.Tests.Services.UserServicesTest
{
    public class GetBusinessOwnersTest
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

        public GetBusinessOwnersTest()
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

        #region TC_GetBO_01 - Success with multiple business owners
        [Fact]
        public async Task GetBusinessOwnersAsync_HasBusinessOwners_ReturnsSuccessWithList()
        {
            // Arrange
            var businessOwners = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "owner1@example.com",
                    FullName = "John Owner",
                    PhoneNumber = "0123456789",
                    Organization = "ABC Corp",
                    BusinessLicense = "BL001",
                    IsApproved = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    AvatarUrl = "avatar1.png"
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "owner2@example.com",
                    FullName = "Jane Business",
                    PhoneNumber = "0987654321",
                    Organization = "XYZ Ltd",
                    BusinessLicense = "BL002",
                    IsApproved = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    AvatarUrl = "avatar2.png"
                }
            };

            _mockUserRepository
                .Setup(x => x.GetBusinessOwnersAsync())
                .ReturnsAsync(businessOwners);

            // Act
            var result = await _userService.GetBusinessOwnersAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Business owners retrieved successfully.", result.Message);
            Assert.NotNull(result.Data);

            var dataList = result.Data.ToList();
            Assert.Equal(2, dataList.Count);

            // Verify first owner
            var owner1 = dataList[0];
            Assert.Equal("owner1@example.com", owner1.Email);
            Assert.Equal("John Owner", owner1.FullName);
            Assert.Equal("ABC Corp", owner1.Organization);
            Assert.Equal("BL001", owner1.BusinessLicense);
            Assert.True(owner1.IsApproved);
            Assert.True(owner1.IsActive);

            // Verify second owner
            var owner2 = dataList[1];
            Assert.Equal("owner2@example.com", owner2.Email);
            Assert.Equal("Jane Business", owner2.FullName);
            Assert.Equal("XYZ Ltd", owner2.Organization);
            Assert.Equal("BL002", owner2.BusinessLicense);
            Assert.False(owner2.IsApproved);
            Assert.True(owner2.IsActive);

            _mockUserRepository.Verify(x => x.GetBusinessOwnersAsync(), Times.Once);
        }
        #endregion

        #region TC_GetBO_02 - Success with empty list
        [Fact]
        public async Task GetBusinessOwnersAsync_NoBusinessOwners_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var emptyList = new List<User>();

            _mockUserRepository
                .Setup(x => x.GetBusinessOwnersAsync())
                .ReturnsAsync(emptyList);

            // Act
            var result = await _userService.GetBusinessOwnersAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Business owners retrieved successfully.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);

            _mockUserRepository.Verify(x => x.GetBusinessOwnersAsync(), Times.Once);
        }
        #endregion

        #region TC_GetBO_03 - Success with single business owner
        [Fact]
        public async Task GetBusinessOwnersAsync_SingleBusinessOwner_ReturnsSuccessWithOneItem()
        {
            // Arrange
            var businessOwners = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "single@example.com",
                    FullName = "Single Owner",
                    PhoneNumber = "0111222333",
                    Organization = "Single Corp",
                    BusinessLicense = "BL999",
                    IsApproved = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    AvatarUrl = null
                }
            };

            _mockUserRepository
                .Setup(x => x.GetBusinessOwnersAsync())
                .ReturnsAsync(businessOwners);

            // Act
            var result = await _userService.GetBusinessOwnersAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);

            var owner = result.Data.Single();
            Assert.Equal("single@example.com", owner.Email);
            Assert.Equal("Single Owner", owner.FullName);
            Assert.Null(owner.AvatarUrl);

            _mockUserRepository.Verify(x => x.GetBusinessOwnersAsync(), Times.Once);
        }
        #endregion

        #region TC_GetBO_04 - Repository throws exception
        [Fact]
        public async Task GetBusinessOwnersAsync_RepositoryThrowsException_ReturnsError()
        {
            // Arrange
            _mockUserRepository
                .Setup(x => x.GetBusinessOwnersAsync())
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _userService.GetBusinessOwnersAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Error retrieving business owners", result.Message);
            Assert.Contains("Database connection failed", result.Message);
            Assert.Null(result.Data);

            _mockUserRepository.Verify(x => x.GetBusinessOwnersAsync(), Times.Once);
        }
        #endregion

        #region TC_GetBO_05 - Verify mapping of all fields
        [Fact]
        public async Task GetBusinessOwnersAsync_ValidData_MapsAllFieldsCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var createdDate = DateTime.UtcNow.AddMonths(-1);

            var businessOwners = new List<User>
            {
                new User
                {
                    Id = userId,
                    Email = "test@example.com",
                    FullName = "Test Owner",
                    PhoneNumber = "0123456789",
                    Organization = "Test Org",
                    BusinessLicense = "BL123",
                    IsApproved = true,
                    IsActive = false,
                    CreatedAt = createdDate,
                    AvatarUrl = "http://example.com/avatar.jpg"
                }
            };

            _mockUserRepository
                .Setup(x => x.GetBusinessOwnersAsync())
                .ReturnsAsync(businessOwners);

            // Act
            var result = await _userService.GetBusinessOwnersAsync();

            // Assert
            Assert.True(result.Success);
            var owner = result.Data.Single();

            // Verify all fields are mapped correctly
            Assert.Equal(userId, owner.Id);
            Assert.Equal("test@example.com", owner.Email);
            Assert.Equal("Test Owner", owner.FullName);
            Assert.Equal("0123456789", owner.PhoneNumber);
            Assert.Equal("Test Org", owner.Organization);
            Assert.Equal("BL123", owner.BusinessLicense);
            Assert.True(owner.IsApproved);
            Assert.False(owner.IsActive);
            Assert.Equal(createdDate, owner.CreatedAt);
            Assert.Equal("http://example.com/avatar.jpg", owner.AvatarUrl);
        }
        #endregion

        #region TC_GetBO_06 - Repository returns null (edge case)
        [Fact]
        public async Task GetBusinessOwnersAsync_RepositoryReturnsNull_ThrowsException()
        {
            // Arrange
            _mockUserRepository
                .Setup(x => x.GetBusinessOwnersAsync())
                .ReturnsAsync((IEnumerable<User>)null);

            // Act
            var result = await _userService.GetBusinessOwnersAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Error retrieving business owners", result.Message);
        }
        #endregion

        #region TC_GetBO_07 - Large list performance test
        [Fact]
        public async Task GetBusinessOwnersAsync_LargeList_HandlesCorrectly()
        {
            // Arrange
            var largeList = new List<User>();
            for (int i = 0; i < 100; i++)
            {
                largeList.Add(new User
                {
                    Id = Guid.NewGuid(),
                    Email = $"owner{i}@example.com",
                    FullName = $"Owner {i}",
                    Organization = $"Org {i}",
                    BusinessLicense = $"BL{i:000}",
                    IsApproved = i % 2 == 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-i)
                });
            }

            _mockUserRepository
                .Setup(x => x.GetBusinessOwnersAsync())
                .ReturnsAsync(largeList);

            // Act
            var result = await _userService.GetBusinessOwnersAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(100, result.Data.Count());

            _mockUserRepository.Verify(x => x.GetBusinessOwnersAsync(), Times.Once);
        }
        #endregion

        #region TC_GetBO_08 - Server or DB error
        [Fact]
        public async Task GetBusinessOwnersAsync_ServerOrDbError_ReturnsErrorResponse()
        {
            // Arrange
            _mockUserRepository
                .Setup(x => x.GetBusinessOwnersAsync())
                .ThrowsAsync(new Exception("Connection timeout: Unable to connect to database server"));

            // Act
            var result = await _userService.GetBusinessOwnersAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Error retrieving business owners", result.Message);
            Assert.Contains("Connection timeout", result.Message);
            Assert.Null(result.Data);

            _mockUserRepository.Verify(x => x.GetBusinessOwnersAsync(), Times.Once);
        }
        #endregion

    }
}
