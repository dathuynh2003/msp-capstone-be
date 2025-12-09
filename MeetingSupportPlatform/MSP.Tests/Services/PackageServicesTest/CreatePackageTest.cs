using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using MSP.Application.Models.Requests.Package;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Package;
using MSP.Application.Services.Interfaces.Package;
using MSP.Domain.Entities;
using Xunit;

namespace MSP.Tests.Services.PackageServicesTest
{
    public class CreatePackageTest
    {
        private readonly Mock<IPackageRepository> _mockPackageRepository;
        private readonly Mock<ILimitationRepository> _mockLimitationRepository;
        private readonly IPackageService _packageService;

        public CreatePackageTest()
        {
            _mockPackageRepository = new Mock<IPackageRepository>();
            _mockLimitationRepository = new Mock<ILimitationRepository>();

            _packageService = new PackageService(
                _mockPackageRepository.Object,
                _mockLimitationRepository.Object
            );
        }

        [Fact]
        public async Task CreateAsync_WithValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var createdById = Guid.NewGuid();
            var limitationId1 = Guid.NewGuid();
            var limitationId2 = Guid.NewGuid();

            var request = new CreatePackageRequest
            {
                Name = "Premium Package",
                Description = "Premium features for power users",
                Price = 100000,
                Currency = "VND",
                CreatedById = createdById,
                LimitationIds = new List<Guid> { limitationId1, limitationId2 }
            };

            var limitations = new List<Limitation>
            {
                new Limitation
                {
                    Id = limitationId1,
                    Name = "Max Projects",
                    Description = "Maximum number of projects",
                    LimitationType = "NumberProject",
                    IsUnlimited = false,
                    LimitValue = 10,
                    LimitUnit = "projects"
                },
                new Limitation
                {
                    Id = limitationId2,
                    Name = "Max Members",
                    Description = "Maximum organization members",
                    LimitationType = "NumberMemberInOrganization",
                    IsUnlimited = false,
                    LimitValue = 50,
                    LimitUnit = "members"
                }
            };

            _mockLimitationRepository
                .Setup(x => x.GetByIdsAsync(request.LimitationIds))
                .ReturnsAsync(limitations);

            _mockPackageRepository
                .Setup(x => x.AddAsync(It.IsAny<MSP.Domain.Entities.Package>()))
                .Returns((Task<Package>)Task.CompletedTask);

            _mockPackageRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _packageService.CreateAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Package created successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(request.Name, result.Data.Name);
            Assert.Equal(request.Description, result.Data.Description);
            Assert.Equal(request.Price, result.Data.Price);
            Assert.Equal(request.Currency, result.Data.Currency);
            Assert.Equal(request.BillingCycle, result.Data.BillingCycle);
            Assert.Equal(2, result.Data.Limitations.Count);

            _mockLimitationRepository.Verify(x => x.GetByIdsAsync(request.LimitationIds), Times.Once);
            _mockPackageRepository.Verify(x => x.AddAsync(It.IsAny<MSP.Domain.Entities.Package>()), Times.Once);
            _mockPackageRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithNoLimitations_ReturnsSuccessResponse()
        {
            // Arrange
            var createdById = Guid.NewGuid();

            var request = new CreatePackageRequest
            {
                Name = "Basic Package",
                Description = "Basic features",
                Price = 0,
                Currency = "VND",
                CreatedById = createdById,
                LimitationIds = new List<Guid>()
            };

            _mockPackageRepository
                .Setup(x => x.AddAsync(It.IsAny<MSP.Domain.Entities.Package>()))
                .Returns((Task<Package>)Task.CompletedTask);

            _mockPackageRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _packageService.CreateAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(request.Name, result.Data.Name);
            Assert.Empty(result.Data.Limitations);

            _mockLimitationRepository.Verify(x => x.GetByIdsAsync(It.IsAny<List<Guid>>()), Times.Never);
            _mockPackageRepository.Verify(x => x.AddAsync(It.IsAny<MSP.Domain.Entities.Package>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithNullLimitationIds_ReturnsSuccessResponse()
        {
            // Arrange
            var createdById = Guid.NewGuid();

            var request = new CreatePackageRequest
            {
                Name = "Standard Package",
                Description = "Standard features",
                Price = 49000,
                Currency = "VND",
                CreatedById = createdById,
                LimitationIds = null
            };

            _mockPackageRepository
                .Setup(x => x.AddAsync(It.IsAny<MSP.Domain.Entities.Package>()))
                .Returns((Task<Package>)Task.CompletedTask);

            _mockPackageRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _packageService.CreateAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(request.Name, result.Data.Name);
            Assert.Empty(result.Data.Limitations);

            _mockLimitationRepository.Verify(x => x.GetByIdsAsync(It.IsAny<List<Guid>>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_SetsCorrectTimestamps()
        {
            // Arrange
            var beforeTest = DateTime.UtcNow;
            var request = new CreatePackageRequest
            {
                Name = "Test Package",
                Description = "Test",
                Price = 10000,
                Currency = "VND",
                CreatedById = Guid.NewGuid(),
                LimitationIds = new List<Guid>()
            };

            MSP.Domain.Entities.Package? capturedPackage = null;

            _mockPackageRepository
                .Setup(x => x.AddAsync(It.IsAny<MSP.Domain.Entities.Package>()))
                .Callback<MSP.Domain.Entities.Package>(p => capturedPackage = p)
                .Returns((Task<Package>)Task.CompletedTask);

            _mockPackageRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _packageService.CreateAsync(request);
            var afterTest = DateTime.UtcNow;

            // Assert
            Assert.NotNull(capturedPackage);
            Assert.True(capturedPackage.CreatedAt >= beforeTest && capturedPackage.CreatedAt <= afterTest);
            Assert.True(capturedPackage.UpdatedAt >= beforeTest && capturedPackage.UpdatedAt <= afterTest);
            Assert.Equal(capturedPackage.CreatedAt, capturedPackage.UpdatedAt);
        }

        [Fact]
        public async Task CreateAsync_GeneratesNewPackageId()
        {
            // Arrange
            var request = new CreatePackageRequest
            {
                Name = "Test Package",
                Description = "Test",
                Price = 10000,
                Currency = "VND",
                CreatedById = Guid.NewGuid(),
                LimitationIds = new List<Guid>()
            };

            MSP.Domain.Entities.Package? capturedPackage = null;

            _mockPackageRepository
                .Setup(x => x.AddAsync(It.IsAny<MSP.Domain.Entities.Package>()))
                .Callback<MSP.Domain.Entities.Package>(p => capturedPackage = p)
                .Returns((Task<Package>)Task.CompletedTask);

            _mockPackageRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _packageService.CreateAsync(request);

            // Assert
            Assert.NotNull(capturedPackage);
            Assert.NotEqual(Guid.Empty, capturedPackage.Id);
            Assert.NotEqual(Guid.Empty, result.Data.Id);
        }

        [Fact]
        public async Task CreateAsync_WithMultipleLimitations_MapsLimitationsCorrectly()
        {
            // Arrange
            var limitationIds = new List<Guid> 
            { 
                Guid.NewGuid(), 
                Guid.NewGuid(), 
                Guid.NewGuid() 
            };

            var request = new CreatePackageRequest
            {
                Name = "Enterprise Package",
                Description = "Enterprise features",
                Price = 199000,
                Currency = "VND",
                CreatedById = Guid.NewGuid(),
                LimitationIds = limitationIds
            };

            var limitations = new List<Limitation>
            {
                new Limitation
                {
                    Id = limitationIds[0],
                    Name = "Limitation 1",
                    Description = "Description 1",
                    LimitationType = "NumberProject",
                    IsUnlimited = false,
                    LimitValue = 100,
                    LimitUnit = "projects",
                    IsDeleted = false
                },
                new Limitation
                {
                    Id = limitationIds[1],
                    Name = "Limitation 2",
                    Description = "Description 2",
                    LimitationType = "NumberMemberInOrganization",
                    IsUnlimited = true,
                    LimitValue = null,
                    LimitUnit = "members",
                    IsDeleted = false
                },
                new Limitation
                {
                    Id = limitationIds[2],
                    Name = "Limitation 3",
                    Description = "Description 3",
                    LimitationType = "NumberMeeting",
                    IsUnlimited = false,
                    LimitValue = 500,
                    LimitUnit = "meetings",
                    IsDeleted = false
                }
            };

            _mockLimitationRepository
                .Setup(x => x.GetByIdsAsync(request.LimitationIds))
                .ReturnsAsync(limitations);

            _mockPackageRepository
                .Setup(x => x.AddAsync(It.IsAny<MSP.Domain.Entities.Package>()))
                .Returns((Task<Package>)Task.CompletedTask);

            _mockPackageRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _packageService.CreateAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3, result.Data.Limitations.Count);

            var limitation1 = result.Data.Limitations.First(l => l.Id == limitationIds[0]);
            Assert.Equal("Limitation 1", limitation1.Name);
            Assert.Equal(100, limitation1.LimitValue);
            Assert.False(limitation1.IsUnlimited);

            var limitation2 = result.Data.Limitations.First(l => l.Id == limitationIds[1]);
            Assert.True(limitation2.IsUnlimited);
            Assert.Null(limitation2.LimitValue);

            var limitation3 = result.Data.Limitations.First(l => l.Id == limitationIds[2]);
            Assert.Equal(500, limitation3.LimitValue);
        }

        [Fact]
        public async Task CreateAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var request = new CreatePackageRequest
            {
                Name = "Test Package",
                Description = "Test",
                Price = 10000,
                Currency = "VND",
                CreatedById = Guid.NewGuid(),
                LimitationIds = new List<Guid>()
            };

            _mockPackageRepository
                .Setup(x => x.AddAsync(It.IsAny<MSP.Domain.Entities.Package>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _packageService.CreateAsync(request));
        }
    }
}
