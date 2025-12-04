using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using MSP.Application.Abstracts;
using MSP.Application.Models.Requests.Auth;
using MSP.Application.Models.Responses.Auth;
using MSP.Application.Services.Implementations.Auth;
using MSP.Application.Services.Interfaces.Auth;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Services.Interfaces.OrganizationInvitation;
using MSP.Domain.Entities;
using MSP.Shared.Enums;
using Xunit;

namespace MSP.Tests.Services.AccountServicesTest
{
    public class LoginTest
    {
        private readonly Mock<IAuthTokenProcessor> _mockAuthTokenProcessor;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IGoogleTokenValidator> _mockGoogleTokenValidator;
        private Mock<IOrganizationInvitationService> _mockOrganizationInvitationService;

        private readonly IAccountService _accountService;

        public LoginTest()
        {
            _mockAuthTokenProcessor = new Mock<IAuthTokenProcessor>();
            _mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null
            );
            _mockUserRepository = new Mock<IUserRepository>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockGoogleTokenValidator = new Mock<IGoogleTokenValidator>();
            _mockOrganizationInvitationService = new Mock<IOrganizationInvitationService>();

            _accountService = new AccountService(
                _mockAuthTokenProcessor.Object,
                _mockUserManager.Object,
                _mockUserRepository.Object,
                _mockNotificationService.Object,
                _mockConfiguration.Object,
                _mockGoogleTokenValidator.Object,
                _mockOrganizationInvitationService.Object
            );
        }

        #region LoginAsync Tests

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsSuccessResponse()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                UserName = "test@example.com",
                FullName = "Test User",
                EmailConfirmed = true,
                IsApproved = true
            };

            var roles = new[] { UserRoleEnum.Member.ToString() };
            var jwtToken = "jwt_token_here";
            var refreshToken = "refresh_token_here";
            var expiresAtUtc = DateTime.UtcNow.AddHours(1);

            _mockUserManager
                .Setup(x => x.FindByEmailAsync(loginRequest.Email))
                .ReturnsAsync(user);

            _mockUserManager
                .Setup(x => x.CheckPasswordAsync(user, loginRequest.Password))
                .ReturnsAsync(true);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(roles);

            _mockAuthTokenProcessor
                .Setup(x => x.GenerateJwtToken(user, roles))
                .Returns((jwtToken, expiresAtUtc));

            _mockAuthTokenProcessor
                .Setup(x => x.GenerateRefreshToken())
                .Returns(refreshToken);

            _mockUserManager
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _accountService.LoginAsync(loginRequest);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Login successful.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(jwtToken, result.Data.AccessToken);
            Assert.Equal(refreshToken, result.Data.RefreshToken);

            _mockUserManager.Verify(x => x.FindByEmailAsync(loginRequest.Email), Times.Once);
            _mockUserManager.Verify(x => x.CheckPasswordAsync(user, loginRequest.Password), Times.Once);
            _mockAuthTokenProcessor.Verify(x => x.GenerateJwtToken(user, roles), Times.Once);
            _mockAuthTokenProcessor.Verify(x => x.GenerateRefreshToken(), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidEmail_ReturnsErrorResponse()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "nonexistent@example.com",
                Password = "Password123!"
            };

            _mockUserManager
                .Setup(x => x.FindByEmailAsync(loginRequest.Email))
                .ReturnsAsync((User)null);

            // Act
            var result = await _accountService.LoginAsync(loginRequest);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid email or password.", result.Message);
            Assert.Null(result.Data);

            _mockUserManager.Verify(x => x.FindByEmailAsync(loginRequest.Email), Times.Once);
            _mockUserManager.Verify(x => x.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ReturnsErrorResponse()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "WrongPassword123!"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FullName = "test@example.com",
                EmailConfirmed = true
            };

            _mockUserManager
                .Setup(x => x.FindByEmailAsync(loginRequest.Email))
                .ReturnsAsync(user);

            _mockUserManager
                .Setup(x => x.CheckPasswordAsync(user, loginRequest.Password))
                .ReturnsAsync(false);

            // Act
            var result = await _accountService.LoginAsync(loginRequest);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid email or password.", result.Message);
            Assert.Null(result.Data);

            _mockUserManager.Verify(x => x.FindByEmailAsync(loginRequest.Email), Times.Once);
            _mockUserManager.Verify(x => x.CheckPasswordAsync(user, loginRequest.Password), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithUnconfirmedEmail_ReturnsErrorResponse()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FullName = "test@example.com",
                EmailConfirmed = false
            };

            _mockUserManager
                .Setup(x => x.FindByEmailAsync(loginRequest.Email))
                .ReturnsAsync(user);

            _mockUserManager
                .Setup(x => x.CheckPasswordAsync(user, loginRequest.Password))
                .ReturnsAsync(true);

            // Act
            var result = await _accountService.LoginAsync(loginRequest);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Please verify and confirm your email before logging in.", result.Message);
            Assert.Null(result.Data);

            // Verify that GetRolesAsync is never called since email is unconfirmed
            _mockUserManager.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WithUnapprovedBusinessOwner_ReturnsErrorResponse()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "businessowner@example.com",
                Password = "Password123!"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "businessowner@example.com",
                FullName = "businessowner@example.com",
                EmailConfirmed = true,
                IsApproved = false
            };

            var roles = new[] { UserRoleEnum.BusinessOwner.ToString() };

            _mockUserManager
                .Setup(x => x.FindByEmailAsync(loginRequest.Email))
                .ReturnsAsync(user);

            _mockUserManager
                .Setup(x => x.CheckPasswordAsync(user, loginRequest.Password))
                .ReturnsAsync(true);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(roles);

            // Act
            var result = await _accountService.LoginAsync(loginRequest);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Your account is pending admin approval. Please wait for approval before logging in.", result.Message);
            Assert.Null(result.Data);

            _mockAuthTokenProcessor.Verify(x => x.GenerateJwtToken(It.IsAny<User>(), It.IsAny<string[]>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WithApprovedBusinessOwner_ReturnsSuccessResponse()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "businessowner@example.com",
                Password = "Password123!"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "businessowner@example.com",
                FullName = "businessowner@example.com",
                EmailConfirmed = true,
                IsApproved = true
            };

            var roles = new[] { UserRoleEnum.BusinessOwner.ToString() };
            var jwtToken = "jwt_token_here";
            var refreshToken = "refresh_token_here";
            var expiresAtUtc = DateTime.UtcNow.AddHours(1);

            _mockUserManager
                .Setup(x => x.FindByEmailAsync(loginRequest.Email))
                .ReturnsAsync(user);

            _mockUserManager
                .Setup(x => x.CheckPasswordAsync(user, loginRequest.Password))
                .ReturnsAsync(true);

            _mockUserManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(roles);

            _mockAuthTokenProcessor
                .Setup(x => x.GenerateJwtToken(user, roles))
                .Returns((jwtToken, expiresAtUtc));

            _mockAuthTokenProcessor
                .Setup(x => x.GenerateRefreshToken())
                .Returns(refreshToken);

            _mockUserManager
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _accountService.LoginAsync(loginRequest);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Login successful.", result.Message);
            Assert.NotNull(result.Data);
        }

      
      

        #endregion
    }
}
