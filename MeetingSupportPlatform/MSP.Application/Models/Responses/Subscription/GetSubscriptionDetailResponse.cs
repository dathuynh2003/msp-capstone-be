using MSP.Application.Models.Responses.Package;
using MSP.Application.Models.Responses.Users;
using MSP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Responses.Subscription
{
    public class GetSubscriptionDetailResponse
    {
        public Guid Id { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionID { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public Guid PackageId { get; set; }
        public GetPackageResponse Package { get; set; }
        public Guid UserId { get; set; }
        public GetUserResponse User { get; set; }
    }
}
