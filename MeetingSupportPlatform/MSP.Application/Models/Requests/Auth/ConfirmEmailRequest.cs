using System.ComponentModel.DataAnnotations;

namespace MSP.Application.Models.Requests.Auth
{
    public class ConfirmEmailRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;
    }
}
