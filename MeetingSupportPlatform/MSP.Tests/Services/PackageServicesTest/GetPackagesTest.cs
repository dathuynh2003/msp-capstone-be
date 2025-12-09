using Moq;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Package;
using MSP.Application.Services.Interfaces.Package;
using MSP.Domain.Entities;
using MSP.Shared.Enums;
using Xunit;

namespace MSP.Tests.Services.PackageServicesTest
{
    public class GetPackagesTest
    {
        private readonly Mock<IPackageRepository> _mockPackageRepository;
        private readonly IPackageService _packageService;
        private readonly Mock<ILimitationRepository> _mockLimitationRepository;

        public GetPackagesTest()
        {
            _mockPackageRepository = new Mock<IPackageRepository>();
            _mockLimitationRepository = new Mock<ILimitationRepository>();
            _packageService = new PackageService(_mockPackageRepository.Object, _mockLimitationRepository.Object);
        }

        #region TC_GetPackages_01 - Success with multiple packages
        [Fact]
        public async Task GetAllAsync_HasPackages_ReturnsSuccessWithList()
        {
            // Arrange
            var packages = new List<Package>
    {
        new Package
        {
            Id = Guid.NewGuid(),
            Name = "Basic Package",
            Description = "Basic plan",
            Price = 10.00m,
            Currency = "USD",
            BillingCycle = 30,
            IsDeleted = false,
            Limitations = new List<Limitation>
            {
                new Limitation
                {
                    Id = Guid.NewGuid(),
                    Name = "Project Limit",
                    LimitationType = LimitationTypeEnum.NumberProject.ToString(),
                    IsUnlimited = false,
                    LimitValue = 5,
                    LimitUnit = "Projects",
                    IsDeleted = false
                },
                new Limitation
                {
                    Id = Guid.NewGuid(),
                    Name = "Member Limit",
                    LimitationType = LimitationTypeEnum.NumberMemberInOrganization.ToString(),
                    IsUnlimited = false,
                    LimitValue = 10,
                    LimitUnit = "Members",
                    IsDeleted = false
                }
            }
        },
        new Package
        {
            Id = Guid.NewGuid(),
            Name = "Premium Package",
            Description = "Premium plan",
            Price = 50.00m,
            Currency = "USD",
            BillingCycle = 30,
            IsDeleted = false,
            Limitations = new List<Limitation>()
        }
    };

            _mockPackageRepository
                .Setup(x => x.GetAll())
                .ReturnsAsync(packages);

            // Act
            var result = await _packageService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);

            // Verify first package
            var package1 = result.Data[0];
            Assert.Equal("Basic Package", package1.Name);
            Assert.Equal(10.00m, package1.Price);
            Assert.Equal("USD", package1.Currency);
            Assert.Equal(30, package1.BillingCycle);
            Assert.False(package1.isDeleted);
            Assert.Equal(2, package1.Limitations.Count);

            var package1Limitations = package1.Limitations.ToList();
            // After ordering: Member Org (order=1) should be first, Project (order=2) second
            Assert.Equal("Member Limit", package1Limitations[0].Name); // Member Org = order 1
            Assert.Equal("Project Limit", package1Limitations[1].Name); // Project = order 2

            var package2 = result.Data[1];
            Assert.Equal("Premium Package", package2.Name);
            Assert.Empty(package2.Limitations);

            _mockPackageRepository.Verify(x => x.GetAll(), Times.Once);
        }
        #endregion

        #region TC_GetPackages_02 - Success with empty list
        [Fact]
        public async Task GetAllAsync_NoPackages_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var emptyList = new List<Package>();

            _mockPackageRepository
                .Setup(x => x.GetAll())
                .ReturnsAsync(emptyList);

            // Act
            var result = await _packageService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);

            _mockPackageRepository.Verify(x => x.GetAll(), Times.Once);
        }
        #endregion

        #region TC_GetPackages_03 - Success with ordered limitations
        [Fact]
        public async Task GetAllAsync_WithLimitations_OrdersCorrectly()
        {
            // Arrange
            var packages = new List<Package>
    {
        new Package
        {
            Id = Guid.NewGuid(),
            Name = "Test Package",
            Price = 20.00m,
            IsDeleted = false,
            Limitations = new List<Limitation>
            {
                new Limitation
                {
                    Id = Guid.NewGuid(),
                    Name = "Meeting Limit",
                    LimitationType = LimitationTypeEnum.NumberMeeting.ToString(), // Order: 4
                    LimitValue = 100,
                    IsDeleted = false
                },
                new Limitation
                {
                    Id = Guid.NewGuid(),
                    Name = "Project Limit",
                    LimitationType = LimitationTypeEnum.NumberProject.ToString(), // Order: 2
                    LimitValue = 10,
                    IsDeleted = false
                },
                new Limitation
                {
                    Id = Guid.NewGuid(),
                    Name = "Member Org Limit",
                    LimitationType = LimitationTypeEnum.NumberMemberInOrganization.ToString(), // Order: 1
                    LimitValue = 50,
                    IsDeleted = false
                },
                new Limitation
                {
                    Id = Guid.NewGuid(),
                    Name = "Member Meeting Limit",
                    LimitationType = LimitationTypeEnum.NumberMemberInMeeting.ToString(), // Order: 5
                    LimitValue = 20,
                    IsDeleted = false
                },
                new Limitation
                {
                    Id = Guid.NewGuid(),
                    Name = "Member Project Limit",
                    LimitationType = LimitationTypeEnum.NumberMemberInProject.ToString(), // Order: 3
                    LimitValue = 15,
                    IsDeleted = false
                }
            }
        }
    };

            _mockPackageRepository
                .Setup(x => x.GetAll())
                .ReturnsAsync(packages);

            // Act
            var result = await _packageService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            var packageResponse = result.Data.Single();
            Assert.Equal(5, packageResponse.Limitations.Count);

            var limitationsList = packageResponse.Limitations.ToList(); // Convert to List first

            Assert.Equal("Member Org Limit", limitationsList[0].Name);
            Assert.Equal("Project Limit", limitationsList[1].Name);
            Assert.Equal("Member Project Limit", limitationsList[2].Name);
            Assert.Equal("Meeting Limit", limitationsList[3].Name);
            Assert.Equal("Member Meeting Limit", limitationsList[4].Name);

            _mockPackageRepository.Verify(x => x.GetAll(), Times.Once);
        }
        #endregion

        #region TC_GetPackages_04 - Success with unlimited limitations
        [Fact]
        public async Task GetAllAsync_WithUnlimitedLimitations_ReturnsCorrectly()
        {
            // Arrange
            var packages = new List<Package>
            {
                new Package
                {
                    Id = Guid.NewGuid(),
                    Name = "Unlimited Package",
                    Price = 100.00m,
                    IsDeleted = false,
                    Limitations = new List<Limitation>
                    {
                        new Limitation
                        {
                            Id = Guid.NewGuid(),
                            Name = "Unlimited Projects",
                            LimitationType = LimitationTypeEnum.NumberProject.ToString(),
                            IsUnlimited = true,
                            LimitValue = null,
                            LimitUnit = "Projects",
                            IsDeleted = false
                        }
                    }
                }
            };

            _mockPackageRepository
                .Setup(x => x.GetAll())
                .ReturnsAsync(packages);

            // Act
            var result = await _packageService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            var packageResponse = result.Data.Single();
            var limitation = packageResponse.Limitations.Single();

            Assert.Equal("Unlimited Projects", limitation.Name);
            Assert.True(limitation.IsUnlimited);
            Assert.Null(limitation.LimitValue);

            _mockPackageRepository.Verify(x => x.GetAll(), Times.Once);
        }
        #endregion

        #region TC_GetPackages_05 - Success with deleted packages
        [Fact]
        public async Task GetAllAsync_WithDeletedPackages_ReturnsAll()
        {
            // Arrange
            var packages = new List<Package>
            {
                new Package
                {
                    Id = Guid.NewGuid(),
                    Name = "Active Package",
                    Price = 10.00m,
                    IsDeleted = false,
                    Limitations = new List<Limitation>()
                },
                new Package
                {
                    Id = Guid.NewGuid(),
                    Name = "Deleted Package",
                    Price = 20.00m,
                    IsDeleted = true,
                    Limitations = new List<Limitation>()
                }
            };

            _mockPackageRepository
                .Setup(x => x.GetAll())
                .ReturnsAsync(packages);

            // Act
            var result = await _packageService.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data.Count);

            Assert.Contains(result.Data, p => p.Name == "Active Package" && !p.isDeleted);
            Assert.Contains(result.Data, p => p.Name == "Deleted Package" && p.isDeleted);

            _mockPackageRepository.Verify(x => x.GetAll(), Times.Once);
        }
        #endregion

    }
}
