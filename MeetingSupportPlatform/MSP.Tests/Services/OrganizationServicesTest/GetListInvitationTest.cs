using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.OrganizationInvitation;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Services.Interfaces.OrganizationInvitation;
using MSP.Domain.Entities;
using MSP.Shared.Enums;
using Xunit;

namespace MSP.Tests.Services.OrganizationServicesTest
{
    public class GetListInvitationTest
    {
        private readonly Mock<IOrganizationInviteRepository> _mockOrganizationInviteRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IProjectMemberRepository> _mockProjectMemberRepository;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly IOrganizationInvitationService _organizationInvitationService;

        public GetListInvitationTest()
        {
            _mockOrganizationInviteRepository = new Mock<IOrganizationInviteRepository>();
            _mockProjectMemberRepository = new Mock<IProjectMemberRepository>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null
            );

            _organizationInvitationService = new OrganizationInvitationService(
                _mockOrganizationInviteRepository.Object,
                _mockUserManager.Object,
                _mockProjectMemberRepository.Object,
                _mockNotificationService.Object,
                _mockConfiguration.Object
            );
        }

        [Fact]
        public async Task GetSentInvitationsByBusinessOwnerIdAsync_WithInternalInvitations_ReturnsSuccessResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var memberId1 = Guid.NewGuid();
            var memberId2 = Guid.NewGuid();

            var businessOwner = new User
            {
                Id = businessOwnerId,
                Email = "owner@example.com",
                FullName = "Business Owner",
                Organization = "Test Organization",
                AvatarUrl = "owner-avatar.jpg"
            };

            var member1 = new User
            {
                Id = memberId1,
                Email = "member1@example.com",
                FullName = "Member One",
                AvatarUrl = "member1-avatar.jpg"
            };

            var member2 = new User
            {
                Id = memberId2,
                Email = "member2@example.com",
                FullName = "Member Two",
                AvatarUrl = null
            };

            var sentInvitations = new List<OrganizationInvitation>
            {
                new OrganizationInvitation
                {
                    Id = Guid.NewGuid(),
                    BusinessOwnerId = businessOwnerId,
                    BusinessOwner = businessOwner,
                    MemberId = memberId1,
                    Member = member1,
                    InvitedEmail = null,
                    Type = InvitationType.Invite,
                    Status = InvitationStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    RespondedAt = null
                },
                new OrganizationInvitation
                {
                    Id = Guid.NewGuid(),
                    BusinessOwnerId = businessOwnerId,
                    BusinessOwner = businessOwner,
                    MemberId = memberId2,
                    Member = member2,
                    InvitedEmail = null,
                    Type = InvitationType.Invite,
                    Status = InvitationStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    RespondedAt = null
                }
            };

            _mockOrganizationInviteRepository
                .Setup(x => x.GetSentInvitationsByBusinessOwnerIdAsync(businessOwnerId))
                .ReturnsAsync(sentInvitations);

            // Act
            var result = await _organizationInvitationService.GetSentInvitationsByBusinessOwnerIdAsync(businessOwnerId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Found 2 sent invitation(s).", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());

            var firstInvitation = result.Data.First();
            Assert.Equal(businessOwnerId, firstInvitation.BusinessOwnerId);
            Assert.Equal("Business Owner", firstInvitation.BusinessOwnerName);
            Assert.Equal("Test Organization", firstInvitation.OrganizationName);
            Assert.Equal(memberId1, firstInvitation.MemberId);
            Assert.Equal("Member One", firstInvitation.MemberName);
            Assert.Equal("member1@example.com", firstInvitation.MemberEmail);
            Assert.Equal("member1-avatar.jpg", firstInvitation.MemberAvatar);
            Assert.Equal(InvitationType.Invite, firstInvitation.Type);
            Assert.Equal(InvitationStatus.Pending, firstInvitation.Status);
        }

        [Fact]
        public async Task GetSentInvitationsByBusinessOwnerIdAsync_WithExternalInvitations_ReturnsSuccessResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();

            var businessOwner = new User
            {
                Id = businessOwnerId,
                Email = "owner@example.com",
                FullName = "Business Owner",
                Organization = "Test Organization"
            };

            var sentInvitations = new List<OrganizationInvitation>
            {
                new OrganizationInvitation
                {
                    Id = Guid.NewGuid(),
                    BusinessOwnerId = businessOwnerId,
                    BusinessOwner = businessOwner,
                    MemberId = null,
                    Member = null,
                    InvitedEmail = "external1@example.com",
                    Type = InvitationType.Invite,
                    Status = InvitationStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new OrganizationInvitation
                {
                    Id = Guid.NewGuid(),
                    BusinessOwnerId = businessOwnerId,
                    BusinessOwner = businessOwner,
                    MemberId = null,
                    Member = null,
                    InvitedEmail = "external2@example.com",
                    Type = InvitationType.Invite,
                    Status = InvitationStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            _mockOrganizationInviteRepository
                .Setup(x => x.GetSentInvitationsByBusinessOwnerIdAsync(businessOwnerId))
                .ReturnsAsync(sentInvitations);

            // Act
            var result = await _organizationInvitationService.GetSentInvitationsByBusinessOwnerIdAsync(businessOwnerId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data.Count());

            var externalInvitation1 = result.Data.First();
            Assert.Equal(Guid.Empty, externalInvitation1.MemberId);
            Assert.Equal("external1@example.com", externalInvitation1.MemberName);
            Assert.Equal("external1@example.com", externalInvitation1.MemberEmail);
            Assert.Null(externalInvitation1.MemberAvatar);
        }

        [Fact]
        public async Task GetSentInvitationsByBusinessOwnerIdAsync_WithMixedInvitations_ReturnsAllInvitations()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();

            var businessOwner = new User
            {
                Id = businessOwnerId,
                Email = "owner@example.com",
                FullName = "Business Owner",
                Organization = "Test Organization"
            };

            var member = new User
            {
                Id = memberId,
                Email = "internal@example.com",
                FullName = "Internal Member"
            };

            var sentInvitations = new List<OrganizationInvitation>
            {
                new OrganizationInvitation
                {
                    Id = Guid.NewGuid(),
                    BusinessOwnerId = businessOwnerId,
                    BusinessOwner = businessOwner,
                    MemberId = memberId,
                    Member = member,
                    InvitedEmail = null,
                    Type = InvitationType.Invite,
                    Status = InvitationStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new OrganizationInvitation
                {
                    Id = Guid.NewGuid(),
                    BusinessOwnerId = businessOwnerId,
                    BusinessOwner = businessOwner,
                    MemberId = null,
                    Member = null,
                    InvitedEmail = "external@example.com",
                    Type = InvitationType.Invite,
                    Status = InvitationStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            _mockOrganizationInviteRepository
                .Setup(x => x.GetSentInvitationsByBusinessOwnerIdAsync(businessOwnerId))
                .ReturnsAsync(sentInvitations);

            // Act
            var result = await _organizationInvitationService.GetSentInvitationsByBusinessOwnerIdAsync(businessOwnerId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data.Count());

            var internalInvitation = result.Data.First();
            Assert.NotEqual(Guid.Empty, internalInvitation.MemberId);
            Assert.Equal("Internal Member", internalInvitation.MemberName);

            var externalInvitation = result.Data.Last();
            Assert.Equal(Guid.Empty, externalInvitation.MemberId);
            Assert.Equal("external@example.com", externalInvitation.MemberEmail);
        }

        [Fact]
        public async Task GetSentInvitationsByBusinessOwnerIdAsync_WithNoInvitations_ReturnsEmptyList()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();

            _mockOrganizationInviteRepository
                .Setup(x => x.GetSentInvitationsByBusinessOwnerIdAsync(businessOwnerId))
                .ReturnsAsync(new List<OrganizationInvitation>());

            // Act
            var result = await _organizationInvitationService.GetSentInvitationsByBusinessOwnerIdAsync(businessOwnerId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Found 0 sent invitation(s).", result.Message);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetSentInvitationsByBusinessOwnerIdAsync_WithDifferentStatuses_ReturnsAllStatuses()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();

            var businessOwner = new User
            {
                Id = businessOwnerId,
                Email = "owner@example.com",
                FullName = "Business Owner",
                Organization = "Test Organization"
            };

            var sentInvitations = new List<OrganizationInvitation>
            {
                new OrganizationInvitation
                {
                    Id = Guid.NewGuid(),
                    BusinessOwnerId = businessOwnerId,
                    BusinessOwner = businessOwner,
                    InvitedEmail = "pending@example.com",
                    Type = InvitationType.Invite,
                    Status = InvitationStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    RespondedAt = null
                },
                new OrganizationInvitation
                {
                    Id = Guid.NewGuid(),
                    BusinessOwnerId = businessOwnerId,
                    BusinessOwner = businessOwner,
                    InvitedEmail = "accepted@example.com",
                    Type = InvitationType.Invite,
                    Status = InvitationStatus.Accepted,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    RespondedAt = DateTime.UtcNow.AddDays(-1)
                },
                new OrganizationInvitation
                {
                    Id = Guid.NewGuid(),
                    BusinessOwnerId = businessOwnerId,
                    BusinessOwner = businessOwner,
                    InvitedEmail = "rejected@example.com",
                    Type = InvitationType.Invite,
                    Status = InvitationStatus.Rejected,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    RespondedAt = DateTime.UtcNow
                }
            };

            _mockOrganizationInviteRepository
                .Setup(x => x.GetSentInvitationsByBusinessOwnerIdAsync(businessOwnerId))
                .ReturnsAsync(sentInvitations);

            // Act
            var result = await _organizationInvitationService.GetSentInvitationsByBusinessOwnerIdAsync(businessOwnerId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3, result.Data.Count());

            Assert.Contains(result.Data, inv => inv.Status == InvitationStatus.Pending);
            Assert.Contains(result.Data, inv => inv.Status == InvitationStatus.Accepted);
            Assert.Contains(result.Data, inv => inv.Status == InvitationStatus.Rejected);
        }

        [Fact]
        public async Task GetSentInvitationsByBusinessOwnerIdAsync_WhenRepositoryThrowsException_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();

            _mockOrganizationInviteRepository
                .Setup(x => x.GetSentInvitationsByBusinessOwnerIdAsync(businessOwnerId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _organizationInvitationService.GetSentInvitationsByBusinessOwnerIdAsync(businessOwnerId);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Error retrieving sent invitations", result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetReceivedInvitationsByMemberIdAsync_WithValidMemberId_ReturnsAllReceivedInvitations()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var businessOwnerId1 = Guid.NewGuid();
            var businessOwnerId2 = Guid.NewGuid();

            var member = new User
            {
                Id = memberId,
                Email = "member@example.com",
                FullName = "Member User"
            };

            var businessOwner1 = new User
            {
                Id = businessOwnerId1,
                Email = "owner1@example.com",
                FullName = "Owner One",
                Organization = "Organization One"
            };

            var businessOwner2 = new User
            {
                Id = businessOwnerId2,
                Email = "owner2@example.com",
                FullName = "Owner Two",
                Organization = "Organization Two"
            };

            var receivedInvitations = new List<OrganizationInvitation>
            {
                new OrganizationInvitation
                {
                    Id = Guid.NewGuid(),
                    BusinessOwnerId = businessOwnerId1,
                    BusinessOwner = businessOwner1,
                    MemberId = memberId,
                    Member = member,
                    Type = InvitationType.Invite,
                    Status = InvitationStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new OrganizationInvitation
                {
                    Id = Guid.NewGuid(),
                    BusinessOwnerId = businessOwnerId2,
                    BusinessOwner = businessOwner2,
                    MemberId = memberId,
                    Member = member,
                    Type = InvitationType.Invite,
                    Status = InvitationStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(memberId.ToString()))
                .ReturnsAsync(member);

            _mockOrganizationInviteRepository
                .Setup(x => x.GetReceivedInvitationsByMemberIdAsync(memberId))
                .ReturnsAsync(receivedInvitations);

            // Act
            var result = await _organizationInvitationService.GetReceivedInvitationsByMemberIdAsync(memberId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Found 2 received invitation(s).", result.Message);
            Assert.Equal(2, result.Data.Count());

            var firstInvitation = result.Data.First();
            Assert.Equal("Organization One", firstInvitation.OrganizationName);
            Assert.Equal("Owner One", firstInvitation.BusinessOwnerName);

            var secondInvitation = result.Data.Last();
            Assert.Equal("Organization Two", secondInvitation.OrganizationName);
        }
    }
}
