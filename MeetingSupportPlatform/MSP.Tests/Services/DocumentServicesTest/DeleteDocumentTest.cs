using Microsoft.AspNetCore.Identity;
using Moq;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Document;
using MSP.Domain.Entities;
using Xunit;

namespace MSP.Tests.Services.DocumentServicesTest
{
    public class DeleteDocumentTest
    {
        private readonly Mock<IDocumentRepository> _documentRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly DocumentService _documentService;

        public DeleteDocumentTest()
        {
            _documentRepositoryMock = new Mock<IDocumentRepository>();
            _projectRepositoryMock = new Mock<IProjectRepository>();

            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _documentService = new DocumentService(
                _documentRepositoryMock.Object,
                _projectRepositoryMock.Object,
                _userManagerMock.Object);
        }

        [Fact]
        public async Task DeleteDocumentAsync_WithValidDocumentId_ReturnsSuccessResponse()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var document = new Domain.Entities.Document
            {
                Id = documentId,
                Name = "Test Document.pdf",
                OwnerId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                FileUrl = "https://storage.com/documents/test.pdf",
                Description = "Test document",
                Size = 1024000,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId))
                .ReturnsAsync(document);

            _documentRepositoryMock.Setup(x => x.SoftDeleteAsync(document))
                .Returns(Task.CompletedTask);

            _documentRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _documentService.DeleteDocumentAsync(documentId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Document deleted successfully", result.Data);
            _documentRepositoryMock.Verify(x => x.SoftDeleteAsync(document), Times.Once);
            _documentRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteDocumentAsync_WithNonExistentDocument_ReturnsErrorResponse()
        {
            // Arrange
            var documentId = Guid.NewGuid();

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId))
                .ReturnsAsync((Domain.Entities.Document?)null);

            // Act
            var result = await _documentService.DeleteDocumentAsync(documentId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Document not found", result.Message);
            _documentRepositoryMock.Verify(x => x.SoftDeleteAsync(It.IsAny<Domain.Entities.Document>()), Times.Never);
            _documentRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteDocumentAsync_WithAlreadyDeletedDocument_ReturnsErrorResponse()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var document = new Domain.Entities.Document
            {
                Id = documentId,
                Name = "Test Document.pdf",
                OwnerId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                FileUrl = "https://storage.com/documents/test.pdf",
                IsDeleted = true, // Already deleted
                CreatedAt = DateTime.UtcNow
            };

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId))
                .ReturnsAsync(document);

            // Act
            var result = await _documentService.DeleteDocumentAsync(documentId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Document not found", result.Message);
            _documentRepositoryMock.Verify(x => x.SoftDeleteAsync(It.IsAny<Domain.Entities.Document>()), Times.Never);
        }

        [Fact]
        public async Task DeleteDocumentAsync_UsesSoftDelete_NotHardDelete()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var document = new Domain.Entities.Document
            {
                Id = documentId,
                Name = "Test Document.pdf",
                OwnerId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                FileUrl = "https://storage.com/documents/test.pdf",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId))
                .ReturnsAsync(document);

            _documentRepositoryMock.Setup(x => x.SoftDeleteAsync(document))
                .Returns(Task.CompletedTask);

            _documentRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _documentService.DeleteDocumentAsync(documentId);

            // Assert
            Assert.True(result.Success);
            _documentRepositoryMock.Verify(x => x.SoftDeleteAsync(document), Times.Once);
        }

        [Fact]
        public async Task DeleteDocumentAsync_VerifiesSaveChangesIsCalled()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var document = new Domain.Entities.Document
            {
                Id = documentId,
                Name = "Test Document.pdf",
                OwnerId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                FileUrl = "https://storage.com/documents/test.pdf",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId))
                .ReturnsAsync(document);

            _documentRepositoryMock.Setup(x => x.SoftDeleteAsync(document))
                .Returns(Task.CompletedTask);

            _documentRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _documentService.DeleteDocumentAsync(documentId);

            // Assert
            Assert.True(result.Success);
            _documentRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteDocumentAsync_VerifiesCorrectOrderOfOperations()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var document = new Domain.Entities.Document
            {
                Id = documentId,
                Name = "Test Document.pdf",
                OwnerId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                FileUrl = "https://storage.com/documents/test.pdf",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            var callOrder = new List<string>();

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId))
                .ReturnsAsync(document);

            _documentRepositoryMock.Setup(x => x.SoftDeleteAsync(document))
                .Callback(() => callOrder.Add("SoftDelete"))
                .Returns(Task.CompletedTask);

            _documentRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Callback(() => callOrder.Add("SaveChanges"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _documentService.DeleteDocumentAsync(documentId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, callOrder.Count);
            Assert.Equal("SoftDelete", callOrder[0]);
            Assert.Equal("SaveChanges", callOrder[1]);
        }

        [Fact]
        public async Task DeleteDocumentAsync_WithMultipleCallsOnSameDocument_OnlyFirstSucceeds()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var document = new Domain.Entities.Document
            {
                Id = documentId,
                Name = "Test Document.pdf",
                OwnerId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                FileUrl = "https://storage.com/documents/test.pdf",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _documentRepositoryMock.SetupSequence(x => x.GetByIdAsync(documentId))
                .ReturnsAsync(document)
                .ReturnsAsync(new Domain.Entities.Document
                {
                    Id = documentId,
                    Name = "Test Document.pdf",
                    OwnerId = document.OwnerId,
                    ProjectId = document.ProjectId,
                    FileUrl = document.FileUrl,
                    IsDeleted = true, // Now deleted
                    CreatedAt = document.CreatedAt
                });

            _documentRepositoryMock.Setup(x => x.SoftDeleteAsync(It.IsAny<Domain.Entities.Document>()))
                .Returns(Task.CompletedTask);

            _documentRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var firstResult = await _documentService.DeleteDocumentAsync(documentId);
            var secondResult = await _documentService.DeleteDocumentAsync(documentId);

            // Assert
            Assert.True(firstResult.Success);
            Assert.False(secondResult.Success);
            Assert.Equal("Document not found", secondResult.Message);
            _documentRepositoryMock.Verify(x => x.SoftDeleteAsync(It.IsAny<Domain.Entities.Document>()), Times.Once);
        }

        [Fact]
        public async Task DeleteDocumentAsync_ReturnsSuccessMessageWithCorrectFormat()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var document = new Domain.Entities.Document
            {
                Id = documentId,
                Name = "Test Document.pdf",
                OwnerId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                FileUrl = "https://storage.com/documents/test.pdf",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId))
                .ReturnsAsync(document);

            _documentRepositoryMock.Setup(x => x.SoftDeleteAsync(document))
                .Returns(Task.CompletedTask);

            _documentRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _documentService.DeleteDocumentAsync(documentId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Document deleted successfully", result.Data);
        }

        [Fact]
        public async Task DeleteDocumentAsync_DoesNotCallSaveChanges_WhenDocumentNotFound()
        {
            // Arrange
            var documentId = Guid.NewGuid();

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId))
                .ReturnsAsync((Domain.Entities.Document?)null);

            // Act
            var result = await _documentService.DeleteDocumentAsync(documentId);

            // Assert
            Assert.False(result.Success);
            _documentRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteDocumentAsync_WithEmptyGuid_HandlesGracefully()
        {
            // Arrange
            var documentId = Guid.Empty;

            _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId))
                .ReturnsAsync((Domain.Entities.Document?)null);

            // Act
            var result = await _documentService.DeleteDocumentAsync(documentId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Document not found", result.Message);
        }
    }
}
