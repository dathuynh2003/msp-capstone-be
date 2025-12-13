using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MSP.Application.Models.Requests.Auth;
using MSP.Application.Services.Interfaces.Auth;

namespace MSP.Infrastructure.Processors
{
    public class GoogleTokenValidator : IGoogleTokenValidator
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleTokenValidator> _logger;

        public GoogleTokenValidator(IConfiguration configuration, ILogger<GoogleTokenValidator> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<GoogleLoginRequest?> ValidateGoogleTokenAsync(string idToken)
        {
            try
            {
                var googleClientId = _configuration["Authentication:Google:ClientId"];
                
                if (string.IsNullOrEmpty(googleClientId))
                {
                    _logger.LogError("Google ClientId is not configured");
                    return null;
                }

                // Verify the token with Google
                var validationSettings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { googleClientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);

                if (payload == null)
                {
                    _logger.LogWarning("Google token validation returned null payload");
                    return null;
                }

                // Extract user information from verified token
                return new GoogleLoginRequest
                {
                    IdToken = idToken,
                    GoogleId = payload.Subject, // Google User ID
                    Email = payload.Email,
                    FirstName = payload.GivenName ?? "",
                    LastName = payload.FamilyName ?? "",
                    AvatarUrl = payload.Picture
                };
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning(ex, "Invalid Google JWT token");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Google token");
                return null;
            }
        }
    }
}
