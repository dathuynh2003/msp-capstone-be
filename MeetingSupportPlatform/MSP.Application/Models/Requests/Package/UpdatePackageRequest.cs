using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Requests.Package
{
    public class UpdatePackageRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public int BillingCycle { get; set; }
        public Guid CreatedById { get; set; }
        public ICollection<Guid> LimitationIds { get; set; } = new List<Guid>();
    }
}
