using System;
using System.Collections.Generic;
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
    public class InviteMemberTest
    {
        private readonly Mock<IOrganizationInviteRepository> _mockOrganizationInviteRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IProjectMemberRepository> _mockProjectMemberRepository;
        private readonly Mock<IProjectTaskRepository> _mockProjectTaskRepository;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly IOrganizationInvitationService _organizationInvitationService;

        public InviteMemberTest()
        {
            _mockOrganizationInviteRepository = new Mock<IOrganizationInviteRepository>();
            _mockProjectMemberRepository = new Mock<IProjectMemberRepository>();
            _mockProjectTaskRepository = new Mock<IProjectTaskRepository>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null
            );

            _organizationInvitationService = new OrganizationInvitationService(
                _mockOrganizationInviteRepository.Object,
                _mockUserManager.Object,
                _mockProjectMemberRepository.Object,
                _mockProjectTaskRepository.Object,
                _mockProjectRepository.Object,
                _mockNotificationService.Object,
                _mockConfiguration.Object
            );
        }

        [Fact]
        public async Task SendInvitationAsync_ToExistingMember_ReturnsSuccessResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var memberEmail = "member@example.com";

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
                Email = memberEmail,
                FullName = "Member User", // Ensure required property is set
                Organization = null,
                ManagedById = null
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(businessOwnerId.ToString()))
                .ReturnsAsync(businessOwner);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(businessOwner))
                .ReturnsAsync(new List<string> { UserRoleEnum.BusinessOwner.ToString() });

            _mockUserManager
                .Setup(x => x.FindByEmailAsync(memberEmail.ToUpper()))
                .ReturnsAsync(member);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(member))
                .ReturnsAsync(new List<string> { UserRoleEnum.Member.ToString() });

            _mockOrganizationInviteRepository
                .Setup(x => x.IsInvitationExistsAsync(businessOwnerId, memberId))
                .ReturnsAsync(false);

            _mockOrganizationInviteRepository
                .Setup(x => x.AddAsync(It.IsAny<OrganizationInvitation>()))
                .ReturnsAsync(true);

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .Returns((Task<Shared.Common.ApiResponse<Application.Models.Responses.Notification.NotificationResponse>>)Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.SendEmailNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            // Act
            var result = await _organizationInvitationService.SendInvitationAsync(businessOwnerId, memberEmail);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Equal("Invitation sent successfully.", result.Message);

            _mockOrganizationInviteRepository.Verify(
                x => x.AddAsync(It.Is<OrganizationInvitation>(inv => 
                    inv.BusinessOwnerId == businessOwnerId &&
                    inv.MemberId == memberId &&
                    inv.Type == InvitationType.Invite &&
                    inv.Status == InvitationStatus.Pending
                )),
                Times.Once
            );

            _mockNotificationService.Verify(
                x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()),
                Times.Once
            );

            _mockNotificationService.Verify(
                x => x.SendEmailNotification(memberEmail, "Organization Invitation", It.IsAny<string>()),
                Times.Once
            );
        }

        [Fact]
        public async Task SendInvitationAsync_ToNonExistentMember_SendsExternalInvitation()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var memberEmail = "external@example.com";

            var businessOwner = new User
            {
                Id = businessOwnerId,
                Email = "owner@example.com",
                FullName = "Business Owner",
                Organization = "Test Organization"
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(businessOwnerId.ToString()))
                .ReturnsAsync(businessOwner);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(businessOwner))
                .ReturnsAsync(new List<string> { UserRoleEnum.BusinessOwner.ToString() });

            _mockUserManager
                .Setup(x => x.FindByEmailAsync(memberEmail.ToUpper()))
                .ReturnsAsync((User?)null);

            _mockOrganizationInviteRepository
                .Setup(x => x.IsExternalInvitationExistsAsync(businessOwnerId, memberEmail))
                .ReturnsAsync(false);

            _mockOrganizationInviteRepository
                .Setup(x => x.AddAsync(It.IsAny<OrganizationInvitation>()))
                .ReturnsAsync(true);

            _mockConfiguration
                .Setup(x => x["AppSettings:ClientUrl"])
                .Returns("https://test.example.com");

            _mockNotificationService
                .Setup(x => x.SendEmailNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            // Act
            var result = await _organizationInvitationService.SendInvitationAsync(businessOwnerId, memberEmail);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Equal("Invitation email sent. The user will receive instructions to join.", result.Message);

            _mockOrganizationInviteRepository.Verify(
                x => x.AddAsync(It.Is<OrganizationInvitation>(inv =>
                    inv.BusinessOwnerId == businessOwnerId &&
                    inv.MemberId == null &&
                    inv.InvitedEmail == memberEmail.ToLower() &&
                    !string.IsNullOrEmpty(inv.Token) &&
                    inv.Type == InvitationType.Invite &&
                    inv.Status == InvitationStatus.Pending
                )),
                Times.Once
            );

            _mockNotificationService.Verify(
                x => x.SendEmailNotification(
                    memberEmail,
                    It.Is<string>(s => s.Contains("Invitation to join")),
                    It.IsAny<string>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task SendInvitationAsync_WithNonBusinessOwner_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var memberEmail = "member@example.com";

            var nonBusinessOwner = new User
            {
                Id = businessOwnerId,
                Email = "notowner@example.com",
                FullName = "Not Owner"
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(businessOwnerId.ToString()))
                .ReturnsAsync(nonBusinessOwner);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(nonBusinessOwner))
                .ReturnsAsync(new List<string> { UserRoleEnum.Member.ToString() });

            // Act
            var result = await _organizationInvitationService.SendInvitationAsync(businessOwnerId, memberEmail);

            // Assert
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Equal("You must be a business owner to send invitations.", result.Message);

            _mockOrganizationInviteRepository.Verify(
                x => x.AddAsync(It.IsAny<OrganizationInvitation>()),
                Times.Never
            );
        }

        [Fact]
        public async Task SendInvitationAsync_WithoutOrganization_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var memberEmail = "member@example.com";

            var businessOwner = new User
            {
                Id = businessOwnerId,
                Email = "owner@example.com",
                FullName = "Business Owner",
                Organization = null
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(businessOwnerId.ToString()))
                .ReturnsAsync(businessOwner);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(businessOwner))
                .ReturnsAsync(new List<string> { UserRoleEnum.BusinessOwner.ToString() });

            // Act
            var result = await _organizationInvitationService.SendInvitationAsync(businessOwnerId, memberEmail);

            // Assert
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Equal("You must have an organization to send invitations.", result.Message);
        }

        [Fact]
        public async Task SendInvitationAsync_ToMemberAlreadyInOrganization_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var memberEmail = "member@example.com";

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
                Email = memberEmail,
                FullName = "Member User",
                Organization = "Another Organization",
                ManagedById = Guid.NewGuid()
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(businessOwnerId.ToString()))
                .ReturnsAsync(businessOwner);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(businessOwner))
                .ReturnsAsync(new List<string> { UserRoleEnum.BusinessOwner.ToString() });

            _mockUserManager
                .Setup(x => x.FindByEmailAsync(memberEmail.ToUpper()))
                .ReturnsAsync(member);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(member))
                .ReturnsAsync(new List<string> { UserRoleEnum.Member.ToString() });

            _mockOrganizationInviteRepository
                .Setup(x => x.IsInvitationExistsAsync(businessOwnerId, memberId))
                .ReturnsAsync(false);

            // Act
            var result = await _organizationInvitationService.SendInvitationAsync(businessOwnerId, memberEmail);

            // Assert
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Equal("This user is already part of another organization.", result.Message);

            _mockOrganizationInviteRepository.Verify(
                x => x.AddAsync(It.IsAny<OrganizationInvitation>()),
                Times.Never
            );
        }

        [Fact]
        public async Task SendInvitationAsync_WithExistingPendingInvitation_ReturnsErrorResponse()
        {
            // Arrange
            var businessOwnerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var memberEmail = "member@example.com";

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
                Email = memberEmail,
                FullName = "Member User",
                Organization = null
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(businessOwnerId.ToString()))
                .ReturnsAsync(businessOwner);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(businessOwner))
                .ReturnsAsync(new List<string> { UserRoleEnum.BusinessOwner.ToString() });

            _mockUserManager
                .Setup(x => x.FindByEmailAsync(memberEmail.ToUpper()))
                .ReturnsAsync(member);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(member))
                .ReturnsAsync(new List<string> { UserRoleEnum.Member.ToString() });

            _mockOrganizationInviteRepository
                .Setup(x => x.IsInvitationExistsAsync(businessOwnerId, memberId))
                .ReturnsAsync(true);

            // Act
            var result = await _organizationInvitationService.SendInvitationAsync(businessOwnerId, memberEmail);

            // Assert
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Equal("You already have a pending invitation for this user.", result.Message);

            _mockOrganizationInviteRepository.Verify(
                x => x.AddAsync(It.IsAny<OrganizationInvitation>()),
                Times.Never
            );
        }

        [Fact]
        public async Task RequestJoinOrganizeAsync_WithValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var businessOwnerId = Guid.NewGuid();

            var member = new User
            {
                Id = memberId,
                Email = "member@example.com",
                FullName = "Member User",
                Organization = null
            };

            var businessOwner = new User
            {
                Id = businessOwnerId,
                Email = "owner@example.com",
                FullName = "Business Owner",
                Organization = "Test Organization"
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(memberId.ToString()))
                .ReturnsAsync(member);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(businessOwnerId.ToString()))
                .ReturnsAsync(businessOwner);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(businessOwner))
                .ReturnsAsync(new List<string> { "BusinessOwner" });

            _mockOrganizationInviteRepository
                .Setup(x => x.IsInvitationExistsAsync(businessOwnerId, memberId))
                .ReturnsAsync(false);

            _mockOrganizationInviteRepository
                .Setup(x => x.AddAsync(It.IsAny<OrganizationInvitation>()))
                .ReturnsAsync(true);

            _mockNotificationService
                .Setup(x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                .Returns((Task<Shared.Common.ApiResponse<Application.Models.Responses.Notification.NotificationResponse>>)Task.CompletedTask);

            _mockNotificationService
                .Setup(x => x.SendEmailNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            // Act
            var result = await _organizationInvitationService.RequestJoinOrganizeAsync(memberId, businessOwnerId);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Equal("Request to join organization sent successfully.", result.Message);

            _mockOrganizationInviteRepository.Verify(
                x => x.AddAsync(It.Is<OrganizationInvitation>(inv =>
                    inv.BusinessOwnerId == businessOwnerId &&
                    inv.MemberId == memberId &&
                    inv.Type == InvitationType.Request &&
                    inv.Status == InvitationStatus.Pending
                )),
                Times.Once
            );

            _mockNotificationService.Verify(
                x => x.CreateInAppNotificationAsync(It.IsAny<CreateNotificationRequest>()),
                Times.Once
            );
        }

        [Fact]
        public async Task RequestJoinOrganizeAsync_WhenMemberAlreadyInOrganization_ReturnsErrorResponse()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var businessOwnerId = Guid.NewGuid();

            var member = new User
            {
                Id = memberId,
                Email = "member@example.com",
                FullName = "Member User",
                Organization = "Existing Organization"
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(memberId.ToString()))
                .ReturnsAsync(member);

            // Act
            var result = await _organizationInvitationService.RequestJoinOrganizeAsync(memberId, businessOwnerId);

            // Assert
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Equal("You are already part of an organization. Please leave your current organization first.", result.Message);

            _mockOrganizationInviteRepository.Verify(
                x => x.AddAsync(It.IsAny<OrganizationInvitation>()),
                Times.Never
            );
        }

        [Fact]
        public async Task RequestJoinOrganizeAsync_WhenBusinessOwnerNotFound_ReturnsErrorResponse()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var businessOwnerId = Guid.NewGuid();

            var member = new User
            {
                Id = memberId,
                Email = "member@example.com",
                FullName = "Member User",
                Organization = null
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(memberId.ToString()))
                .ReturnsAsync(member);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(businessOwnerId.ToString()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _organizationInvitationService.RequestJoinOrganizeAsync(memberId, businessOwnerId);

            // Assert
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Equal("Business owner not found.", result.Message);
        }

        [Fact]
        public async Task RequestJoinOrganizeAsync_WhenUserIsNotBusinessOwner_ReturnsErrorResponse()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var businessOwnerId = Guid.NewGuid();

            var member = new User
            {
                Id = memberId,
                Email = "member@example.com",
                FullName = "Member User",
                Organization = null
            };

            var notBusinessOwner = new User
            {
                Id = businessOwnerId,
                Email = "notowner@example.com",
                FullName = "Business User"
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(memberId.ToString()))
                .ReturnsAsync(member);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(businessOwnerId.ToString()))
                .ReturnsAsync(notBusinessOwner);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(notBusinessOwner))
                .ReturnsAsync(new List<string> { "Member" });

            // Act
            var result = await _organizationInvitationService.RequestJoinOrganizeAsync(memberId, businessOwnerId);

            // Assert
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Equal("The specified user is not a business owner.", result.Message);
        }
    }
}
