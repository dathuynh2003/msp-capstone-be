using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using MSP.Application.Abstracts;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.Users;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Services.Interfaces.Users;
using MSP.Domain.Entities;
using Xunit;

namespace MSP.Tests.Services.OrganizationServicesTest
{
    public class DeleteMemberTest
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IOrganizationInviteRepository> _mockOrganizationInviteRepository;
        private readonly Mock<IProjectMemberRepository> _mockProjectMemberRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<ISubscriptionRepository> _mockSubscriptionRepository;
        private readonly Mock<IPackageRepository> _mockPackageRepository;
        private readonly IUserService _userService;

        public DeleteMemberTest()
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
            _mockSubscriptionRepository = new Mock<ISubscriptionRepository>();
            _mockPackageRepository = new Mock<IPackageRepository>();

            _userService = new UserService(
                _mockUserManager.Object,
                _mockUserRepository.Object,
                _mockNotificationService.Object,
                _mockOrganizationInviteRepository.Object,
                _mockProjectMemberRepository.Object,
                _mockProjectRepository.Object,
                _mockSubscriptionRepository.Object,
                _mockPackageRepository.Object
            );
        }

        [Fact]
        public async Task RemoveMemberFromOrganizationAsync_WithValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var projectId1 = Guid.NewGuid();
            var projectId2 = Guid.NewGuid();

            var member = new User
            {
                Id = memberId,
                Email = "member@example.com",
                FullName = "Member User",
                Organization = "Test Organization",
                ManagedById = businessOwnerId
            };

            var projectIds = new List<Guid> { projectId1, projectId2 };

            var projectMemberships = new List<ProjectMember>
            {
                new ProjectMember
                {
                    Id = Guid.NewGuid(),
                    MemberId = memberId,
                    ProjectId = projectId1,
                    LeftAt = null
                },
                new ProjectMember
                {
                    Id = Guid.NewGuid(),
                    MemberId = memberId,
                    ProjectId = projectId2,
                    LeftAt = null
                }
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(memberId.ToString()))
                .ReturnsAsync(member);

            _mockUserManager
                .Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockProjectRepository
                .Setup(x => x.GetProjectIdsByOwnerIdAsync(businessOwnerId))
                .ReturnsAsync(projectIds);

            _mockProjectMemberRepository
                .Setup(x => x.GetActiveMembershipsByMemberAndProjectsAsync(memberId, projectIds))
                .ReturnsAsync(projectMemberships);

            _mockProjectMemberRepository
                .Setup(x => x.UpdateRangeAsync(It.IsAny<List<ProjectMember>>()))
                .Returns(Task.CompletedTask);

            _mockProjectMemberRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.RemoveMemberFromOrganizationAsync(businessOwnerId, memberId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Member removed successfully.", result.Message);
            Assert.Contains("Removed member from organization and updated 2 project(s)", result.Data);

            _mockUserManager.Verify(
                x => x.UpdateAsync(It.Is<User>(u =>
                    u.Id == memberId &&
                    u.Organization == null &&
                    u.ManagedById == null
                )),
                Times.Once
            );

            _mockProjectMemberRepository.Verify(
                x => x.UpdateRangeAsync(It.Is<List<ProjectMember>>(list =>
                    list.Count == 2 &&
                    list.All(pm => pm.LeftAt != null)
                )),
                Times.Once
            );

            _mockProjectMemberRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveMemberFromOrganizationAsync_WithNonExistentMember_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();

            _mockUserManager
                .Setup(x => x.FindByIdAsync(memberId.ToString()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.RemoveMemberFromOrganizationAsync(businessOwnerId, memberId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Member not found.", result.Message);

            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
            _mockProjectMemberRepository.Verify(x => x.UpdateRangeAsync(It.IsAny<List<ProjectMember>>()), Times.Never);
        }

        [Fact]
        public async Task RemoveMemberFromOrganizationAsync_WithMemberNotBelongingToBusinessOwner_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var otherBusinessOwnerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();

            var member = new User
            {
                Id = memberId,
                Email = "member@example.com",
                FullName = "Member User",
                Organization = "Another Organization",
                ManagedById = otherBusinessOwnerId
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(memberId.ToString()))
                .ReturnsAsync(member);

            // Act
            var result = await _userService.RemoveMemberFromOrganizationAsync(businessOwnerId, memberId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("This member does not belong to your organization.", result.Message);

            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RemoveMemberFromOrganizationAsync_WithMemberWithoutOrganization_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();

            var member = new User
            {
                Id = memberId,
                Email = "member@example.com",
                FullName = "Member User",
                Organization = null,
                ManagedById = businessOwnerId
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(memberId.ToString()))
                .ReturnsAsync(member);

            // Act
            var result = await _userService.RemoveMemberFromOrganizationAsync(businessOwnerId, memberId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("This member does not belong to your organization.", result.Message);
        }

        [Fact]
        public async Task RemoveMemberFromOrganizationAsync_WhenUpdateUserFails_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();

            var member = new User
            {
                Id = memberId,
                Email = "member@example.com",
                FullName = "Member User",
                Organization = "Test Organization",
                ManagedById = businessOwnerId
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(memberId.ToString()))
                .ReturnsAsync(member);

            _mockUserManager
                .Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));

            // Act
            var result = await _userService.RemoveMemberFromOrganizationAsync(businessOwnerId, memberId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to remove member from organization.", result.Message);

            _mockProjectRepository.Verify(x => x.GetProjectIdsByOwnerIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockProjectMemberRepository.Verify(x => x.UpdateRangeAsync(It.IsAny<List<ProjectMember>>()), Times.Never);
        }

        [Fact]
        public async Task RemoveMemberFromOrganizationAsync_WithNoProjectMemberships_ReturnsSuccessResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();

            var member = new User
            {
                Id = memberId,
                Email = "member@example.com",
                FullName = "Member User",
                Organization = "Test Organization",
                ManagedById = businessOwnerId
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(memberId.ToString()))
                .ReturnsAsync(member);

            _mockUserManager
                .Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockProjectRepository
                .Setup(x => x.GetProjectIdsByOwnerIdAsync(businessOwnerId))
                .ReturnsAsync(new List<Guid>());

            _mockProjectMemberRepository
                .Setup(x => x.GetActiveMembershipsByMemberAndProjectsAsync(memberId, It.IsAny<List<Guid>>()))
                .ReturnsAsync(new List<ProjectMember>());

            _mockProjectMemberRepository
                .Setup(x => x.UpdateRangeAsync(It.IsAny<List<ProjectMember>>()))
                .Returns(Task.CompletedTask);

            _mockProjectMemberRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.RemoveMemberFromOrganizationAsync(businessOwnerId, memberId);

            // Assert
            Assert.True(result.Success);
            Assert.Contains("updated 0 project(s)", result.Data);
        }

        [Fact]
        public async Task RemoveMemberFromOrganizationAsync_SetsLeftAtTimestamp()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var beforeTest = DateTime.UtcNow;

            var member = new User
            {
                Id = memberId,
                Email = "member@example.com",
                FullName = "Member User",
                Organization = "Test Organization",
                ManagedById = businessOwnerId
            };

            var projectMemberships = new List<ProjectMember>
            {
                new ProjectMember
                {
                    Id = Guid.NewGuid(),
                    MemberId = memberId,
                    ProjectId = projectId,
                    LeftAt = null
                }
            };

            List<ProjectMember>? capturedMemberships = null;

            _mockUserManager
                .Setup(x => x.FindByIdAsync(memberId.ToString()))
                .ReturnsAsync(member);

            _mockUserManager
                .Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockProjectRepository
                .Setup(x => x.GetProjectIdsByOwnerIdAsync(businessOwnerId))
                .ReturnsAsync(new List<Guid> { projectId });

            _mockProjectMemberRepository
                .Setup(x => x.GetActiveMembershipsByMemberAndProjectsAsync(memberId, It.IsAny<List<Guid>>()))
                .ReturnsAsync(projectMemberships);

            _mockProjectMemberRepository
                .Setup(x => x.UpdateRangeAsync(It.IsAny<List<ProjectMember>>()))
                .Callback<List<ProjectMember>>(pm => capturedMemberships = pm)
                .Returns(Task.CompletedTask);

            _mockProjectMemberRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _userService.RemoveMemberFromOrganizationAsync(businessOwnerId, memberId);
            var afterTest = DateTime.UtcNow;

            // Assert
            Assert.NotNull(capturedMemberships);
            Assert.Single(capturedMemberships);
            Assert.NotNull(capturedMemberships[0].LeftAt);
            Assert.True(capturedMemberships[0].LeftAt >= beforeTest && capturedMemberships[0].LeftAt <= afterTest);
        }

        [Fact]
        public async Task RemoveMemberFromOrganizationAsync_WithMultipleProjects_UpdatesAllMemberships()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();

            var member = new User
            {
                Id = memberId,
                Email = "member@example.com",
                FullName = "Member User",
                Organization = "Test Organization",
                ManagedById = businessOwnerId
            };

            var projectIds = new List<Guid>
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()
            };

            var projectMemberships = new List<ProjectMember>
            {
                new ProjectMember { Id = Guid.NewGuid(), MemberId = memberId, ProjectId = projectIds[0], LeftAt = null },
                new ProjectMember { Id = Guid.NewGuid(), MemberId = memberId, ProjectId = projectIds[1], LeftAt = null },
                new ProjectMember { Id = Guid.NewGuid(), MemberId = memberId, ProjectId = projectIds[2], LeftAt = null }
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(memberId.ToString()))
                .ReturnsAsync(member);

            _mockUserManager
                .Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockProjectRepository
                .Setup(x => x.GetProjectIdsByOwnerIdAsync(businessOwnerId))
                .ReturnsAsync(projectIds);

            _mockProjectMemberRepository
                .Setup(x => x.GetActiveMembershipsByMemberAndProjectsAsync(memberId, projectIds))
                .ReturnsAsync(projectMemberships);

            _mockProjectMemberRepository
                .Setup(x => x.UpdateRangeAsync(It.IsAny<List<ProjectMember>>()))
                .Returns(Task.CompletedTask);

            _mockProjectMemberRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.RemoveMemberFromOrganizationAsync(businessOwnerId, memberId);

            // Assert
            Assert.True(result.Success);
            Assert.Contains("updated 3 project(s)", result.Data);

            _mockProjectMemberRepository.Verify(
                x => x.UpdateRangeAsync(It.Is<List<ProjectMember>>(list =>
                    list.Count == 3 &&
                    list.All(pm => pm.LeftAt != null)
                )),
                Times.Once
            );
        }

        [Fact]
        public async Task RemoveMemberFromOrganizationAsync_WhenExceptionThrown_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();

            _mockUserManager
                .Setup(x => x.FindByIdAsync(memberId.ToString()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _userService.RemoveMemberFromOrganizationAsync(businessOwnerId, memberId);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Error removing member from organization", result.Message);
        }
    }
}
