using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Responses.Payment
{
    public class PaymentResponse
    {
        public string CheckoutUrl { get; set; }       // URL để user truy cập thanh toán
        public string QrCode { get; set; }            // QR code để quét
        public long OrderCode { get; set; }           // Mã đơn hàng
        public int Amount { get; set; }
        public string Status { get; set; }
    }
}
