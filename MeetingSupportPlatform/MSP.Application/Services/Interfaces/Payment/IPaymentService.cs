using MSP.Application.Models.Requests.Payment;
using MSP.Application.Models.Responses.Payment;
using MSP.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MSP.Application.Services.Interfaces.Payment
{
    public interface IPaymentService
    {
        Task<PaymentResponse> CreatePaymentLinkAsync(CreatePaymentLinkRequest request);
        Task<bool> HandlePaymentWebhookAsync(WebhookRequest payload);
        bool VerifyPayOSWebhook(WebhookRequest payload);
    }
}
