using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Requests.Payment
{
    public class WebhookRequest
    {
        public string Code { get; set; }
        public string Desc { get; set; }
        public bool Success { get; set; }
        public WebhookPaymentData Data { get; set; }
        public string Signature { get; set; }
    }
}
