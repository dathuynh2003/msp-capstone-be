using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Responses.Payment
{
    public class PaymentWebhookData
    {
        public long OrderCode { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string TransactionDateTime { get; set; }
        public string Reference { get; set; }          // Transaction ID từ PayOS
        public string Status { get; set; }             // PAID, CANCELLED, PENDING
    }
}
