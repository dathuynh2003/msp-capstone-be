using Microsoft.AspNetCore.Identity;
using Moq;
using MSP.Application.Models.Requests.Document;
using MSP.Application.Models.Responses.Document;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Document;
using MSP.Domain.Entities;
using Xunit;

namespace MSP.Tests.Services.DocumentServicesTest
{
    public class AddDocumentTest
    {
        private readonly Mock<IDocumentRepository> _documentRepositoryMock;
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly DocumentService _documentService;

        public AddDocumentTest()
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
        public async Task CreateDocumentAsync_WithValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new CreateDocumentRequest
            {
                Name = "Test Document.pdf",
                OwnerId = ownerId,
                ProjectId = projectId,
                FileUrl = "https://storage.com/documents/test.pdf",
                Description = "Test document description",
                Size = 1024000
            };

            var owner = new User
            {
                Id = ownerId,
                FullName = "Test Owner",
                Email = "owner@test.com",
                AvatarUrl = "avatar.jpg"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                IsDeleted = false
            };

            var document = new Domain.Entities.Document
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                OwnerId = request.OwnerId,
                ProjectId = request.ProjectId,
                FileUrl = request.FileUrl,
                Description = request.Description,
                Size = request.Size,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(ownerId.ToString()))
                .ReturnsAsync(owner);

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _documentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Document>()))
                .ReturnsAsync(document);

            _documentRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _userManagerMock.Setup(x => x.GetRolesAsync(owner))
                .ReturnsAsync(new List<string> { "BusinessOwner" });

            // Act
            var result = await _documentService.CreateDocumentAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(request.Name, result.Data.Name);
            Assert.Equal(request.FileUrl, result.Data.FileUrl);
            Assert.Equal(request.Size, result.Data.Size);
            Assert.Equal("Document created successfully", result.Message);
            _documentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Document>()), Times.Once);
            _documentRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateDocumentAsync_WithNonExistentOwner_ReturnsErrorResponse()
        {
            // Arrange
            var request = new CreateDocumentRequest
            {
                Name = "Test Document.pdf",
                OwnerId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                FileUrl = "https://storage.com/documents/test.pdf",
                Description = "Test document description",
                Size = 1024000
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(request.OwnerId.ToString()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _documentService.CreateDocumentAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Owner not found", result.Message);
            _documentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Document>()), Times.Never);
        }

        [Fact]
        public async Task CreateDocumentAsync_WithNonExistentProject_ReturnsErrorResponse()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new CreateDocumentRequest
            {
                Name = "Test Document.pdf",
                OwnerId = ownerId,
                ProjectId = projectId,
                FileUrl = "https://storage.com/documents/test.pdf",
                Description = "Test document description",
                Size = 1024000
            };

            var owner = new User
            {
                Id = ownerId,
                FullName = "Test Owner",
                Email = "owner@test.com"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(ownerId.ToString()))
                .ReturnsAsync(owner);

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync((Domain.Entities.Project?)null);

            // Act
            var result = await _documentService.CreateDocumentAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Project not found", result.Message);
            _documentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Document>()), Times.Never);
        }

        [Fact]
        public async Task CreateDocumentAsync_WithDeletedProject_ReturnsErrorResponse()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new CreateDocumentRequest
            {
                Name = "Test Document.pdf",
                OwnerId = ownerId,
                ProjectId = projectId,
                FileUrl = "https://storage.com/documents/test.pdf",
                Description = "Test document description",
                Size = 1024000
            };

            var owner = new User
            {
                Id = ownerId,
                FullName = "Test Owner",
                Email = "owner@test.com"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                IsDeleted = true // Project is deleted
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(ownerId.ToString()))
                .ReturnsAsync(owner);

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            // Act
            var result = await _documentService.CreateDocumentAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Project not found", result.Message);
            _documentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Document>()), Times.Never);
        }

        [Fact]
        public async Task CreateDocumentAsync_VerifiesCreatedAtTimestamp()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var beforeTime = DateTime.UtcNow;

            var request = new CreateDocumentRequest
            {
                Name = "Test Document.pdf",
                OwnerId = ownerId,
                ProjectId = projectId,
                FileUrl = "https://storage.com/documents/test.pdf",
                Description = "Test document description",
                Size = 1024000
            };

            var owner = new User
            {
                Id = ownerId,
                FullName = "Test Owner",
                Email = "owner@test.com"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                IsDeleted = false
            };

            Domain.Entities.Document? capturedDocument = null;

            _userManagerMock.Setup(x => x.FindByIdAsync(ownerId.ToString()))
                .ReturnsAsync(owner);

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _documentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Document>()))
                .Callback<Domain.Entities.Document>(d => capturedDocument = d)
                .ReturnsAsync((Domain.Entities.Document d) => d);

            _documentRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _userManagerMock.Setup(x => x.GetRolesAsync(owner))
                .ReturnsAsync(new List<string> { "BusinessOwner" });

            // Act
            var result = await _documentService.CreateDocumentAsync(request);
            var afterTime = DateTime.UtcNow;

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(capturedDocument);
            Assert.True(capturedDocument.CreatedAt >= beforeTime && capturedDocument.CreatedAt <= afterTime);
        }

        [Fact]
        public async Task CreateDocumentAsync_VerifiesAllDocumentProperties()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new CreateDocumentRequest
            {
                Name = "Important Document.pdf",
                OwnerId = ownerId,
                ProjectId = projectId,
                FileUrl = "https://storage.com/documents/important.pdf",
                Description = "Very important document",
                Size = 2048000
            };

            var owner = new User
            {
                Id = ownerId,
                FullName = "Test Owner",
                Email = "owner@test.com"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                IsDeleted = false
            };

            Domain.Entities.Document? capturedDocument = null;

            _userManagerMock.Setup(x => x.FindByIdAsync(ownerId.ToString()))
                .ReturnsAsync(owner);

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _documentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Document>()))
                .Callback<Domain.Entities.Document>(d => capturedDocument = d)
                .ReturnsAsync((Domain.Entities.Document d) => d);

            _documentRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _userManagerMock.Setup(x => x.GetRolesAsync(owner))
                .ReturnsAsync(new List<string> { "BusinessOwner" });

            // Act
            var result = await _documentService.CreateDocumentAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(capturedDocument);
            Assert.Equal(request.Name, capturedDocument.Name);
            Assert.Equal(request.OwnerId, capturedDocument.OwnerId);
            Assert.Equal(request.ProjectId, capturedDocument.ProjectId);
            Assert.Equal(request.FileUrl, capturedDocument.FileUrl);
            Assert.Equal(request.Description, capturedDocument.Description);
            Assert.Equal(request.Size, capturedDocument.Size);
        }

        [Fact]
        public async Task CreateDocumentAsync_IncludesOwnerDetailsInResponse()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new CreateDocumentRequest
            {
                Name = "Test Document.pdf",
                OwnerId = ownerId,
                ProjectId = projectId,
                FileUrl = "https://storage.com/documents/test.pdf",
                Description = "Test document description",
                Size = 1024000
            };

            var owner = new User
            {
                Id = ownerId,
                FullName = "John Doe",
                Email = "john.doe@test.com",
                AvatarUrl = "https://avatar.com/johndoe.jpg"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                IsDeleted = false
            };

            var document = new Domain.Entities.Document
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                OwnerId = request.OwnerId,
                ProjectId = request.ProjectId,
                FileUrl = request.FileUrl,
                Description = request.Description,
                Size = request.Size,
                CreatedAt = DateTime.UtcNow
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(ownerId.ToString()))
                .ReturnsAsync(owner);

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _documentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Document>()))
                .ReturnsAsync(document);

            _documentRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _userManagerMock.Setup(x => x.GetRolesAsync(owner))
                .ReturnsAsync(new List<string> { "ProjectManager" });

            // Act
            var result = await _documentService.CreateDocumentAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.Owner);
            Assert.Equal(owner.Id, result.Data.Owner.Id);
            Assert.Equal(owner.FullName, result.Data.Owner.FullName);
            Assert.Equal(owner.Email, result.Data.Owner.Email);
            Assert.Equal(owner.AvatarUrl, result.Data.Owner.AvatarUrl);
            Assert.Equal("ProjectManager", result.Data.Owner.Role);
        }

        [Fact]
        public async Task CreateDocumentAsync_WithNullDescription_HandlesGracefully()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new CreateDocumentRequest
            {
                Name = "Test Document.pdf",
                OwnerId = ownerId,
                ProjectId = projectId,
                FileUrl = "https://storage.com/documents/test.pdf",
                Description = null, // Null description
                Size = 1024000
            };

            var owner = new User
            {
                Id = ownerId,
                FullName = "Test Owner",
                Email = "owner@test.com"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                IsDeleted = false
            };

            var document = new Domain.Entities.Document
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                OwnerId = request.OwnerId,
                ProjectId = request.ProjectId,
                FileUrl = request.FileUrl,
                Description = request.Description,
                Size = request.Size,
                CreatedAt = DateTime.UtcNow
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(ownerId.ToString()))
                .ReturnsAsync(owner);

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _documentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Document>()))
                .ReturnsAsync(document);

            _documentRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _userManagerMock.Setup(x => x.GetRolesAsync(owner))
                .ReturnsAsync(new List<string> { "Member" });

            // Act
            var result = await _documentService.CreateDocumentAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Null(result.Data.Description);
        }

        [Fact]
        public async Task CreateDocumentAsync_WithLargeFileSize_SavesCorrectly()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var largeSize = 104857600L; // 100 MB

            var request = new CreateDocumentRequest
            {
                Name = "Large Document.pdf",
                OwnerId = ownerId,
                ProjectId = projectId,
                FileUrl = "https://storage.com/documents/large.pdf",
                Description = "Large file document",
                Size = largeSize
            };

            var owner = new User
            {
                Id = ownerId,
                FullName = "Test Owner",
                Email = "owner@test.com"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                IsDeleted = false
            };

            var document = new Domain.Entities.Document
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                OwnerId = request.OwnerId,
                ProjectId = request.ProjectId,
                FileUrl = request.FileUrl,
                Description = request.Description,
                Size = request.Size,
                CreatedAt = DateTime.UtcNow
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(ownerId.ToString()))
                .ReturnsAsync(owner);

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _documentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Document>()))
                .ReturnsAsync(document);

            _documentRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _userManagerMock.Setup(x => x.GetRolesAsync(owner))
                .ReturnsAsync(new List<string> { "BusinessOwner" });

            // Act
            var result = await _documentService.CreateDocumentAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(largeSize, result.Data.Size);
        }

        [Fact]
        public async Task CreateDocumentAsync_VerifiesSaveChangesIsCalled()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new CreateDocumentRequest
            {
                Name = "Test Document.pdf",
                OwnerId = ownerId,
                ProjectId = projectId,
                FileUrl = "https://storage.com/documents/test.pdf",
                Description = "Test document description",
                Size = 1024000
            };

            var owner = new User
            {
                Id = ownerId,
                FullName = "Test Owner",
                Email = "owner@test.com"
            };

            var project = new Domain.Entities.Project
            {
                Id = projectId,
                Name = "Test Project",
                IsDeleted = false
            };

            var document = new Domain.Entities.Document
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                OwnerId = request.OwnerId,
                ProjectId = request.ProjectId,
                FileUrl = request.FileUrl,
                Description = request.Description,
                Size = request.Size,
                CreatedAt = DateTime.UtcNow
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(ownerId.ToString()))
                .ReturnsAsync(owner);

            _projectRepositoryMock.Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _documentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Document>()))
                .ReturnsAsync(document);

            _documentRepositoryMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _userManagerMock.Setup(x => x.GetRolesAsync(owner))
                .ReturnsAsync(new List<string> { "BusinessOwner" });

            // Act
            var result = await _documentService.CreateDocumentAsync(request);

            // Assert
            Assert.True(result.Success);
            _documentRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}
