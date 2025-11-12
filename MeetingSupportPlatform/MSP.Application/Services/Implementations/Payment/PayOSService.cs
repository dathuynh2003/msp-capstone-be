using FFMpegCore;
using Microsoft.Extensions.Options;
using MSP.Application.Models.Requests.Payment;
using MSP.Application.Models.Responses.Payment;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Payment;
using MSP.Shared.Common;
using MSP.Shared.Enums;
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
        private string ClientId => _options.ClientId;
        private string ApiKey => _options.ApiKey;
        private string ChecksumKey => _options.ChecksumKey;
        private string BaseUrl => _options.BaseUrl;
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

                var res = await client.PostAsync($"{BaseUrl}/v2/payment-requests", content);
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

        public async Task<bool> HandlePaymentWebhookAsync(JsonElement payload)
        {
            var data = payload.GetProperty("data");
            var orderCode = data.GetProperty("orderCode").GetInt32();
            var amount = data.GetProperty("amount").GetInt32();
            var code = data.GetProperty("code").GetString();
            var desc = data.GetProperty("desc").GetString();
            var reference = data.GetProperty("reference").GetString();
            var transactionDateTime = data.GetProperty("transactionDateTime").GetString();
            var subscription = await _subscriptionRepository.GetByOrderCodeAsync(orderCode);

            if (subscription == null)
                return false;

            if (subscription.Status == PaymentEnum.Paid.ToString().ToUpper() || subscription.Status == PaymentEnum.Cancelled.ToString().ToUpper())
                return true; // Đã xử lý rồi
            if (code == "00")
            {
                subscription.Status = PaymentEnum.Paid.ToString().ToUpper();
                subscription.PaidAt = DateTime.Parse(transactionDateTime);
                subscription.PaymentMethod = "PayOS";
                subscription.TransactionID = reference;
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


        public bool VerifyPayOSWebhook(JsonElement payload)
        {
            var data = payload.GetProperty("data");

            // Tạo dictionary chứa tất cả các key-value trong "data"
            var dict = new SortedDictionary<string, string>();
            foreach (var prop in data.EnumerateObject())
            {
                dict[prop.Name] = prop.Value.GetRawText().Trim('"');
            }

            // Build raw data string: key1=value1&key2=value2&...
            var rawBuilder = new StringBuilder();
            foreach (var kvp in dict)
            {
                rawBuilder.Append($"{kvp.Key}={kvp.Value}&");
            }
            rawBuilder.Length--; // Bỏ dấu '&' cuối cùng

            var rawData = rawBuilder.ToString();
            var generatedSignature = GenerateSignature(rawData);
            var providedSignature = payload.GetProperty("signature").GetString();

            return generatedSignature == providedSignature;
        }

        public async Task<bool> ConfirmWebhookAsync(string webhookUrl)
        {
            try
            {
                var payload = new { webhookUrl = webhookUrl };
                var json = JsonSerializer.Serialize(payload);
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("x-client-id", ClientId);
                client.DefaultRequestHeaders.Add("x-api-key", ApiKey);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var res = await client.PostAsync($"{BaseUrl}/confirm-webhook", content);
                var responseBody = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"PayOS response: {responseBody}");
                return res.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while confirming webhook: {ex.Message}");
                return false;
            }
        }


    }
}
