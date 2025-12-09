using System;
using System.Threading.Tasks;
using Moq;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Package;
using MSP.Application.Services.Interfaces.Package;
using MSP.Domain.Entities;
using Xunit;

namespace MSP.Tests.Services.PackageServicesTest
{
    public class DeletePackageTest
    {
        private readonly Mock<IPackageRepository> _mockPackageRepository;
        private readonly Mock<ILimitationRepository> _mockLimitationRepository;
        private readonly IPackageService _packageService;

        public DeletePackageTest()
        {
            _mockPackageRepository = new Mock<IPackageRepository>();
            _mockLimitationRepository = new Mock<ILimitationRepository>();

            _packageService = new PackageService(
                _mockPackageRepository.Object,
                _mockLimitationRepository.Object
            );
        }

        [Fact]
        public async Task DeleteAsync_WithValidPackageId_ReturnsSuccessResponse()
        {
            // Arrange
            var packageId = Guid.NewGuid();

            var existingPackage = new MSP.Domain.Entities.Package
            {
                Id = packageId,
                Name = "Package To Delete",
                Description = "This package will be deleted",
                Price = 99000,
                Currency = "VND",
                IsDeleted = false
            };

            _mockPackageRepository
                .Setup(x => x.GetByIdAsync(packageId))
                .ReturnsAsync(existingPackage);

            _mockPackageRepository
                .Setup(x => x.SoftDeleteAsync(It.IsAny<MSP.Domain.Entities.Package>()))
                .Returns(Task.CompletedTask);

            _mockPackageRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _packageService.DeleteAsync(packageId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Package deleted successfully", result.Message);

            _mockPackageRepository.Verify(x => x.GetByIdAsync(packageId), Times.Once);
            _mockPackageRepository.Verify(x => x.SoftDeleteAsync(existingPackage), Times.Once);
            _mockPackageRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentPackageId_ReturnsErrorResponse()
        {
            // Arrange
            var packageId = Guid.NewGuid();

            _mockPackageRepository
                .Setup(x => x.GetByIdAsync(packageId))
                .ReturnsAsync((MSP.Domain.Entities.Package?)null);

            // Act
            var result = await _packageService.DeleteAsync(packageId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Package not found", result.Message);
            Assert.Null(result.Data);

            _mockPackageRepository.Verify(x => x.GetByIdAsync(packageId), Times.Once);
            _mockPackageRepository.Verify(x => x.SoftDeleteAsync(It.IsAny<MSP.Domain.Entities.Package>()), Times.Never);
            _mockPackageRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WithAlreadyDeletedPackage_ReturnsErrorResponse()
        {
            // Arrange
            var packageId = Guid.NewGuid();

            var deletedPackage = new MSP.Domain.Entities.Package
            {
                Id = packageId,
                Name = "Already Deleted Package",
                Description = "This package is already deleted",
                Price = 99000,
                Currency = "VND",
                IsDeleted = true
            };

            _mockPackageRepository
                .Setup(x => x.GetByIdAsync(packageId))
                .ReturnsAsync(deletedPackage);

            // Act
            var result = await _packageService.DeleteAsync(packageId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Package not found", result.Message);
            Assert.Null(result.Data);

            _mockPackageRepository.Verify(x => x.GetByIdAsync(packageId), Times.Once);
            _mockPackageRepository.Verify(x => x.SoftDeleteAsync(It.IsAny<MSP.Domain.Entities.Package>()), Times.Never);
            _mockPackageRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_CallsSoftDeleteNotHardDelete()
        {
            // Arrange
            var packageId = Guid.NewGuid();

            var existingPackage = new MSP.Domain.Entities.Package
            {
                Id = packageId,
                Name = "Package To Delete",
                IsDeleted = false
            };

            _mockPackageRepository
                .Setup(x => x.GetByIdAsync(packageId))
                .ReturnsAsync(existingPackage);

            _mockPackageRepository
                .Setup(x => x.SoftDeleteAsync(It.IsAny<MSP.Domain.Entities.Package>()))
                .Returns(Task.CompletedTask);

            _mockPackageRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _packageService.DeleteAsync(packageId);

            // Assert
            // Verify that SoftDeleteAsync is called (not a hard delete method)
            _mockPackageRepository.Verify(x => x.SoftDeleteAsync(existingPackage), Times.Once);

            // Verify that hard delete methods are NOT called
            _mockPackageRepository.Verify(x => x.SoftDeleteAsync(It.IsAny<MSP.Domain.Entities.Package>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var packageId = Guid.NewGuid();

            var existingPackage = new MSP.Domain.Entities.Package
            {
                Id = packageId,
                Name = "Package",
                IsDeleted = false
            };

            _mockPackageRepository
                .Setup(x => x.GetByIdAsync(packageId))
                .ReturnsAsync(existingPackage);

            _mockPackageRepository
                .Setup(x => x.SoftDeleteAsync(It.IsAny<MSP.Domain.Entities.Package>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _packageService.DeleteAsync(packageId));

            _mockPackageRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_MultipleCalls_EachCallChecksPackageStatus()
        {
            // Arrange
            var packageId = Guid.NewGuid();

            var existingPackage = new MSP.Domain.Entities.Package
            {
                Id = packageId,
                Name = "Package",
                IsDeleted = false
            };

            _mockPackageRepository
                .SetupSequence(x => x.GetByIdAsync(packageId))
                .ReturnsAsync(existingPackage)
                .ReturnsAsync(new MSP.Domain.Entities.Package { Id = packageId, IsDeleted = true });

            _mockPackageRepository
                .Setup(x => x.SoftDeleteAsync(It.IsAny<MSP.Domain.Entities.Package>()))
                .Returns(Task.CompletedTask);

            _mockPackageRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result1 = await _packageService.DeleteAsync(packageId);
            var result2 = await _packageService.DeleteAsync(packageId);

            // Assert
            Assert.True(result1.Success);
            Assert.False(result2.Success);
            Assert.Equal("Package not found", result2.Message);

            _mockPackageRepository.Verify(x => x.GetByIdAsync(packageId), Times.Exactly(2));
            _mockPackageRepository.Verify(x => x.SoftDeleteAsync(It.IsAny<MSP.Domain.Entities.Package>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_SaveChangesCalledAfterSoftDelete()
        {
            // Arrange
            var packageId = Guid.NewGuid();
            var callOrder = new System.Collections.Generic.List<string>();

            var existingPackage = new MSP.Domain.Entities.Package
            {
                Id = packageId,
                Name = "Package",
                IsDeleted = false
            };

            _mockPackageRepository
                .Setup(x => x.GetByIdAsync(packageId))
                .ReturnsAsync(existingPackage);

            _mockPackageRepository
                .Setup(x => x.SoftDeleteAsync(It.IsAny<MSP.Domain.Entities.Package>()))
                .Callback(() => callOrder.Add("SoftDelete"))
                .Returns(Task.CompletedTask);

            _mockPackageRepository
                .Setup(x => x.SaveChangesAsync())
                .Callback(() => callOrder.Add("SaveChanges"))
                .Returns(Task.CompletedTask);

            // Act
            await _packageService.DeleteAsync(packageId);

            // Assert
            Assert.Equal(2, callOrder.Count);
            Assert.Equal("SoftDelete", callOrder[0]);
            Assert.Equal("SaveChanges", callOrder[1]);
        }
    }
}
