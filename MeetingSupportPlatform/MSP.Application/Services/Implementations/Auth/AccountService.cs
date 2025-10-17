using Hangfire;
using Microsoft.AspNetCore.Identity;
using MSP.Application.Abstracts;
using MSP.Application.Exceptions;
using MSP.Application.Models.Requests.Auth;
using MSP.Application.Models.Requests.Notification;
using MSP.Application.Models.Responses.Auth;
using MSP.Application.Services.Interfaces.Auth;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Domain.Entities;
using MSP.Shared.Common;
using MSP.Shared.Enums;
using MSP.Shared.Specifications;

namespace MSP.Application.Services.Implementations.Auth
{
    public class AccountService : IAccountService
    {
        private readonly IAuthTokenProcessor _authTokenProcessor;
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;

        public AccountService(IAuthTokenProcessor authTokenProcessor, UserManager<User> userManager, IUserRepository userRepository, INotificationService notificationService)
        {
            _authTokenProcessor = authTokenProcessor;
            _userManager = userManager;
            _userRepository = userRepository;
            _notificationService = notificationService;
        }
        public async Task<ApiResponse<string>> RegisterAsync(RegisterRequest registerRequest)
        {
            var userExists = await _userManager.FindByEmailAsync(registerRequest.Email) != null;
            if (userExists)
                return ApiResponse<string>.ErrorResponse(null, "Email này đã được sử dụng!");

            var role = registerRequest.Role switch
            {
                "Member" => UserRoleEnum.Member.ToString(),
                "BusinessOwner" => UserRoleEnum.BusinessOwner.ToString(),
                _ => throw new ArgumentException("Invalid role type.")
            };

            var user = new User
            {
                Email = registerRequest.Email,
                UserName = registerRequest.Email,
                FullName = registerRequest.FullName,
                PhoneNumber = registerRequest.PhoneNumber,
                SecurityStamp = Guid.NewGuid().ToString(),
                EmailConfirmed = false, // Email chưa được xác thực
                CreatedAt = DateTime.UtcNow,
                IsApproved = role == UserRoleEnum.Member.ToString() // Member được approve ngay, BusinessOwner cần chờ admin
            };

            if (role == UserRoleEnum.BusinessOwner.ToString())
            {
                user.Organization = registerRequest.Organization;
                user.BusinessLicense = registerRequest.BusinessLicense;
            }

            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, registerRequest.Password);

            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                throw new RegistrationFailedException(result.Errors.Select(e => e.Description));
            }
            
            // Thêm user vào role tương ứng
            await _userManager.AddToRoleAsync(user, role);

            // Generate email confirmation token
            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Gửi email xác nhận bằng Hangfire
            var confirmationUrl = $"http://localhost:3000/confirm-email?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(confirmationToken)}";
            var emailBody = EmailNotificationTemplate.ConfirmMailNotification(user.FullName, confirmationUrl);

            _notificationService.SendEmailNotification(
                user.Email,
                "Xác nhận email - Meeting Support Platform",
                emailBody
            );

            // Thông báo khác nhau tùy theo role
            var message = role == UserRoleEnum.Member.ToString() 
                ? "Người dùng đã được tạo thành công! Vui lòng kiểm tra email để xác nhận tài khoản của bạn."
                : "Người dùng đã được tạo thành công! Vui lòng kiểm tra email để xác nhận tài khoản của bạn. Tài khoản của bạn sẽ được admin xem xét trước khi bạn có thể đăng nhập.";

            return ApiResponse<string>.SuccessResponse(message, "Đăng kí tài khoản thành công! Vui lòng kiểm tra email để xác nhận tài khoản của bạn.");
        }

        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest loginRequest)
        {
            var user = await _userManager.FindByEmailAsync(loginRequest.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequest.Password))
            {
                return ApiResponse<LoginResponse>.ErrorResponse(null, "Sai email hoặc mặt khẩu.");
            }
            if (!user.EmailConfirmed)
            {
                return ApiResponse<LoginResponse>.ErrorResponse(null, "Vui lòng kiểm tra và xác nhận email trước khi đăng nhập.");
            }
            
            // Kiểm tra trạng thái approval cho BusinessOwner
            var roles = (await _userManager.GetRolesAsync(user)).ToArray();
            if (roles.Contains(UserRoleEnum.BusinessOwner.ToString()) && !user.IsApproved)
            {
                return ApiResponse<LoginResponse>.ErrorResponse(null, "Tài khoản của bạn đang chờ được admin duyệt. Vui lòng chờ duyệt trước khi đăng nhập.");
            }
            
            var (jwtToken, expiresAtUtc) = _authTokenProcessor.GenerateJwtToken(user, roles);
            var refreshToken = _authTokenProcessor.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiresAtUtc = refreshTokenExpiry;
            await _userManager.UpdateAsync(user);

            _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("ACCESS_TOKEN", jwtToken, expiresAtUtc);
            _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("REFRESH_TOKEN", refreshToken, refreshTokenExpiry);
            var response = new LoginResponse
            {
                AccessToken = jwtToken,
                RefreshToken = refreshToken
            };
            return ApiResponse<LoginResponse>.SuccessResponse(response, "Đăng nhập thành công.");
        }

        public async Task<ApiResponse<LoginResponse>> GoogleLoginAsync(GoogleLoginRequest googleLoginRequest)
        {
            // Check if user exists by Google ID
            var existingUser = await _userRepository.GetUserByGoogleIdAsync(googleLoginRequest.GoogleId);

            if (existingUser != null)
            {
                // User exists, check approval status
                var existingRoles = (await _userManager.GetRolesAsync(existingUser)).ToArray();
                var (existingJwtToken, existingExpiresAtUtc) = _authTokenProcessor.GenerateJwtToken(existingUser, existingRoles);
                var existingRefreshToken = _authTokenProcessor.GenerateRefreshToken();
                var existingRefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                existingUser.RefreshToken = existingRefreshToken;
                existingUser.RefreshTokenExpiresAtUtc = existingRefreshTokenExpiry;
                await _userManager.UpdateAsync(existingUser);
                _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("ACCESS_TOKEN", existingJwtToken, existingExpiresAtUtc);
                _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("REFRESH_TOKEN", existingRefreshToken, existingRefreshTokenExpiry);
                var existingResponse = new LoginResponse
                {
                    AccessToken = existingJwtToken,
                    RefreshToken = existingRefreshToken
                };
                return ApiResponse<LoginResponse>.SuccessResponse(existingResponse, "Google login successful.");
            }

            // Check if user exists by email but not Google ID (merge accounts)
            var userByEmail = await _userManager.FindByEmailAsync(googleLoginRequest.Email);
            if (userByEmail != null)
            {
                // Link Google account to existing user
                userByEmail.GoogleId = googleLoginRequest.GoogleId;
                userByEmail.Provider = "Google";
                userByEmail.AvatarUrl = googleLoginRequest.AvatarUrl;
                await _userManager.UpdateAsync(userByEmail);

                var emailRoles = (await _userManager.GetRolesAsync(userByEmail)).ToArray();
                var (emailJwtToken, emailExpiresAtUtc) = _authTokenProcessor.GenerateJwtToken(userByEmail, emailRoles);
                var emailRefreshToken = _authTokenProcessor.GenerateRefreshToken();
                var emailRefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                userByEmail.RefreshToken = emailRefreshToken;
                userByEmail.RefreshTokenExpiresAtUtc = emailRefreshTokenExpiry;
                await _userManager.UpdateAsync(userByEmail);
                _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("ACCESS_TOKEN", emailJwtToken, emailExpiresAtUtc);
                _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("REFRESH_TOKEN", emailRefreshToken, emailRefreshTokenExpiry);
                var emailResponse = new LoginResponse
                {
                    AccessToken = emailJwtToken,
                    RefreshToken = emailRefreshToken
                };
                return ApiResponse<LoginResponse>.SuccessResponse(emailResponse, "Google account linked successfully.");
            }

            return ApiResponse<LoginResponse>.ErrorResponse(null, "Account doesn't exist in system.");
        }

        public async Task<ApiResponse<RefreshTokenResponse>> RefreshTokenAsync(string? refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new RefreshTokenException("Refresh token is missing.");
            }
            var user = await _userRepository.GetUserByRefreshTokenAsync(refreshToken);
            if (user == null)
            {
                throw new RefreshTokenException("Unable to retrieve user for refresh token.");
            }
            if (user.RefreshTokenExpiresAtUtc < DateTime.UtcNow)
            {
                throw new RefreshTokenException("Refresh token has expired. Please log in again.");
            }
            var roles = (await _userManager.GetRolesAsync(user)).ToArray();
            var (jwtToken, expiresAtUtc) = _authTokenProcessor.GenerateJwtToken(user, roles);
            var newRefreshToken = _authTokenProcessor.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiresAtUtc = refreshTokenExpiry;
            await _userManager.UpdateAsync(user);
            _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("ACCESS_TOKEN", jwtToken, expiresAtUtc);
            _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("REFRESH_TOKEN", newRefreshToken, refreshTokenExpiry);
            var rs = new RefreshTokenResponse
            {
                AccessToken = jwtToken,
                RefreshToken = newRefreshToken
            };
            return ApiResponse<RefreshTokenResponse>.SuccessResponse(rs, "Refresh Token successful.");
        }

        public async Task<ApiResponse<string>> ConfirmEmailAsync(ConfirmEmailRequest confirmEmailRequest)
        {
            var user = await _userManager.FindByEmailAsync(confirmEmailRequest.Email);
            if (user == null)
            {
                return ApiResponse<string>.ErrorResponse(null, "User not found.");
            }

            if (user.EmailConfirmed)
            {
                return ApiResponse<string>.SuccessResponse("Email already confirmed.", "Email đã được xác nhận thành công.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, confirmEmailRequest.Token);
            if (!result.Succeeded)
            {
                return ApiResponse<string>.ErrorResponse(null, "Invalid confirmation token.");
            }

            // Tạo notification in-app lưu vào DB
            await _notificationService.CreateInAppNotificationAsync(new CreateNotificationRequest
            {
                UserId = user.Id,
                Title = "Chào mừng đến với MSP",
                Message = $"Hi {user.FullName}, chào mừng bạn đến với nền tảng hỗ trợ các cuộc họp!",
                Type = MSP.Shared.Enums.NotificationTypeEnum.InApp.ToString(),
                Data = $"{{\"eventType\":\"EmailConfirmed\",\"userId\":\"{user.Id}\"}}"
            });

            return ApiResponse<string>.SuccessResponse("Email confirmed successfully!", "Email đã được xác nhận thành công.");
        }

        public async Task<ApiResponse<string>> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest resendRequest)
        {
            var user = await _userManager.FindByEmailAsync(resendRequest.Email);
            if (user == null)
            {
                return ApiResponse<string>.ErrorResponse(null, "User not found.");
            }

            if (user.EmailConfirmed)
            {
                return ApiResponse<string>.SuccessResponse("Email already confirmed.", "Email is already confirmed.");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            return ApiResponse<string>.SuccessResponse("Confirmation email sent successfully!", "Please check your email for confirmation instructions.");
        }



    }
}
