using MSP.Application.Models.Responses.Limitation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Responses.Package
{
    public class GetPackageResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public int BillingCycle { get; set; }
        public bool isDeleted { get; set; }
        public List<GetLimitationResponse> Limitations { get; set; } = new();
    }
}
