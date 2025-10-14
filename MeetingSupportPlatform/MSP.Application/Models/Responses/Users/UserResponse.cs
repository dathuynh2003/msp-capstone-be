using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Responses.Users
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string? GoogleId { get; set; }
        public string? Organization { get; set; }
        public string? BusinessLicense { get; set; }
        public bool IsApproved { get; set; } = false; // Trạng thái duyệt cho BusinessOwner
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
