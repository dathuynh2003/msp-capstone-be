using System.ComponentModel.DataAnnotations;

namespace MSP.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public string? Type { get; set; } // Email, SMS, Push, InApp
        
        public bool IsRead { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ReadAt { get; set; }
        
        public string? Data { get; set; } // JSON data for additional information
    }
}

