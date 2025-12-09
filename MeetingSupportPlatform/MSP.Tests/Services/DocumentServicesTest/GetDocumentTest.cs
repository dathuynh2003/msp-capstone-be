using Microsoft.AspNetCore.Identity;
using Moq;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Document;
using MSP.Domain.Entities;
using Xunit;

namespace MSP.Tests.Services.DocumentServicesTest
{
    public class GetDocumentTest
    {
        private readonly Mock<IDocumentRepository> _documentRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly DocumentService _documentService;

        public GetDocumentTest()
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
        public async Task GetDocumentByIdAsync_WithValidDocumentId_ReturnsSuccessResponse()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var owner = new User
            {
                Id = ownerId,
                FullName = "Test Owner",
                Email = "owner@test.com",
                AvatarUrl = "avatar.jpg"
            };

            var document = new Domain.Entities.Document
            {
                Id = documentId,
                Name = "Test Document.pdf",
                OwnerId = ownerId,
                ProjectId = projectId,
                FileUrl = "https://storage.com/documents/test.pdf",
                Description = "Test document description",
                Size = 1024000,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Owner = owner
            };

            _documentRepositoryMock.Setup(x => x.GetDocumentByIdAsync(documentId))
                .ReturnsAsync(document);

            _userManagerMock.Setup(x => x.GetRolesAsync(owner))
                .ReturnsAsync(new List<string> { "BusinessOwner" });

            // Act
            var result = await _documentService.GetDocumentByIdAsync(documentId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(documentId, result.Data.Id);
            Assert.Equal("Test Document.pdf", result.Data.Name);
            Assert.Equal("https://storage.com/documents/test.pdf", result.Data.FileUrl);
            Assert.Equal(1024000, result.Data.Size);
            Assert.Equal("Document retrieved successfully", result.Message);
        }

        [Fact]
        public async Task GetDocumentByIdAsync_WithNonExistentDocument_ReturnsErrorResponse()
        {
            // Arrange
            var documentId = Guid.NewGuid();

            _documentRepositoryMock.Setup(x => x.GetDocumentByIdAsync(documentId))
                .ReturnsAsync((Domain.Entities.Document?)null);

            // Act
            var result = await _documentService.GetDocumentByIdAsync(documentId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Document not found", result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetDocumentByIdAsync_IncludesOwnerDetails_InResponse()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var owner = new User
            {
                Id = ownerId,
                FullName = "John Doe",
                Email = "john.doe@test.com",
                AvatarUrl = "https://avatar.com/johndoe.jpg"
            };

            var document = new Domain.Entities.Document
            {
                Id = documentId,
                Name = "Test Document.pdf",
                OwnerId = ownerId,
                ProjectId = Guid.NewGuid(),
                FileUrl = "https://storage.com/documents/test.pdf",
                Description = "Test document",
                Size = 1024000,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                Owner = owner
            };

            _documentRepositoryMock.Setup(x => x.GetDocumentByIdAsync(documentId))
                .ReturnsAsync(document);

            _userManagerMock.Setup(x => x.GetRolesAsync(owner))
                .ReturnsAsync(new List<string> { "ProjectManager" });

            // Act
            var result = await _documentService.GetDocumentByIdAsync(documentId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.Owner);
            Assert.Equal(ownerId, result.Data.Owner.Id);
            Assert.Equal("John Doe", result.Data.Owner.FullName);
            Assert.Equal("john.doe@test.com", result.Data.Owner.Email);
            Assert.Equal("https://avatar.com/johndoe.jpg", result.Data.Owner.AvatarUrl);
            Assert.Equal("ProjectManager", result.Data.Owner.Role);
        }

        [Fact]
        public async Task GetDocumentByIdAsync_WithNullOwner_HandlesGracefully()
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
                CreatedAt = DateTime.UtcNow,
                Owner = null! // No owner loaded
            };

            _documentRepositoryMock.Setup(x => x.GetDocumentByIdAsync(documentId))
                .ReturnsAsync(document);

            // Act
            var result = await _documentService.GetDocumentByIdAsync(documentId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Null(result.Data.Owner);
        }

        [Fact]
        public async Task GetDocumentByIdAsync_VerifiesAllDocumentProperties()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow.AddDays(-5);
            var updatedAt = DateTime.UtcNow;

            var owner = new User
            {
                Id = ownerId,
                FullName = "Test Owner",
                Email = "owner@test.com"
            };

            var document = new Domain.Entities.Document
            {
                Id = documentId,
                Name = "Important Document.pdf",
                OwnerId = ownerId,
                ProjectId = projectId,
                FileUrl = "https://storage.com/documents/important.pdf",
                Description = "Very important document",
                Size = 2048000,
                IsDeleted = false,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                Owner = owner
            };

            _documentRepositoryMock.Setup(x => x.GetDocumentByIdAsync(documentId))
                .ReturnsAsync(document);

            _userManagerMock.Setup(x => x.GetRolesAsync(owner))
                .ReturnsAsync(new List<string> { "Member" });

            // Act
            var result = await _documentService.GetDocumentByIdAsync(documentId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(documentId, result.Data.Id);
            Assert.Equal("Important Document.pdf", result.Data.Name);
            Assert.Equal(ownerId, result.Data.OwnerId);
            Assert.Equal(projectId, result.Data.ProjectId);
            Assert.Equal("https://storage.com/documents/important.pdf", result.Data.FileUrl);
            Assert.Equal("Very important document", result.Data.Description);
            Assert.Equal(2048000, result.Data.Size);
            Assert.Equal(createdAt, result.Data.CreatedAt);
            Assert.Equal(updatedAt, result.Data.UpdatedAt);
        }

        [Fact]
        public async Task GetDocumentByIdAsync_WithNullDescription_HandlesGracefully()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var owner = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Test Owner",
                Email = "owner@test.com"
            };

            var document = new Domain.Entities.Document
            {
                Id = documentId,
                Name = "Test Document.pdf",
                OwnerId = owner.Id,
                ProjectId = Guid.NewGuid(),
                FileUrl = "https://storage.com/documents/test.pdf",
                Description = null, // Null description
                Size = 1024000,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                Owner = owner
            };

            _documentRepositoryMock.Setup(x => x.GetDocumentByIdAsync(documentId))
                .ReturnsAsync(document);

            _userManagerMock.Setup(x => x.GetRolesAsync(owner))
                .ReturnsAsync(new List<string> { "BusinessOwner" });

            // Act
            var result = await _documentService.GetDocumentByIdAsync(documentId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Null(result.Data.Description);
        }

        [Fact]
        public async Task GetDocumentByIdAsync_WithEmptyGuid_ReturnsErrorResponse()
        {
            // Arrange
            var documentId = Guid.Empty;

            _documentRepositoryMock.Setup(x => x.GetDocumentByIdAsync(documentId))
                .ReturnsAsync((Domain.Entities.Document?)null);

            // Act
            var result = await _documentService.GetDocumentByIdAsync(documentId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Document not found", result.Message);
        }

        [Fact]
        public async Task GetDocumentByIdAsync_VerifiesRepositoryIsCalledOnce()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var owner = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Test Owner",
                Email = "owner@test.com"
            };

            var document = new Domain.Entities.Document
            {
                Id = documentId,
                Name = "Test Document.pdf",
                OwnerId = owner.Id,
                ProjectId = Guid.NewGuid(),
                FileUrl = "https://storage.com/documents/test.pdf",
                Size = 1024000,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                Owner = owner
            };

            _documentRepositoryMock.Setup(x => x.GetDocumentByIdAsync(documentId))
                .ReturnsAsync(document);

            _userManagerMock.Setup(x => x.GetRolesAsync(owner))
                .ReturnsAsync(new List<string> { "BusinessOwner" });

            // Act
            var result = await _documentService.GetDocumentByIdAsync(documentId);

            // Assert
            Assert.True(result.Success);
            _documentRepositoryMock.Verify(x => x.GetDocumentByIdAsync(documentId), Times.Once);
        }

        [Fact]
        public async Task GetDocumentByIdAsync_WithLargeFileSize_ReturnsCorrectSize()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var largeSize = 104857600L; // 100 MB
            var owner = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Test Owner",
                Email = "owner@test.com"
            };

            var document = new Domain.Entities.Document
            {
                Id = documentId,
                Name = "Large Document.pdf",
                OwnerId = owner.Id,
                ProjectId = Guid.NewGuid(),
                FileUrl = "https://storage.com/documents/large.pdf",
                Description = "Large file",
                Size = largeSize,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                Owner = owner
            };

            _documentRepositoryMock.Setup(x => x.GetDocumentByIdAsync(documentId))
                .ReturnsAsync(document);

            _userManagerMock.Setup(x => x.GetRolesAsync(owner))
                .ReturnsAsync(new List<string> { "BusinessOwner" });

            // Act
            var result = await _documentService.GetDocumentByIdAsync(documentId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(largeSize, result.Data.Size);
        }

        [Fact]
        public async Task GetDocumentByIdAsync_WithDifferentFileTypes_ReturnsCorrectFileUrl()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var owner = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Test Owner",
                Email = "owner@test.com"
            };

            var document = new Domain.Entities.Document
            {
                Id = documentId,
                Name = "Test Document.docx",
                OwnerId = owner.Id,
                ProjectId = Guid.NewGuid(),
                FileUrl = "https://storage.com/documents/test.docx",
                Description = "Word document",
                Size = 512000,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                Owner = owner
            };

            _documentRepositoryMock.Setup(x => x.GetDocumentByIdAsync(documentId))
                .ReturnsAsync(document);

            _userManagerMock.Setup(x => x.GetRolesAsync(owner))
                .ReturnsAsync(new List<string> { "Member" });

            // Act
            var result = await _documentService.GetDocumentByIdAsync(documentId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Test Document.docx", result.Data.Name);
            Assert.Equal("https://storage.com/documents/test.docx", result.Data.FileUrl);
        }

        [Fact]
        public async Task GetDocumentByIdAsync_VerifiesOwnerRoleRetrieval()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var owner = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Test Owner",
                Email = "owner@test.com"
            };

            var document = new Domain.Entities.Document
            {
                Id = documentId,
                Name = "Test Document.pdf",
                OwnerId = owner.Id,
                ProjectId = Guid.NewGuid(),
                FileUrl = "https://storage.com/documents/test.pdf",
                Size = 1024000,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                Owner = owner
            };

            _documentRepositoryMock.Setup(x => x.GetDocumentByIdAsync(documentId))
                .ReturnsAsync(document);

            _userManagerMock.Setup(x => x.GetRolesAsync(owner))
                .ReturnsAsync(new List<string> { "ProjectManager" });

            // Act
            var result = await _documentService.GetDocumentByIdAsync(documentId);

            // Assert
            Assert.True(result.Success);
            _userManagerMock.Verify(x => x.GetRolesAsync(owner), Times.Once);
            Assert.Equal("ProjectManager", result.Data!.Owner!.Role);
        }
    }
}
