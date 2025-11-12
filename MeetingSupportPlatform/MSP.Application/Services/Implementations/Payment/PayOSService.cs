using FFMpegCore;
using Microsoft.Extensions.Options;
using MSP.Application.Models.Requests.Payment;
using MSP.Application.Models.Responses.Payment;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Payment;
using MSP.Shared.Common;
using PayOS;
using PayOS.Exceptions;
using PayOS.Models;
using PayOS.Models.V2.PaymentRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace MSP.Application.Services.Implementations.Payment
{
    public class PayOSConfiguration
    {
        public string ClientId { get; set; }
        public string ApiKey { get; set; }
        public string ChecksumKey { get; set; }
        public string BaseUrl { get; set; }
    }

    public class PayOSService : IPaymentService
    {
        private readonly PayOSConfiguration _options;
        private readonly ISubscriptionRepository _subscriptionRepository;
        public PayOSService(IOptions<PayOSConfiguration> options, ISubscriptionRepository subscriptionRepository)
        {
            _options = options.Value;
            _subscriptionRepository = subscriptionRepository;
        }
        public string ClientId => _options.ClientId;
        public string ApiKey => _options.ApiKey;
        public string ChecksumKey => _options.ChecksumKey;
        public string BaseUrl => _options.BaseUrl;

        public async Task<PaymentResponse> CreatePaymentLinkAsync(CreatePaymentRequest request)
        {
            
            // create order code
            long generateOrderCode = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var raw = $"amount={request.Amount}&cancelUrl={request.CancelUrl}&description={request.Description}&orderCode={generateOrderCode}&returnUrl={request.ReturnUrl}";
            var sig = GenerateSignature(raw);
            var body = new 
            {
                orderCode = generateOrderCode,
                amount = request.Amount,
                description = request.Description,
                returnUrl = request.ReturnUrl,
                cancelUrl = request.CancelUrl,
                signature = sig,
            };
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("x-client-id", ClientId);
                client.DefaultRequestHeaders.Add("x-api-key", ApiKey);

                var res = await client.PostAsync($"{BaseUrl}/payment-requests", content);
                res.EnsureSuccessStatusCode(); // ném HttpRequestException nếu lỗi HTTP

                var responseJson = await res.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(responseJson);
                var dataElement = doc.RootElement.GetProperty("data");
                var checkoutUrl = dataElement.GetProperty("checkoutUrl").GetString();
                var qrCode = dataElement.GetProperty("qrCode").GetString();
                var status = dataElement.GetProperty("status").GetString();
                return new PaymentResponse
                {
                    CheckoutUrl = checkoutUrl,
                    QrCode = qrCode,
                    OrderCode = generateOrderCode,
                    Amount = request.Amount,
                    Status = status
                };
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Error: {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON Parse Error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                throw;
            }

        }

        public Task<PaymentStatusResponse> GetPaymentStatusAsync(long orderCode, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> HandlePaymentWebhookAsync(PaymentWebhookData webhookData, CancellationToken cancellationToken = default)
        {
            var subscription = await _subscriptionRepository.GetByOrderCodeAsync(webhookData.OrderCode);

            if (subscription == null)
                return false;

            subscription.Status = webhookData.Status.ToUpper();
            if (subscription.Status == "PAID")
            {
                subscription.PaidAt = DateTime.Parse(webhookData.TransactionDateTime);
                subscription.PaymentMethod = "PayOS";
                subscription.TransactionID = webhookData.Reference;
                subscription.StartDate = DateTime.UtcNow;
                subscription.EndDate = subscription.StartDate.Value.AddMonths(subscription.Package.BillingCycle);
            }

            subscription.UpdatedAt = DateTime.UtcNow;
            await _subscriptionRepository.UpdateAsync(subscription);
            await _subscriptionRepository.SaveChangesAsync();

            return true;
        }

        public string GenerateSignature(string rawData)
        {
            var key = Encoding.UTF8.GetBytes(ChecksumKey);
            var data = Encoding.UTF8.GetBytes(rawData);
            using var hmac = new HMACSHA256(key);
            var hash = hmac.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        public bool VerifyBankWebhook(JsonElement payload)
        {
            throw new NotImplementedException();
        }

        public bool VerifyPayOSWebhook(WebhookRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
