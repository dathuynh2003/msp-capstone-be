using MSP.Application.Models.Requests.Auth;
using MSP.Application.Services.Interfaces.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MSP.Application.Models.Requests;
using MSP.Shared.Enums;
using MSP.WebAPI.Filters;

namespace MSP.WebAPI.Controllers
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AuthController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _accountService.RegisterAsync(registerRequest);
            return Ok(result);
        }

        [HttpPost("login")]
        [RateLimit(maxRequests: 5, timeWindowSeconds: 60)] // 5 requests per minute
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _accountService.LoginAsync(loginRequest);
            return Ok(result);
        }

        [HttpPost("google-login")]
        [RateLimit(maxRequests: 10, timeWindowSeconds: 60)] // 10 requests per minute for Google login
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest googleLoginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _accountService.GoogleLoginAsync(googleLoginRequest);
            return Ok(result);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return BadRequest("Email and token are required.");
            }

            var confirmEmailRequest = new ConfirmEmailRequest
            {
                Email = email,
                Token = token
            };

            var result = await _accountService.ConfirmEmailAsync(confirmEmailRequest);
            return Ok(result);
        }

        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailRequest resendRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _accountService.ResendConfirmationEmailAsync(resendRequest);
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest("Refresh token is required.");
            }
            var rs = await _accountService.RefreshTokenAsync(request.RefreshToken);
            return Ok(rs);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            var result = await _accountService.LogoutAsync(userIdClaim);
            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest forgotPasswordRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _accountService.ForgotPasswordAsync(forgotPasswordRequest);
            return Ok(result);

        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest resetPasswordRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _accountService.ResetPasswordAsync(resetPasswordRequest);
            return Ok(result);
        }
    }
}
