using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Responses.Limitation
{
    public class GetLimitationUsageResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsUnlimited { get; set; }
        public int? LimitValue { get; set; }
        public string? LimitUnit { get; set; }
        public string LimitationType { get; set; }
        public int UsedValue { get; set; } // số đã dùng
    }
}
