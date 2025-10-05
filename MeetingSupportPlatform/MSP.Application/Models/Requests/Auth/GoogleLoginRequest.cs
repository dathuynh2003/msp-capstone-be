using System.ComponentModel.DataAnnotations;

namespace MSP.Application.Models.Requests.Auth
{
    public class GoogleLoginRequest
    {
        [Required]
        public string GoogleId { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }
    }
}
