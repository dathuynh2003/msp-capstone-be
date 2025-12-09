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
    public class GetListMemberTest
    {
        private readonly Mock<IOrganizationInviteRepository> _mockOrganizationInviteRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IProjectMemberRepository> _mockProjectMemberRepository;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly IOrganizationInvitationService _organizationInvitationService;

        public GetListMemberTest()
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
        public async Task GetPendingRequestsByBusinessOwnerIdAsync_WithValidBusinessOwnerId_ReturnsSuccessResponse()
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
                AvatarUrl = "avatar.jpg",
                Organization = "Test Organization"
            };

            var member1 = new User
            {
                Id = memberId1,
                Email = "member1@example.com",
                FullName = "Member One",
                AvatarUrl = "avatar1.jpg"
            };

            var member2 = new User
            {
                Id = memberId2,
                Email = "member2@example.com",
                FullName = "Member Two",
                AvatarUrl = null
            };

            var pendingRequests = new List<OrganizationInvitation>
            {
                new OrganizationInvitation
                {
                    Id = Guid.NewGuid(),
                    BusinessOwnerId = businessOwnerId,
                    BusinessOwner = businessOwner,
                    MemberId = memberId1,
                    Member = member1,
                    Type = InvitationType.Request,
                    Status = InvitationStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    RespondedAt = null
                },
                new OrganizationInvitation
                {
                    Id = Guid.NewGuid(),
                    BusinessOwnerId = businessOwnerId,
                    BusinessOwner = businessOwner,
                    MemberId = memberId2,
                    Member = member2,
                    Type = InvitationType.Request,
                    Status = InvitationStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    RespondedAt = null
                }
            };

            _mockOrganizationInviteRepository
                .Setup(x => x.GetPendingRequestsByBusinessOwnerIdAsync(businessOwnerId))
                .ReturnsAsync(pendingRequests);

            // Act
            var result = await _organizationInvitationService.GetPendingRequestsByBusinessOwnerIdAsync(businessOwnerId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Found 2 pending request(s).", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());

            var firstRequest = result.Data.First();
            Assert.Equal(businessOwnerId, firstRequest.BusinessOwnerId);
            Assert.Equal("Business Owner", firstRequest.BusinessOwnerName);
            Assert.Equal("Test Organization", firstRequest.OrganizationName);
            Assert.Equal(memberId1, firstRequest.MemberId);
            Assert.Equal("Member One", firstRequest.MemberName);
            Assert.Equal(InvitationType.Request, firstRequest.Type);
            Assert.Equal(InvitationStatus.Pending, firstRequest.Status);

            _mockOrganizationInviteRepository.Verify(
                x => x.GetPendingRequestsByBusinessOwnerIdAsync(businessOwnerId), 
                Times.Once
            );
        }

        [Fact]
        public async Task GetPendingRequestsByBusinessOwnerIdAsync_WithNoPendingRequests_ReturnsEmptyList()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();

            _mockOrganizationInviteRepository
                .Setup(x => x.GetPendingRequestsByBusinessOwnerIdAsync(businessOwnerId))
                .ReturnsAsync(new List<OrganizationInvitation>());

            // Act
            var result = await _organizationInvitationService.GetPendingRequestsByBusinessOwnerIdAsync(businessOwnerId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Found 0 pending request(s).", result.Message);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPendingRequestsByBusinessOwnerIdAsync_WhenRepositoryThrowsException_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();

            _mockOrganizationInviteRepository
                .Setup(x => x.GetPendingRequestsByBusinessOwnerIdAsync(businessOwnerId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _organizationInvitationService.GetPendingRequestsByBusinessOwnerIdAsync(businessOwnerId);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Error retrieving pending requests", result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetReceivedInvitationsByMemberIdAsync_WithValidMemberId_ReturnsSuccessResponse()
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
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
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
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
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
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());

            var firstInvitation = result.Data.First();
            Assert.Equal(businessOwnerId1, firstInvitation.BusinessOwnerId);
            Assert.Equal("Organization One", firstInvitation.OrganizationName);
            Assert.Equal(memberId, firstInvitation.MemberId);
            Assert.Equal(InvitationType.Invite, firstInvitation.Type);
            Assert.Equal(InvitationStatus.Pending, firstInvitation.Status);
        }

        [Fact]
        public async Task GetReceivedInvitationsByMemberIdAsync_WithInvalidMember_ReturnsErrorResponse()
        {
            // Arrange
            var memberId = Guid.NewGuid();

            _mockUserManager
                .Setup(x => x.FindByIdAsync(memberId.ToString()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _organizationInvitationService.GetReceivedInvitationsByMemberIdAsync(memberId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid Member! Cannot fetch invitaions", result.Message);
            Assert.Null(result.Data);

            _mockOrganizationInviteRepository.Verify(
                x => x.GetReceivedInvitationsByMemberIdAsync(It.IsAny<Guid>()), 
                Times.Never
            );
        }

        [Fact]
        public async Task GetSentInvitationsByBusinessOwnerIdAsync_WithValidBusinessOwnerId_ReturnsSuccessResponse()
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
                Email = "member@example.com",
                FullName = "Member User"
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
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
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
            Assert.Equal("Found 2 sent invitation(s).", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());

            var internalInvitation = result.Data.First();
            Assert.Equal(memberId, internalInvitation.MemberId);
            Assert.Equal("Member User", internalInvitation.MemberName);
            Assert.Equal("member@example.com", internalInvitation.MemberEmail);

            var externalInvitation = result.Data.Last();
            Assert.Equal(Guid.Empty, externalInvitation.MemberId);
            Assert.Equal("external@example.com", externalInvitation.MemberName);
            Assert.Equal("external@example.com", externalInvitation.MemberEmail);
        }

        [Fact]
        public async Task GetSentRequestsByMemberIdAsync_WithValidMemberId_ReturnsSuccessResponse()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var businessOwnerId = Guid.NewGuid();

            var member = new User
            {
                Id = memberId,
                Email = "member@example.com",
                FullName = "Member User"
            };

            var businessOwner = new User
            {
                Id = businessOwnerId,
                Email = "owner@example.com",
                FullName = "Business Owner",
                Organization = "Test Organization"
            };

            var sentRequests = new List<OrganizationInvitation>
            {
                new OrganizationInvitation
                {
                    Id = Guid.NewGuid(),
                    BusinessOwnerId = businessOwnerId,
                    BusinessOwner = businessOwner,
                    MemberId = memberId,
                    Member = member,
                    Type = InvitationType.Request,
                    Status = InvitationStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            _mockOrganizationInviteRepository
                .Setup(x => x.GetSentRequestsByMemberIdAsync(memberId))
                .ReturnsAsync(sentRequests);

            // Act
            var result = await _organizationInvitationService.GetSentRequestsByMemberIdAsync(memberId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Found 1 sent request(s).", result.Message);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);

            var request = result.Data.First();
            Assert.Equal(memberId, request.MemberId);
            Assert.Equal(businessOwnerId, request.BusinessOwnerId);
            Assert.Equal("Test Organization", request.OrganizationName);
            Assert.Equal(InvitationType.Request, request.Type);
        }
    }
}
