using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Requests.Limitation
{
    public class UpdateLimitationRequest
    {
        public Guid LimitationId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsUnlimited { get; set; }
        public int LimitValue { get; set; }
        public string LimitUnit { get; set; }
    }
}
