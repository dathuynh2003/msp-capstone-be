using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Responses.Subscription
{
    public class GetSubscriptionResponse
    {
        public Guid SubscriptionId { get; set; }
        public string PaymentUrl { get; set; }       // Link thanh toán
        public string QrCodeUrl { get; set; }        // QR để quét Momo/Banking
        public long OrderCode { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime ExpiresAt { get; set; }      // Link hết hạn sau 15 phút
    }
}
