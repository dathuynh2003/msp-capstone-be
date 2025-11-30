using MSP.Application.Models.Requests.Auth;
using MSP.Application.Models.Responses.Auth;
using MSP.Shared.Common;

namespace MSP.Application.Services.Interfaces.Auth
{
    public interface IAccountService
    {
        Task<ApiResponse<string>> RegisterAsync(RegisterRequest registerRequest);
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest loginRequest);
        Task<ApiResponse<LoginResponse>> GoogleLoginAsync(GoogleLoginRequest googleLoginRequest);
        Task<ApiResponse<RefreshTokenResponse>> RefreshTokenAsync(string? refreshToken);
        Task<ApiResponse<string>> ConfirmEmailAsync(ConfirmEmailRequest confirmEmailRequest);
        Task<ApiResponse<string>> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest resendRequest);
        Task<ApiResponse<string>> LogoutAsync(string? userId = null);

        Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequest forgotPasswordRequest);
        Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest);

    }
}
