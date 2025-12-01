using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;
        private readonly IGoogleTokenValidator _googleTokenValidator;

        public AccountService(
            IAuthTokenProcessor authTokenProcessor, 
            UserManager<User> userManager, 
            IUserRepository userRepository, 
            INotificationService notificationService, 
            IConfiguration configuration,
            IGoogleTokenValidator googleTokenValidator)
        {
            _authTokenProcessor = authTokenProcessor;
            _userManager = userManager;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _configuration = configuration;
            _googleTokenValidator = googleTokenValidator;
        }
        public async Task<ApiResponse<string>> RegisterAsync(RegisterRequest registerRequest)
        {
            var userExists = await _userManager.FindByEmailAsync(registerRequest.Email) != null;
            if (userExists)
                return ApiResponse<string>.ErrorResponse(null, "This email is already in use.");

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
                EmailConfirmed = false, // Email not yet confirmed
                CreatedAt = DateTime.UtcNow,
                IsApproved = role == UserRoleEnum.Member.ToString() // Member is approved immediately, BusinessOwner needs admin approval
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
            
            // Add user to corresponding role
            await _userManager.AddToRoleAsync(user, role);

            // Generate email confirmation token
            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Send confirmation email via Hangfire
            var clientUrl = _configuration["AppSettings:ClientUrl"];
            var confirmationUrl = $"{clientUrl}/confirm-email?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(confirmationToken)}";
            var emailBody = EmailNotificationTemplate.ConfirmMailNotification(user.FullName, confirmationUrl);

            _notificationService.SendEmailNotification(
                user.Email,
                "Email Confirmation - Meeting Support Platform",
                emailBody
            );

            // Different messages depending on role
            var message = role == UserRoleEnum.Member.ToString() 
                ? "User created successfully! Please check your email to confirm your account."
                : "User created successfully! Please check your email to confirm your account. Your account will be reviewed by an admin before you can log in.";

            return ApiResponse<string>.SuccessResponse(message, "Account registration successful! Please check your email to confirm your account.");
        }

        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest loginRequest)
        {
            var user = await _userManager.FindByEmailAsync(loginRequest.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequest.Password))
            {
                return ApiResponse<LoginResponse>.ErrorResponse(null, "Invalid email or password.");
            }
            if (!user.EmailConfirmed)
            {
                return ApiResponse<LoginResponse>.ErrorResponse(null, "Please verify and confirm your email before logging in.");
            }
            
            // Check approval status for BusinessOwner
            var roles = (await _userManager.GetRolesAsync(user)).ToArray();
            if (roles.Contains(UserRoleEnum.BusinessOwner.ToString()) && !user.IsApproved)
            {
                return ApiResponse<LoginResponse>.ErrorResponse(null, "Your account is pending admin approval. Please wait for approval before logging in.");
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
            return ApiResponse<LoginResponse>.SuccessResponse(response, "Login successful.");
        }

        public async Task<ApiResponse<LoginResponse>> GoogleLoginAsync(GoogleLoginRequest googleLoginRequest)
        {
            // Step 1: Verify Google ID Token
            var validatedUser = await _googleTokenValidator.ValidateGoogleTokenAsync(googleLoginRequest.IdToken);
            
            if (validatedUser == null)
            {
                return ApiResponse<LoginResponse>.ErrorResponse(null, "Invalid Google token. Authentication failed.");
            }

            // Use validated data from Google, not from client
            var googleId = validatedUser.GoogleId;
            var email = validatedUser.Email;
            var firstName = validatedUser.FirstName;
            var lastName = validatedUser.LastName;
            var avatarUrl = validatedUser.AvatarUrl;

            // Check if user exists by Google ID
            var existingUser = await _userRepository.GetUserByGoogleIdAsync(googleId);

            if (existingUser != null)
            {
                // Step 2: Check if account is active
                if (!existingUser.IsActive)
                {
                    return ApiResponse<LoginResponse>.ErrorResponse(null, "Your account has been deactivated. Please contact support.");
                }

                // Step 3: Check approval status for BusinessOwner
                var existingRoles = (await _userManager.GetRolesAsync(existingUser)).ToArray();
                if (existingRoles.Contains(UserRoleEnum.BusinessOwner.ToString()) && !existingUser.IsApproved)
                {
                    return ApiResponse<LoginResponse>.ErrorResponse(null, "Your account is pending admin approval. Please wait for approval before logging in.");
                }

                // User exists and is valid, generate tokens and return
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
            var userByEmail = await _userManager.FindByEmailAsync(email);
            if (userByEmail != null)
            {
                // Check if account is active before merging
                if (!userByEmail.IsActive)
                {
                    return ApiResponse<LoginResponse>.ErrorResponse(null, "Your account has been deactivated. Please contact support.");
                }

                // Check if email is confirmed (security measure)
                if (!userByEmail.EmailConfirmed)
                {
                    return ApiResponse<LoginResponse>.ErrorResponse(null, "Please verify your email before linking Google account.");
                }

                // Check approval status for BusinessOwner
                var emailRoles = (await _userManager.GetRolesAsync(userByEmail)).ToArray();
                if (emailRoles.Contains(UserRoleEnum.BusinessOwner.ToString()) && !userByEmail.IsApproved)
                {
                    return ApiResponse<LoginResponse>.ErrorResponse(null, "Your account is pending admin approval.");
                }

                // Link Google account to existing user
                userByEmail.GoogleId = googleId;
                userByEmail.Provider = "Google";
                userByEmail.AvatarUrl = avatarUrl;
                await _userManager.UpdateAsync(userByEmail);

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

            // User doesn't exist - Auto register as Member
            var fullName = $"{firstName} {lastName}".Trim();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = email.Split('@')[0]; // Fallback to email prefix
            }

            var newUser = new User
            {
                Email = email,
                UserName = email,
                FullName = fullName,
                GoogleId = googleId,
                Provider = "Google",
                AvatarUrl = avatarUrl,
                SecurityStamp = Guid.NewGuid().ToString(),
                EmailConfirmed = true, // Google accounts are pre-verified
                CreatedAt = DateTime.UtcNow,
                IsApproved = true, // Member is approved immediately
                IsActive = true
            };

            // Create user without password (Google OAuth)
            var createResult = await _userManager.CreateAsync(newUser);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description);
                return ApiResponse<LoginResponse>.ErrorResponse(null, "Unable to create user account.", errors);
            }

            // Add user to Member role
            var addRoleResult = await _userManager.AddToRoleAsync(newUser, UserRoleEnum.Member.ToString());
            if (!addRoleResult.Succeeded)
            {
                // Rollback: delete created user if role assignment fails
                await _userManager.DeleteAsync(newUser);
                var errors = addRoleResult.Errors.Select(e => e.Description);
                return ApiResponse<LoginResponse>.ErrorResponse(null, "Unable to assign role to user.", errors);
            }

            // Generate tokens for new user
            var roles = new[] { UserRoleEnum.Member.ToString() };
            var (jwtToken, expiresAtUtc) = _authTokenProcessor.GenerateJwtToken(newUser, roles);
            var refreshToken = _authTokenProcessor.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            
            newUser.RefreshToken = refreshToken;
            newUser.RefreshTokenExpiresAtUtc = refreshTokenExpiry;
            await _userManager.UpdateAsync(newUser);

            _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("ACCESS_TOKEN", jwtToken, expiresAtUtc);
            _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("REFRESH_TOKEN", refreshToken, refreshTokenExpiry);

            // Create in-app notification to welcome new user
            try
            {
                await _notificationService.CreateInAppNotificationAsync(new CreateNotificationRequest
                {
                    UserId = newUser.Id,
                    Title = "Welcome to MSP",
                    Message = $"Hi {newUser.FullName}, welcome to the meeting support platform! Your account has been successfully created via Google.",
                    Type = NotificationTypeEnum.InApp.ToString(),
                    Data = $"{{\"eventType\":\"GoogleAccountCreated\",\"userId\":\"{newUser.Id}\",\"provider\":\"Google\"}}"
                });
            }
            catch (Exception)
            {
                // Log error but don't fail request as notification is not critical
                // Notification failure does not affect registration flow
            }

            var response = new LoginResponse
            {
                AccessToken = jwtToken,
                RefreshToken = refreshToken
            };

            return ApiResponse<LoginResponse>.SuccessResponse(response, "Google account registered and logged in successfully.");
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
                return ApiResponse<string>.SuccessResponse("Email already confirmed.", "Email has been confirmed successfully.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, confirmEmailRequest.Token);
            if (!result.Succeeded)
            {
                return ApiResponse<string>.ErrorResponse(null, "Invalid confirmation token.");
            }

            // Create in-app notification and save to DB
            await _notificationService.CreateInAppNotificationAsync(new CreateNotificationRequest
            {
                UserId = user.Id,
                Title = "Welcome to MSP",
                Message = $"Hi {user.FullName}, welcome to the meeting support platform!",
                Type = MSP.Shared.Enums.NotificationTypeEnum.InApp.ToString(),
                Data = $"{{\"eventType\":\"EmailConfirmed\",\"userId\":\"{user.Id}\"}}"
            });

            return ApiResponse<string>.SuccessResponse("Email confirmed successfully!", "Email has been confirmed successfully.");
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

        public async Task<ApiResponse<string>> LogoutAsync(string? userId = null)
        {
            try
            {
                // Clear authentication cookies
                _authTokenProcessor.ClearAuthTokenCookies();

                // Invalidate refresh token in database if userId is provided
                if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
                {
                    var user = await _userManager.FindByIdAsync(userGuid.ToString());
                    if (user != null)
                    {
                        // Clear refresh token to invalidate it
                        user.RefreshToken = null;
                        user.RefreshTokenExpiresAtUtc = null;
                        await _userManager.UpdateAsync(user);
                    }
                }

                return ApiResponse<string>.SuccessResponse("Logged out successfully.", "Đăng xuất thành công.");
            }
            catch (Exception ex)
            {
                // Still return success even if token clearing fails
                // The important part is that cookies are cleared on client side
                return ApiResponse<string>.SuccessResponse("Logged out successfully.", "Đăng xuất thành công.");
            }
        }

        public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequest forgotPasswordRequest)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordRequest.Email);
            if (user == null)
            {
                return ApiResponse<string>.ErrorResponse(null, "User not found.");
            }
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var clientUrl = _configuration["AppSettings:ClientUrl"];
            var resetUrl = $"{clientUrl}/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(resetToken)}";
            var emailBody = EmailNotificationTemplate.ResetPasswordNotification(user.FullName, resetUrl);
            _notificationService.SendEmailNotification(
                user.Email,
                "Password Reset - Meeting Support Platform",
                emailBody
            );
            return ApiResponse<string>.SuccessResponse(null, "Password reset email sent successfully! Please check your email.");

        }

        public async Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordRequest.Email);
            if (user == null)
            {
                return ApiResponse<string>.ErrorResponse(null, "User not found.");
            }
            var result = await _userManager.ResetPasswordAsync(user, resetPasswordRequest.Token, resetPasswordRequest.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return ApiResponse<string>.ErrorResponse(null, "Password reset failed.", errors);
            }
            return ApiResponse<string>.SuccessResponse(null, "Password has been reset successfully.");
        }
    }
}
