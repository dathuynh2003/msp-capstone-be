using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Requests.Subscription
{
    public class CreateSubscriptionRequest
    {
        public Guid PackageId { get; set; }
        public Guid UserId { get; set; }
        public string ReturnUrl { get; set; }       // URL redirect sau khi thanh toán thành công
        public string CancelUrl { get; set; }       // URL redirect sau khi user hủy thanh toán

    }
}
