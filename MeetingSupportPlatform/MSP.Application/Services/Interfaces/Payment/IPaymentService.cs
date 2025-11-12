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
        Task<PaymentResponse> CreatePaymentLinkAsync(CreatePaymentRequest request);
        Task<bool> HandlePaymentWebhookAsync(PaymentWebhookData webhookData);
        Task<PaymentStatusResponse> GetPaymentStatusAsync(long orderCode, CancellationToken cancellationToken = default);
        bool VerifyPayOSWebhook(WebhookRequest request);
        Task<bool> ConfirmWebhookAsync(string webhookUrl);
    }
}
