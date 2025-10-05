using System.ComponentModel.DataAnnotations;

namespace MSP.Application.Models.Requests.Auth
{
    public class ResendConfirmationEmailRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
