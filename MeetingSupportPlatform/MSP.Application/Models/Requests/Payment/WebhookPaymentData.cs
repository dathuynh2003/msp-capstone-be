using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Requests.Payment
{
    public class WebhookPaymentData
    {
        public long OrderCode { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string AccountNumber { get; set; }
        public string Reference { get; set; }
        public string TransactionDateTime { get; set; }
        public string Currency { get; set; }
        public string PaymentLinkId { get; set; }
        public string Status { get; set; } // PAID, CANCELLED, PENDING
        public string Code { get; set; }
        public string Desc { get; set; }
    }
}
