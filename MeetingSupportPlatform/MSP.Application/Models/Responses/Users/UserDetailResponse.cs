using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Responses.Users
{
    public class UserDetailResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Organization { get; set; }
        public Guid? ManagedBy { get; set; }
        public string? ManagerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string RoleName { get; set; }
    }
}
