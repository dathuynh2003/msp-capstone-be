using System.ComponentModel.DataAnnotations;

namespace MSP.Application.Models.Requests.Auth
{
    public class GoogleLoginRequest
    {
        /// <summary>
        /// Google ID Token from client (JWT token issued by Google)
        /// This must be verified server-side for security
        /// </summary>
        [Required]
        public string IdToken { get; set; } = string.Empty;

        // These fields will be extracted from verified token, not trusted from client
        public string GoogleId { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }
    }
}
