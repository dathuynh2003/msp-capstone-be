using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MSP.Application.Models.Responses.Payment
{
    public class PayOSPaymentResponse
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("desc")]
        public string Desc { get; set; }

        [JsonPropertyName("data")]
        public PayOSPaymentData Data { get; set; }

        [JsonPropertyName("signature")]
        public string Signature { get; set; }
    }

    public class PayOSPaymentData
    {
        [JsonPropertyName("checkoutUrl")]
        public string CheckoutUrl { get; set; }

        [JsonPropertyName("qrCode")]
        public string QrCode { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("orderCode")]
        public long OrderCode { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }
    }
}
