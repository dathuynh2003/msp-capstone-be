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
    public class GetListOrganizationRequestTest
    {
        private readonly Mock<IOrganizationInviteRepository> _mockOrganizationInviteRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IProjectMemberRepository> _mockProjectMemberRepository;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly IOrganizationInvitationService _organizationInvitationService;

        public GetListOrganizationRequestTest()
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
        public async Task GetSentRequestsByMemberIdAsync_WithValidMemberId_ReturnsSuccessResponse()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var businessOwnerId1 = Guid.NewGuid();
            var businessOwnerId2 = Guid.NewGuid();

            var member = new User
            {
                Id = memberId,
                Email = "member@example.com",
                FullName = "Member User",
                AvatarUrl = "member-avatar.jpg"
            };

            var businessOwner1 = new User
            {
                Id = businessOwnerId1,
                Email = "owner1@example.com",
                FullName = "Business Owner One",
                Organization = "Organization One",
                AvatarUrl = "owner1-avatar.jpg"
            };

            var businessOwner2 = new User
            {
                Id = businessOwnerId2,
                Email = "owner2@example.com",
                FullName = "Business Owner Two",
                Organization = "Organization Two",
                AvatarUrl = null
            };

            var sentRequests = new List<OrganizationInvitation>
            {
                new OrganizationInvitation
                {
                    Id = Guid.NewGuid(),
                    BusinessOwnerId = businessOwnerId1,
                    BusinessOwner = businessOwner1,
                    MemberId = memberId,
                    Member = member,
                    Type = InvitationType.Request,
                    Status = InvitationStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    RespondedAt = null
                },
                new OrganizationInvitation
                {
                    Id = Guid.NewGuid(),
                    BusinessOwnerId = businessOwnerId2,
                    BusinessOwner = businessOwner2,
                    MemberId = memberId,
                    Member = member,
                    Type = InvitationType.Request,
                    Status = InvitationStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    RespondedAt = null
                }
            };

            _mockOrganizationInviteRepository
                .Setup(x => x.GetSentRequestsByMemberIdAsync(memberId))
                .ReturnsAsync(sentRequests);

            // Act
            var result = await _organizationInvitationService.GetSentRequestsByMemberIdAsync(memberId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Found 2 sent request(s).", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());

            var firstRequest = result.Data.First();
            Assert.Equal(memberId, firstRequest.MemberId);
            Assert.Equal("Member User", firstRequest.MemberName);
            Assert.Equal("member@example.com", firstRequest.MemberEmail);
            Assert.Equal(businessOwnerId1, firstRequest.BusinessOwnerId);
            Assert.Equal("Business Owner One", firstRequest.BusinessOwnerName);
            Assert.Equal("Organization One", firstRequest.OrganizationName);
            Assert.Equal(InvitationType.Request, firstRequest.Type);
            Assert.Equal(InvitationStatus.Pending, firstRequest.Status);
            Assert.Null(firstRequest.RespondedAt);
        }

        [Fact]
        public async Task GetSentRequestsByMemberIdAsync_WithNoRequests_ReturnsEmptyList()
        {
            // Arrange
            var memberId = Guid.NewGuid();

            _mockOrganizationInviteRepository
                .Setup(x => x.GetSentRequestsByMemberIdAsync(memberId))
                .ReturnsAsync(new List<OrganizationInvitation>());

            // Act
            var result = await _organizationInvitationService.GetSentRequestsByMemberIdAsync(memberId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Found 0 sent request(s).", result.Message);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetSentRequestsByMemberIdAsync_WithDifferentStatuses_ReturnsAllStatuses()
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
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    RespondedAt = null
                },
                new OrganizationInvitation
                {
                    Id = Guid.NewGuid(),
                    BusinessOwnerId = businessOwnerId,
                    BusinessOwner = businessOwner,
                    MemberId = memberId,
                    Member = member,
                    Type = InvitationType.Request,
                    Status = InvitationStatus.Accepted,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    RespondedAt = DateTime.UtcNow.AddDays(-2)
                },
                new OrganizationInvitation
                {
                    Id = Guid.NewGuid(),
                    BusinessOwnerId = businessOwnerId,
                    BusinessOwner = businessOwner,
                    MemberId = memberId,
                    Member = member,
                    Type = InvitationType.Request,
                    Status = InvitationStatus.Rejected,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    RespondedAt = DateTime.UtcNow
                }
            };

            _mockOrganizationInviteRepository
                .Setup(x => x.GetSentRequestsByMemberIdAsync(memberId))
                .ReturnsAsync(sentRequests);

            // Act
            var result = await _organizationInvitationService.GetSentRequestsByMemberIdAsync(memberId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3, result.Data.Count());

            Assert.Contains(result.Data, req => req.Status == InvitationStatus.Pending);
            Assert.Contains(result.Data, req => req.Status == InvitationStatus.Accepted);
            Assert.Contains(result.Data, req => req.Status == InvitationStatus.Rejected);

            var acceptedRequest = result.Data.First(req => req.Status == InvitationStatus.Accepted);
            Assert.NotNull(acceptedRequest.RespondedAt);

            var pendingRequest = result.Data.First(req => req.Status == InvitationStatus.Pending);
            Assert.Null(pendingRequest.RespondedAt);
        }

        [Fact]
        public async Task GetSentRequestsByMemberIdAsync_WhenRepositoryThrowsException_ReturnsErrorResponse()
        {
            // Arrange
            var memberId = Guid.NewGuid();

            _mockOrganizationInviteRepository
                .Setup(x => x.GetSentRequestsByMemberIdAsync(memberId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _organizationInvitationService.GetSentRequestsByMemberIdAsync(memberId);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Error retrieving sent requests", result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetPendingRequestsByBusinessOwnerIdAsync_WithMultipleRequests_ReturnsAllPendingRequests()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var memberId1 = Guid.NewGuid();
            var memberId2 = Guid.NewGuid();
            var memberId3 = Guid.NewGuid();

            var businessOwner = new User
            {
                Id = businessOwnerId,
                Email = "owner@example.com",
                FullName = "Business Owner",
                Organization = "Test Organization"
            };

            var member1 = new User
            {
                Id = memberId1,
                Email = "member1@example.com",
                FullName = "Member One"
            };

            var member2 = new User
            {
                Id = memberId2,
                Email = "member2@example.com",
                FullName = "Member Two"
            };

            var member3 = new User
            {
                Id = memberId3,
                Email = "member3@example.com",
                FullName = "Member Three"
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
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
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
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new OrganizationInvitation
                {
                    Id = Guid.NewGuid(),
                    BusinessOwnerId = businessOwnerId,
                    BusinessOwner = businessOwner,
                    MemberId = memberId3,
                    Member = member3,
                    Type = InvitationType.Request,
                    Status = InvitationStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            _mockOrganizationInviteRepository
                .Setup(x => x.GetPendingRequestsByBusinessOwnerIdAsync(businessOwnerId))
                .ReturnsAsync(pendingRequests);

            // Act
            var result = await _organizationInvitationService.GetPendingRequestsByBusinessOwnerIdAsync(businessOwnerId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Found 3 pending request(s).", result.Message);
            Assert.Equal(3, result.Data.Count());

            Assert.All(result.Data, req =>
            {
                Assert.Equal(InvitationType.Request, req.Type);
                Assert.Equal(InvitationStatus.Pending, req.Status);
                Assert.Equal(businessOwnerId, req.BusinessOwnerId);
                Assert.Equal("Test Organization", req.OrganizationName);
            });
        }

        [Fact]
        public async Task GetSentRequestsByMemberIdAsync_IncludesBusinessOwnerDetails()
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
                Organization = "Complete Organization",
                AvatarUrl = "https://example.com/owner-avatar.jpg"
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
            var request = result.Data.First();
            Assert.Equal(businessOwnerId, request.BusinessOwnerId);
            Assert.Equal("Business Owner", request.BusinessOwnerName);
            Assert.Equal("owner@example.com", request.BusinessOwnerEmail);
            Assert.Equal("https://example.com/owner-avatar.jpg", request.BusinessOwnerAvatar);
            Assert.Equal("Complete Organization", request.OrganizationName);
        }

        [Fact]
        public async Task GetSentRequestsByMemberIdAsync_WithOldAndNewRequests_ReturnsAllInChronologicalOrder()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var businessOwnerId = Guid.NewGuid();

            var member = new User { Id = memberId, Email = "member@example.com", FullName = "Member" };
            var businessOwner = new User { Id = businessOwnerId, Email = "owner@example.com", FullName = "Owner", Organization = "Org" };

            var oldDate = DateTime.UtcNow.AddDays(-30);
            var recentDate = DateTime.UtcNow.AddDays(-1);

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
                    CreatedAt = oldDate
                },
                new OrganizationInvitation
                {
                    Id = Guid.NewGuid(),
                    BusinessOwnerId = businessOwnerId,
                    BusinessOwner = businessOwner,
                    MemberId = memberId,
                    Member = member,
                    Type = InvitationType.Request,
                    Status = InvitationStatus.Pending,
                    CreatedAt = recentDate
                }
            };

            _mockOrganizationInviteRepository
                .Setup(x => x.GetSentRequestsByMemberIdAsync(memberId))
                .ReturnsAsync(sentRequests);

            // Act
            var result = await _organizationInvitationService.GetSentRequestsByMemberIdAsync(memberId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data.Count());

            var firstRequest = result.Data.First();
            var lastRequest = result.Data.Last();

            Assert.Equal(oldDate, firstRequest.CreatedAt);
            Assert.Equal(recentDate, lastRequest.CreatedAt);
        }
    }
}
