using System.ComponentModel.DataAnnotations;

namespace MSP.Application.Models.Requests.Notification
{
    public class RegisterFCMTokenRequest
    {
        [Required(ErrorMessage = "FCM Token is required")]
        public string FCMToken { get; set; }

        [Required(ErrorMessage = "Platform is required")]
        [RegularExpression("^(Android|iOS)$", ErrorMessage = "Platform must be 'Android' or 'iOS'")]
        public string Platform { get; set; }

        public string? DeviceId { get; set; }

        public string? DeviceName { get; set; }
    }
}
