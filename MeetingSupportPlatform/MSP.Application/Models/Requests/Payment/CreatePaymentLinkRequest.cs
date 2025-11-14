using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MSP.Application.Models.Requests.Payment
{
    public class CreatePaymentLinkRequest
    {
        [JsonPropertyName("orderCode")]
        public long OrderCode { get; set; }

        [JsonPropertyName("returnUrl")]
        public string ReturnUrl { get; set; }

        [JsonPropertyName("cancelUrl")]
        public string CancelUrl { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("signature")]
        public string Signature { get; set; }
    }
}
