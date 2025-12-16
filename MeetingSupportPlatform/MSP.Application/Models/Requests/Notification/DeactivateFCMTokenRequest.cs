using System.ComponentModel.DataAnnotations;

namespace MSP.Application.Models.Requests.Notification
{
    public class DeactivateFCMTokenRequest
    {
        [Required(ErrorMessage = "FCM Token is required")]
        public string FCMToken { get; set; }
    }
}
