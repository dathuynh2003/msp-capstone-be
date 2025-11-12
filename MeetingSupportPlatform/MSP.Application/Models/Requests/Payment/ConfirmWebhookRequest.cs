using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Requests.Payment
{
    public class ConfirmWebhookRequest
    {
        public string WebhookUrl { get; set; } = "";
    }
}
