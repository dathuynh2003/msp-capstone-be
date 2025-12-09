using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using MSP.Application.Models.Requests.Notification;
using MSP.Application.Repositories;
using MSP.Application.Services.Implementations.OrganizationInvitation;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Services.Interfaces.OrganizationInvitation;
using MSP.Domain.Entities;
using MSP.Shared.Enums;
using Xunit;

namespace MSP.Tests.Services.OrganizationServicesTest
{
    public class AcceptRejectOrganizationRequestTest
    {
        private readonly Mock<IOrganizationInviteRepository> _mockOrganizationInviteRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IProjectMemberRepository> _mockProjectMemberRepository;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly IOrganizationInvitationService _organizationInvitationService;

        public AcceptRejectOrganizationRequestTest()
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

        #region BusinessOwnerAcceptRequestAsync Tests

        [Fact]
        public async Task BusinessOwnerAcceptRequestAsync_WithValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var invitationId = Guid.NewGuid();

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
                FullName = "Member User",
                Organization = null,
                ManagedById = null
            };

            var request = new OrganizationInvitation
            {
                Id = invitationId,
                BusinessOwnerId = businessOwnerId,
                BusinessOwner = businessOwner,
                MemberId = memberId,
                Member = member,
                Type = InvitationType.Request,
                Status = InvitationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _mockOrganizationInviteRepository
                .Setup(x => x.GetByIdAsync(invitationId))
                .ReturnsAsync(request);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(businessOwnerId.ToString()))
                .ReturnsAsync(businessOwner);

            _mockUserManager
                .Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockOrganizationInviteRepository
                .Setup(x => x.UpdateAsync(It.IsAny<OrganizationInvitation>()))
                .Returns(Task.CompletedTask);

            _mockOrganizationInviteRepository
                .Setup(x => x.GetAllPendingInvitationsByMemberIdAsync(memberId))
                .ReturnsAsync(new List<OrganizationInvitation> { request });

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .Returns((Task<Shared.Common.ApiResponse<Application.Models.Responses.Notification.NotificationResponse>>)Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.SendEmailNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            // Act
            var result = await _organizationInvitationService.BusinessOwnerAcceptRequestAsync(businessOwnerId, invitationId);

            // Assert
            Assert.True(result.Success);
            Assert.Contains("has been added to your organization", result.Data);

            _mockUserManager.Verify(
                x => x.UpdateAsync(It.Is<User>(u => 
                    u.Id == memberId && 
                    u.Organization == "Test Organization" && 
                    u.ManagedById == businessOwnerId
                )),
                Times.Once
            );

            _mockOrganizationInviteRepository.Verify(
                x => x.UpdateAsync(It.Is<OrganizationInvitation>(inv =>
                    inv.Id == invitationId &&
                    inv.Status == InvitationStatus.Accepted
                )),
                Times.Once
            );

            _mockNotificationService.Verify(
                x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()),
                Times.Once
            );
        }

        [Fact]
        public async Task BusinessOwnerAcceptRequestAsync_WithNonExistentRequest_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var invitationId = Guid.NewGuid();

            _mockOrganizationInviteRepository
                .Setup(x => x.GetByIdAsync(invitationId))
                .ReturnsAsync((OrganizationInvitation?)null);

            // Act
            var result = await _organizationInvitationService.BusinessOwnerAcceptRequestAsync(businessOwnerId, invitationId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Request not found.", result.Message);

            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task BusinessOwnerAcceptRequestAsync_WithUnauthorizedBusinessOwner_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var wrongBusinessOwnerId = Guid.NewGuid();
            var invitationId = Guid.NewGuid();

            var request = new OrganizationInvitation
            {
                Id = invitationId,
                BusinessOwnerId = businessOwnerId,
                Type = InvitationType.Request,
                Status = InvitationStatus.Pending
            };

            _mockOrganizationInviteRepository
                .Setup(x => x.GetByIdAsync(invitationId))
                .ReturnsAsync(request);

            // Act
            var result = await _organizationInvitationService.BusinessOwnerAcceptRequestAsync(wrongBusinessOwnerId, invitationId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You are not authorized to accept this request.", result.Message);
        }

        [Fact]
        public async Task BusinessOwnerAcceptRequestAsync_WithNonRequestType_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var invitationId = Guid.NewGuid();

            var invitation = new OrganizationInvitation
            {
                Id = invitationId,
                BusinessOwnerId = businessOwnerId,
                Type = InvitationType.Invite,
                Status = InvitationStatus.Pending
            };

            _mockOrganizationInviteRepository
                .Setup(x => x.GetByIdAsync(invitationId))
                .ReturnsAsync(invitation);

            // Act
            var result = await _organizationInvitationService.BusinessOwnerAcceptRequestAsync(businessOwnerId, invitationId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("This is not a join request.", result.Message);
        }

        [Fact]
        public async Task BusinessOwnerAcceptRequestAsync_WithNonPendingStatus_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var invitationId = Guid.NewGuid();

            var request = new OrganizationInvitation
            {
                Id = invitationId,
                BusinessOwnerId = businessOwnerId,
                Type = InvitationType.Request,
                Status = InvitationStatus.Accepted
            };

            _mockOrganizationInviteRepository
                .Setup(x => x.GetByIdAsync(invitationId))
                .ReturnsAsync(request);

            // Act
            var result = await _organizationInvitationService.BusinessOwnerAcceptRequestAsync(businessOwnerId, invitationId);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("This request is already", result.Message);
        }

        [Fact]
        public async Task BusinessOwnerAcceptRequestAsync_CancelsOtherPendingInvitations()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var invitationId = Guid.NewGuid();
            var otherInvitationId1 = Guid.NewGuid();
            var otherInvitationId2 = Guid.NewGuid();

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

            var acceptedRequest = new OrganizationInvitation
            {
                Id = invitationId,
                BusinessOwnerId = businessOwnerId,
                BusinessOwner = businessOwner,
                MemberId = memberId,
                Member = member,
                Type = InvitationType.Request,
                Status = InvitationStatus.Pending
            };

            var otherInvitations = new List<OrganizationInvitation>
            {
                acceptedRequest,
                new OrganizationInvitation { Id = otherInvitationId1, Status = InvitationStatus.Pending },
                new OrganizationInvitation { Id = otherInvitationId2, Status = InvitationStatus.Pending }
            };

            _mockOrganizationInviteRepository
                .Setup(x => x.GetByIdAsync(invitationId))
                .ReturnsAsync(acceptedRequest);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(businessOwnerId.ToString()))
                .ReturnsAsync(businessOwner);

            _mockUserManager
                .Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockOrganizationInviteRepository
                .Setup(x => x.GetAllPendingInvitationsByMemberIdAsync(memberId))
                .ReturnsAsync(otherInvitations);

            _mockOrganizationInviteRepository
                .Setup(x => x.UpdateAsync(It.IsAny<OrganizationInvitation>()))
                .Returns(Task.CompletedTask);

            _mockOrganizationInviteRepository
                .Setup(x => x.UpdateRangeAsync(It.IsAny<List<OrganizationInvitation>>()))
                .Returns(Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .Returns((Task<Shared.Common.ApiResponse<Application.Models.Responses.Notification.NotificationResponse>>)Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.SendEmailNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            // Act
            var result = await _organizationInvitationService.BusinessOwnerAcceptRequestAsync(businessOwnerId, invitationId);

            // Assert
            Assert.True(result.Success);
            Assert.Contains("2 other pending invitation(s) have been canceled", result.Data);

            _mockOrganizationInviteRepository.Verify(
                x => x.UpdateRangeAsync(It.Is<List<OrganizationInvitation>>(list =>
                    list.Count == 2 &&
                    list.All(inv => inv.Status == InvitationStatus.Canceled)
                )),
                Times.Once
            );
        }

        #endregion

        #region BusinessOwnerRejectRequestAsync Tests

        [Fact]
        public async Task BusinessOwnerRejectRequestAsync_WithValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var invitationId = Guid.NewGuid();

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

            var request = new OrganizationInvitation
            {
                Id = invitationId,
                BusinessOwnerId = businessOwnerId,
                BusinessOwner = businessOwner,
                MemberId = memberId,
                Member = member,
                Type = InvitationType.Request,
                Status = InvitationStatus.Pending
            };

            _mockOrganizationInviteRepository
                .Setup(x => x.GetByIdAsync(invitationId))
                .ReturnsAsync(request);

            _mockOrganizationInviteRepository
                .Setup(x => x.UpdateAsync(It.IsAny<OrganizationInvitation>()))
                .Returns(Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .Returns((Task<Shared.Common.ApiResponse<Application.Models.Responses.Notification.NotificationResponse>>)Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.SendEmailNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            // Act
            var result = await _organizationInvitationService.BusinessOwnerRejectRequestAsync(businessOwnerId, invitationId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Request rejected successfully.", result.Message);

            _mockOrganizationInviteRepository.Verify(
                x => x.UpdateAsync(It.Is<OrganizationInvitation>(inv =>
                    inv.Id == invitationId &&
                    inv.Status == InvitationStatus.Rejected
                )),
                Times.Once
            );

            _mockNotificationService.Verify(
                x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()),
                Times.Once
            );
        }

        [Fact]
        public async Task BusinessOwnerRejectRequestAsync_WithNonExistentRequest_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var invitationId = Guid.NewGuid();

            _mockOrganizationInviteRepository
                .Setup(x => x.GetByIdAsync(invitationId))
                .ReturnsAsync((OrganizationInvitation?)null);

            // Act
            var result = await _organizationInvitationService.BusinessOwnerRejectRequestAsync(businessOwnerId, invitationId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Request not found.", result.Message);
        }

        [Fact]
        public async Task BusinessOwnerRejectRequestAsync_WithUnauthorizedBusinessOwner_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var wrongBusinessOwnerId = Guid.NewGuid();
            var invitationId = Guid.NewGuid();

            var request = new OrganizationInvitation
            {
                Id = invitationId,
                BusinessOwnerId = businessOwnerId,
                Type = InvitationType.Request,
                Status = InvitationStatus.Pending
            };

            _mockOrganizationInviteRepository
                .Setup(x => x.GetByIdAsync(invitationId))
                .ReturnsAsync(request);

            // Act
            var result = await _organizationInvitationService.BusinessOwnerRejectRequestAsync(wrongBusinessOwnerId, invitationId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You are not authorized to reject this request.", result.Message);
        }

        [Fact]
        public async Task BusinessOwnerRejectRequestAsync_WithInviteType_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var invitationId = Guid.NewGuid();

            var invitation = new OrganizationInvitation
            {
                Id = invitationId,
                BusinessOwnerId = businessOwnerId,
                Type = InvitationType.Invite,
                Status = InvitationStatus.Pending
            };

            _mockOrganizationInviteRepository
                .Setup(x => x.GetByIdAsync(invitationId))
                .ReturnsAsync(invitation);

            // Act
            var result = await _organizationInvitationService.BusinessOwnerRejectRequestAsync(businessOwnerId, invitationId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("This is not a join request.", result.Message);
        }

        #endregion
    }
}
