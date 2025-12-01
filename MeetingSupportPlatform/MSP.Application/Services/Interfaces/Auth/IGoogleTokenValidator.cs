using MSP.Application.Models.Requests.Auth;

namespace MSP.Application.Services.Interfaces.Auth
{
    public interface IGoogleTokenValidator
    {
        /// <summary>
        /// Validates Google ID token and extracts user information
        /// </summary>
        /// <param name="idToken">Google ID token from client</param>
        /// <returns>Validated Google user info or null if invalid</returns>
        Task<GoogleLoginRequest?> ValidateGoogleTokenAsync(string idToken);
    }
}
