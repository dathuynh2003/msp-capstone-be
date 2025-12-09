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
    public class EditPackageTest
    {
        private readonly Mock<IPackageRepository> _mockPackageRepository;
        private readonly Mock<ILimitationRepository> _mockLimitationRepository;
        private readonly IPackageService _packageService;

        public EditPackageTest()
        {
            _mockPackageRepository = new Mock<IPackageRepository>();
            _mockLimitationRepository = new Mock<ILimitationRepository>();

            _packageService = new PackageService(
                _mockPackageRepository.Object,
                _mockLimitationRepository.Object
            );
        }

        [Fact]
        public async Task UpdateAsync_WithValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var packageId = Guid.NewGuid();
            var createdById = Guid.NewGuid();
            var limitationId1 = Guid.NewGuid();
            var limitationId2 = Guid.NewGuid();

            var request = new UpdatePackageRequest
            {
                Name = "Updated Premium Package",
                Description = "Updated premium features",
                Price = 149000,
                Currency = "VND",
                CreatedById = createdById,
                LimitationIds = new List<Guid> { limitationId1, limitationId2 }
            };

            var existingPackage = new MSP.Domain.Entities.Package
            {
                Id = packageId,
                Name = "Old Name",
                Description = "Old Description",
                Price = 99000,
                Currency = "VND",
                CreatedById = createdById,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-30),
                IsDeleted = false,
                Limitations = new List<Limitation>()
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
                    LimitValue = 20,
                    LimitUnit = "projects",
                    IsDeleted = false
                },
                new Limitation
                {
                    Id = limitationId2,
                    Name = "Max Members",
                    Description = "Maximum organization members",
                    LimitationType = "NumberMemberInOrganization",
                    IsUnlimited = false,
                    LimitValue = 100,
                    LimitUnit = "members",
                    IsDeleted = false
                }
            };

            _mockPackageRepository
                .Setup(x => x.GetPackageByIdAsync(packageId))
                .ReturnsAsync(existingPackage);

            _mockLimitationRepository
                .Setup(x => x.GetByIdsAsync(request.LimitationIds))
                .ReturnsAsync(limitations);

            _mockPackageRepository
                .Setup(x => x.UpdateAsync(It.IsAny<MSP.Domain.Entities.Package>()))
                .Returns(Task.CompletedTask);

            _mockPackageRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _packageService.UpdateAsync(packageId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Package updated successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(request.Name, result.Data.Name);
            Assert.Equal(request.Description, result.Data.Description);
            Assert.Equal(request.Price, result.Data.Price);
            Assert.Equal(request.Currency, result.Data.Currency);
            Assert.Equal(request.BillingCycle, result.Data.BillingCycle);
            Assert.Equal(2, result.Data.Limitations.Count);

            _mockPackageRepository.Verify(x => x.GetPackageByIdAsync(packageId), Times.Once);
            _mockLimitationRepository.Verify(x => x.GetByIdsAsync(request.LimitationIds), Times.Once);
            _mockPackageRepository.Verify(x => x.UpdateAsync(It.IsAny<MSP.Domain.Entities.Package>()), Times.Once);
            _mockPackageRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentPackage_ReturnsErrorResponse()
        {
            // Arrange
            var packageId = Guid.NewGuid();

            var request = new UpdatePackageRequest
            {
                Name = "Updated Package",
                Description = "Updated",
                Price = 99000,
                Currency = "VND",
                CreatedById = Guid.NewGuid(),
                LimitationIds = new List<Guid>()
            };

            _mockPackageRepository
                .Setup(x => x.GetPackageByIdAsync(packageId))
                .ReturnsAsync((MSP.Domain.Entities.Package?)null);

            // Act
            var result = await _packageService.UpdateAsync(packageId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Package not found", result.Message);
            Assert.Null(result.Data);

            _mockPackageRepository.Verify(x => x.GetPackageByIdAsync(packageId), Times.Once);
            _mockPackageRepository.Verify(x => x.UpdateAsync(It.IsAny<MSP.Domain.Entities.Package>()), Times.Never);
            _mockPackageRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WithDeletedPackage_ReturnsErrorResponse()
        {
            // Arrange
            var packageId = Guid.NewGuid();

            var request = new UpdatePackageRequest
            {
                Name = "Updated Package",
                Description = "Updated",
                Price = 99000,
                Currency = "VND",
                CreatedById = Guid.NewGuid(),
                LimitationIds = new List<Guid>()
            };

            var deletedPackage = new MSP.Domain.Entities.Package
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
            var result = await _packageService.UpdateAsync(packageId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Package not found", result.Message);
            Assert.Null(result.Data);

            _mockPackageRepository.Verify(x => x.UpdateAsync(It.IsAny<MSP.Domain.Entities.Package>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ClearsAndReplacesLimitations()
        {
            // Arrange
            var packageId = Guid.NewGuid();
            var oldLimitationId = Guid.NewGuid();
            var newLimitationId = Guid.NewGuid();

            var request = new UpdatePackageRequest
            {
                Name = "Updated Package",
                Description = "Updated",
                Price = 99000,
                Currency = "VND",
                CreatedById = Guid.NewGuid(),
                LimitationIds = new List<Guid> { newLimitationId }
            };

            var oldLimitation = new Limitation
            {
                Id = oldLimitationId,
                Name = "Old Limitation",
                LimitationType = "NumberProject",
                IsDeleted = false
            };

            var existingPackage = new MSP.Domain.Entities.Package
            {
                Id = packageId,
                Name = "Old Package",
                IsDeleted = false,
                Limitations = new List<Limitation> { oldLimitation }
            };

            var newLimitation = new Limitation
            {
                Id = newLimitationId,
                Name = "New Limitation",
                LimitationType = "NumberMemberInOrganization",
                IsDeleted = false
            };

            _mockPackageRepository
                .Setup(x => x.GetPackageByIdAsync(packageId))
                .ReturnsAsync(existingPackage);

            _mockLimitationRepository
                .Setup(x => x.GetByIdsAsync(request.LimitationIds))
                .ReturnsAsync(new List<Limitation> { newLimitation });

            _mockPackageRepository
                .Setup(x => x.UpdateAsync(It.IsAny<MSP.Domain.Entities.Package>()))
                .Returns(Task.CompletedTask);

            _mockPackageRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _packageService.UpdateAsync(packageId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data.Limitations);
            Assert.Equal(newLimitationId, result.Data.Limitations.First().Id);
            Assert.Equal("New Limitation", result.Data.Limitations.First().Name);
        }

        [Fact]
        public async Task UpdateAsync_WithEmptyLimitationIds_ClearsAllLimitations()
        {
            // Arrange
            var packageId = Guid.NewGuid();
            var limitationId = Guid.NewGuid();

            var request = new UpdatePackageRequest
            {
                Name = "Updated Package",
                Description = "Updated",
                Price = 99000,
                Currency = "VND",
                CreatedById = Guid.NewGuid(),
                LimitationIds = new List<Guid>()
            };

            var existingLimitation = new Limitation
            {
                Id = limitationId,
                Name = "Existing Limitation",
                LimitationType = "NumberProject"
            };

            var existingPackage = new MSP.Domain.Entities.Package
            {
                Id = packageId,
                Name = "Old Package",
                IsDeleted = false,
                Limitations = new List<Limitation> { existingLimitation }
            };

            _mockPackageRepository
                .Setup(x => x.GetPackageByIdAsync(packageId))
                .ReturnsAsync(existingPackage);

            _mockPackageRepository
                .Setup(x => x.UpdateAsync(It.IsAny<MSP.Domain.Entities.Package>()))
                .Returns(Task.CompletedTask);

            _mockPackageRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _packageService.UpdateAsync(packageId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data.Limitations);
            _mockLimitationRepository.Verify(x => x.GetByIdsAsync(It.IsAny<List<Guid>>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesTimestamp()
        {
            // Arrange
            var packageId = Guid.NewGuid();
            var beforeTest = DateTime.UtcNow;

            var request = new UpdatePackageRequest
            {
                Name = "Updated Package",
                Description = "Updated",
                Price = 99000,
                Currency = "VND",
                CreatedById = Guid.NewGuid(),
                LimitationIds = new List<Guid>()
            };

            var oldTimestamp = DateTime.UtcNow.AddDays(-10);
            var existingPackage = new MSP.Domain.Entities.Package
            {
                Id = packageId,
                Name = "Old Package",
                CreatedAt = oldTimestamp,
                UpdatedAt = oldTimestamp,
                IsDeleted = false,
                Limitations = new List<Limitation>()
            };

            MSP.Domain.Entities.Package? capturedPackage = null;

            _mockPackageRepository
                .Setup(x => x.GetPackageByIdAsync(packageId))
                .ReturnsAsync(existingPackage);

            _mockPackageRepository
                .Setup(x => x.UpdateAsync(It.IsAny<MSP.Domain.Entities.Package>()))
                .Callback<MSP.Domain.Entities.Package>(p => capturedPackage = p)
                .Returns(Task.CompletedTask);

            _mockPackageRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _packageService.UpdateAsync(packageId, request);
            var afterTest = DateTime.UtcNow;

            // Assert
            Assert.NotNull(capturedPackage);
            Assert.True(capturedPackage.UpdatedAt >= beforeTest && capturedPackage.UpdatedAt <= afterTest);
            Assert.True(capturedPackage.UpdatedAt > oldTimestamp);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesAllPackageProperties()
        {
            // Arrange
            var packageId = Guid.NewGuid();
            var oldCreatedById = Guid.NewGuid();
            var newCreatedById = Guid.NewGuid();

            var request = new UpdatePackageRequest
            {
                Name = "New Name",
                Description = "New Description",
                Price = 199000,
                Currency = "VND",
                CreatedById = newCreatedById,
                LimitationIds = new List<Guid>()
            };

            var existingPackage = new MSP.Domain.Entities.Package
            {
                Id = packageId,
                Name = "Old Name",
                Description = "Old Description",
                Price = 99000,
                Currency = "VND",
                CreatedById = oldCreatedById,
                IsDeleted = false,
                Limitations = new List<Limitation>()
            };

            MSP.Domain.Entities.Package? capturedPackage = null;

            _mockPackageRepository
                .Setup(x => x.GetPackageByIdAsync(packageId))
                .ReturnsAsync(existingPackage);

            _mockPackageRepository
                .Setup(x => x.UpdateAsync(It.IsAny<MSP.Domain.Entities.Package>()))
                .Callback<MSP.Domain.Entities.Package>(p => capturedPackage = p)
                .Returns(Task.CompletedTask);

            _mockPackageRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _packageService.UpdateAsync(packageId, request);

            // Assert
            Assert.NotNull(capturedPackage);
            Assert.Equal("New Name", capturedPackage.Name);
            Assert.Equal("New Description", capturedPackage.Description);
            Assert.Equal(199.99m, capturedPackage.Price);
            Assert.Equal("EUR", capturedPackage.Currency);
            Assert.Equal(newCreatedById, capturedPackage.CreatedById);
        }
    }
}
