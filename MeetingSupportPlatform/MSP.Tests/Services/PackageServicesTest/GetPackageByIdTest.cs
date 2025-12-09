using Moq;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Package;
using MSP.Application.Services.Interfaces.Package;
using MSP.Domain.Entities;
using MSP.Shared.Enums;
using Xunit;

namespace MSP.Tests.Services.PackageServicesTest
{
    public class GetPackageByIdTest
    {
        private readonly Mock<IPackageRepository> _mockPackageRepository;
        private readonly IPackageService _packageService;
        private readonly Mock<ILimitationRepository> _mockLimitationRepository;
        public GetPackageByIdTest()
        {
            _mockPackageRepository = new Mock<IPackageRepository>();
            _mockLimitationRepository = new Mock<ILimitationRepository>();
            _packageService = new PackageService(_mockPackageRepository.Object, _mockLimitationRepository.Object);
        }

        #region TC_GetPkgById_01 - Package not found
        [Fact]
        public async Task GetByIdAsync_PackageNotFound_ReturnsError()
        {
            // Arrange
            var packageId = Guid.NewGuid();

            _mockPackageRepository
                .Setup(x => x.GetPackageByIdAsync(packageId))
                .ReturnsAsync((Package)null);

            // Act
            var result = await _packageService.GetByIdAsync(packageId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Package not found", result.Message);
            Assert.Null(result.Data);

            _mockPackageRepository.Verify(x => x.GetPackageByIdAsync(packageId), Times.Once);
        }
        #endregion

        #region TC_GetPkgById_02 - Package is deleted
        [Fact]
        public async Task GetByIdAsync_PackageIsDeleted_ReturnsError()
        {
            // Arrange
            var packageId = Guid.NewGuid();
            var deletedPackage = new Package
            {
                Id = packageId,
                Name = "Deleted Package",
                IsDeleted = true,
                Limitations = new List<Limitation>()
            };

            _mockPackageRepository
                .Setup(x => x.GetPackageByIdAsync(packageId))
                .ReturnsAsync(deletedPackage);

            // Act
            var result = await _packageService.GetByIdAsync(packageId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Package not found", result.Message);
            Assert.Null(result.Data);

            _mockPackageRepository.Verify(x => x.GetPackageByIdAsync(packageId), Times.Once);
        }
        #endregion

        #region TC_GetPkgById_03 - Success with complete data
        [Fact]
        public async Task GetByIdAsync_ValidPackage_ReturnsSuccess()
        {
            // Arrange
            var packageId = Guid.NewGuid();
            var package = new Package
            {
                Id = packageId,
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
                        Description = "Max projects",
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
                        Description = "Max members",
                        LimitationType = LimitationTypeEnum.NumberMemberInOrganization.ToString(),
                        IsUnlimited = false,
                        LimitValue = 10,
                        LimitUnit = "Members",
                        IsDeleted = false
                    }
                }
            };

            _mockPackageRepository
                .Setup(x => x.GetPackageByIdAsync(packageId))
                .ReturnsAsync(package);

            // Act
            var result = await _packageService.GetByIdAsync(packageId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(packageId, result.Data.Id);
            Assert.Equal("Basic Package", result.Data.Name);
            Assert.Equal("Basic plan", result.Data.Description);
            Assert.Equal(10.00m, result.Data.Price);
            Assert.Equal("USD", result.Data.Currency);
            Assert.Equal(30, result.Data.BillingCycle);
            Assert.False(result.Data.isDeleted);
            Assert.Equal(2, result.Data.Limitations.Count);

            // Verify limitations are ordered (Member Org=1, Project=2)
            var limitationsList = result.Data.Limitations.ToList();
            Assert.Equal("Member Limit", limitationsList[0].Name); // Order: 1
            Assert.Equal("Project Limit", limitationsList[1].Name); // Order: 2

            _mockPackageRepository.Verify(x => x.GetPackageByIdAsync(packageId), Times.Once);
        }
        #endregion

        #region TC_GetPkgById_04 - Success with ordered limitations
        [Fact]
        public async Task GetByIdAsync_WithMultipleLimitations_OrdersCorrectly()
        {
            // Arrange
            var packageId = Guid.NewGuid();
            var package = new Package
            {
                Id = packageId,
                Name = "Complete Package",
                Price = 50.00m,
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
                        Name = "Member Meeting Limit",
                        LimitationType = LimitationTypeEnum.NumberMemberInMeeting.ToString(), // Order: 5
                        LimitValue = 20,
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
                        Name = "Member Project Limit",
                        LimitationType = LimitationTypeEnum.NumberMemberInProject.ToString(), // Order: 3
                        LimitValue = 15,
                        IsDeleted = false
                    }
                }
            };

            _mockPackageRepository
                .Setup(x => x.GetPackageByIdAsync(packageId))
                .ReturnsAsync(package);

            // Act
            var result = await _packageService.GetByIdAsync(packageId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(5, result.Data.Limitations.Count);

            var limitationsList = result.Data.Limitations.ToList();
            // Verify order: 1 -> 2 -> 3 -> 4 -> 5
            Assert.Equal("Member Org Limit", limitationsList[0].Name);
            Assert.Equal("Project Limit", limitationsList[1].Name);
            Assert.Equal("Member Project Limit", limitationsList[2].Name);
            Assert.Equal("Meeting Limit", limitationsList[3].Name);
            Assert.Equal("Member Meeting Limit", limitationsList[4].Name);

            _mockPackageRepository.Verify(x => x.GetPackageByIdAsync(packageId), Times.Once);
        }
        #endregion

        #region TC_GetPkgById_05 - Success with unlimited limitations
        [Fact]
        public async Task GetByIdAsync_WithUnlimitedLimitations_ReturnsCorrectly()
        {
            // Arrange
            var packageId = Guid.NewGuid();
            var package = new Package
            {
                Id = packageId,
                Name = "Unlimited Package",
                Price = 100.00m,
                IsDeleted = false,
                Limitations = new List<Limitation>
                {
                    new Limitation
                    {
                        Id = Guid.NewGuid(),
                        Name = "Unlimited Projects",
                        Description = "No project limit",
                        LimitationType = LimitationTypeEnum.NumberProject.ToString(),
                        IsUnlimited = true,
                        LimitValue = null,
                        LimitUnit = "Projects",
                        IsDeleted = false
                    }
                }
            };

            _mockPackageRepository
                .Setup(x => x.GetPackageByIdAsync(packageId))
                .ReturnsAsync(package);

            // Act
            var result = await _packageService.GetByIdAsync(packageId);

            // Assert
            Assert.True(result.Success);
            var limitation = result.Data.Limitations.Single();
            Assert.Equal("Unlimited Projects", limitation.Name);
            Assert.True(limitation.IsUnlimited);
            Assert.Null(limitation.LimitValue);

            _mockPackageRepository.Verify(x => x.GetPackageByIdAsync(packageId), Times.Once);
        }
        #endregion

        #region TC_GetPkgById_06 - Success with no limitations
        [Fact]
        public async Task GetByIdAsync_PackageWithoutLimitations_ReturnsSuccess()
        {
            // Arrange
            var packageId = Guid.NewGuid();
            var package = new Package
            {
                Id = packageId,
                Name = "Simple Package",
                Price = 5.00m,
                IsDeleted = false,
                Limitations = new List<Limitation>()
            };

            _mockPackageRepository
                .Setup(x => x.GetPackageByIdAsync(packageId))
                .ReturnsAsync(package);

            // Act
            var result = await _packageService.GetByIdAsync(packageId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Simple Package", result.Data.Name);
            Assert.Empty(result.Data.Limitations);

            _mockPackageRepository.Verify(x => x.GetPackageByIdAsync(packageId), Times.Once);
        }
        #endregion

        #region TC_GetPkgById_07 - Empty Guid
        [Fact]
        public async Task GetByIdAsync_EmptyGuid_ReturnsNotFound()
        {
            // Arrange
            var packageId = Guid.Empty;

            _mockPackageRepository
                .Setup(x => x.GetPackageByIdAsync(packageId))
                .ReturnsAsync((Package)null);

            // Act
            var result = await _packageService.GetByIdAsync(packageId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Package not found", result.Message);
            Assert.Null(result.Data);

            _mockPackageRepository.Verify(x => x.GetPackageByIdAsync(Guid.Empty), Times.Once);
        }
        #endregion

        #region TC_GetPkgById_08 - Verify all fields mapped
        [Fact]
        public async Task GetByIdAsync_ValidPackage_MapsAllFieldsCorrectly()
        {
            // Arrange
            var packageId = Guid.NewGuid();
            var limitationId = Guid.NewGuid();

            var package = new Package
            {
                Id = packageId,
                Name = "Test Package",
                Description = "Test description",
                Price = 25.99m,
                Currency = "EUR",
                BillingCycle = 90,
                IsDeleted = false,
                Limitations = new List<Limitation>
                {
                    new Limitation
                    {
                        Id = limitationId,
                        Name = "Test Limitation",
                        Description = "Limitation desc",
                        LimitationType = LimitationTypeEnum.NumberProject.ToString(),
                        IsUnlimited = false,
                        LimitValue = 20,
                        LimitUnit = "Projects",
                        IsDeleted = false
                    }
                }
            };

            _mockPackageRepository
                .Setup(x => x.GetPackageByIdAsync(packageId))
                .ReturnsAsync(package);

            // Act
            var result = await _packageService.GetByIdAsync(packageId);

            // Assert
            Assert.True(result.Success);

            // Verify package fields
            Assert.Equal(packageId, result.Data.Id);
            Assert.Equal("Test Package", result.Data.Name);
            Assert.Equal("Test description", result.Data.Description);
            Assert.Equal(25.99m, result.Data.Price);
            Assert.Equal("EUR", result.Data.Currency);
            Assert.Equal(90, result.Data.BillingCycle);
            Assert.False(result.Data.isDeleted);

            // Verify limitation fields
            var limitation = result.Data.Limitations.Single();
            Assert.Equal(limitationId, limitation.Id);
            Assert.Equal("Test Limitation", limitation.Name);
            Assert.Equal("Limitation desc", limitation.Description);
            Assert.False(limitation.IsUnlimited);
            Assert.Equal(20, limitation.LimitValue);
            Assert.Equal("Projects", limitation.LimitUnit);
            Assert.False(limitation.IsDeleted);

            _mockPackageRepository.Verify(x => x.GetPackageByIdAsync(packageId), Times.Once);
        }
        #endregion
    }
}
