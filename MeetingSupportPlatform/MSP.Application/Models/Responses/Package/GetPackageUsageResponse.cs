using MSP.Application.Models.Responses.Limitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Responses.Package
{
    public class GetPackageUsageResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public int BillingCycle { get; set; }
        public bool IsDeleted { get; set; }
        public List<GetLimitationUsageResponse> Limitations { get; set; } = new();
    }
}
