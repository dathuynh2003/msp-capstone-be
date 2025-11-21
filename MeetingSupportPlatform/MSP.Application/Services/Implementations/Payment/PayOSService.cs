using FFMpegCore;
using Microsoft.Extensions.Logging;
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
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        private readonly ILogger<PayOSService> _logger;
        public PayOSService(IOptions<PayOSConfiguration> options, ISubscriptionRepository subscriptionRepository, ILogger<PayOSService> logger)
        {
            _options = options.Value;
            _subscriptionRepository = subscriptionRepository;
            _logger = logger;
        }
        private string ClientId => _options.ClientId;
        private string ApiKey => _options.ApiKey;
        private string ChecksumKey => _options.ChecksumKey;
        private string BaseUrl => _options.BaseUrl;

        // tạo payment link
        public async Task<PaymentResponse> CreatePaymentLinkAsync(CreatePaymentLinkRequest request)
        {
            request.OrderCode = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var raw = $"amount={request.Amount}&cancelUrl={request.CancelUrl}&description={request.Description}&orderCode={request.OrderCode}&returnUrl={request.ReturnUrl}";
            request.Signature = GenerateSignature(raw);
  

            // body request
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("x-client-id", ClientId);
                client.DefaultRequestHeaders.Add("x-api-key", ApiKey);

                var response = await client.PostAsync($"{BaseUrl}/v2/payment-requests", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var payOSResponse = JsonSerializer.Deserialize<PayOSPaymentResponse>(responseJson);

                return new PaymentResponse
                {
                    CheckoutUrl = payOSResponse.Data.CheckoutUrl,
                    QrCode = payOSResponse.Data.QrCode,
                    OrderCode = request.OrderCode,
                    Amount = request.Amount,
                    Status = payOSResponse.Data.Status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment link");
                throw;
            }
        }
        // xử lí webhook thanh toán 
        public async Task<bool> HandlePaymentWebhookAsync(WebhookRequest payload)
        {
            try
            {
                var data = payload.Data;
                var orderCode = data.OrderCode;
                var amount = data.Amount;
                var code = data.Code;
                var desc = data.Desc;
                var reference = data.Reference;
                var transactionDateTime = data.TransactionDateTime;

                var subscription = await _subscriptionRepository.GetByOrderCodeAsync(orderCode);

                if (subscription == null)
                {
                    _logger.LogWarning("Subscription with orderCode {OrderCode} not found!", orderCode);
                    return false;
                }

                if (subscription.Status == PaymentEnum.Paid.ToString().ToUpper())
                {
                    _logger.LogInformation("Subscription already processed (Status: {Status})", subscription.Status);
                    return true;
                }

                if (code == "00")
                {
                    // inactive previous subscriptions if any
                    var activeOld = await _subscriptionRepository.GetActiveSubscriptionByUserIdAsync(subscription.UserId);
                    if (activeOld != null && activeOld.Id != subscription.Id)
                    {
                        activeOld.IsActive = false;
                        activeOld.UpdatedAt = DateTime.UtcNow;
                        await _subscriptionRepository.UpdateAsync(activeOld);
                    }
                    //activate current subscription
                    subscription.Status = PaymentEnum.Paid.ToString().ToUpper();
                    subscription.PaidAt = DateTime.ParseExact(
                        transactionDateTime,
                        "yyyy-MM-dd HH:mm:ss",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal
                    );
                    subscription.PaidAt = DateTime.UtcNow;
                    subscription.IsActive = true;
                    subscription.PaymentMethod = "PayOS";
                    subscription.TransactionID = reference;
                    subscription.StartDate = DateTime.UtcNow;
                    subscription.EndDate = subscription.StartDate.Value.AddMonths(subscription.Package.BillingCycle);
                }
                else
                {
                    _logger.LogWarning("Payment failed or unrecognized code: {Code}", code);
                }
                subscription.UpdatedAt = DateTime.UtcNow;
                await _subscriptionRepository.UpdateAsync(subscription);
                await _subscriptionRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while handling payment webhook: {Message}", ex.Message);
                return false;
            }
        }


        // xác minh chữ ký webhook(payouts)từ PayOS
        public bool VerifyPayOSWebhook(WebhookRequest payload)
        {
            try
            {
                _logger.LogInformation("Start verifying PayOS webhook...");

                // Tạo signature từ object Data
                var generatedSignature = GeneratePayoutSignature(payload.Data);
                _logger.LogInformation("Generated signature: {Generated}", generatedSignature);
                _logger.LogInformation("Provided signature: {Provided}", payload.Signature);

                return generatedSignature.Equals(payload.Signature, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during webhook verification: {Message}", ex.Message);
                return false;
            }
        }


        // tạo signature payment-request(create payment link)
        public string GenerateSignature(string rawData)
        {
            var key = Encoding.UTF8.GetBytes(ChecksumKey);
            var data = Encoding.UTF8.GetBytes(rawData);
            using var hmac = new HMACSHA256(key);
            var hash = hmac.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        // tạo signature payouts (verify webhook)
        public string GeneratePayoutSignature(WebhookPaymentData data)
        {
            var dict = new SortedDictionary<string, string>();

            foreach (var prop in typeof(WebhookPaymentData).GetProperties())
            {
                var value = prop.GetValue(data);
                string strValue = value?.ToString() ?? "";

                // Lấy tên json nếu có [JsonPropertyName], fallback prop.Name
                var jsonNameAttr = prop.GetCustomAttributes(typeof(JsonPropertyNameAttribute), false)
                                       .FirstOrDefault() as JsonPropertyNameAttribute;
                string keyName = jsonNameAttr?.Name ?? prop.Name;

                dict[keyName] = strValue;
            }

            var rawData = string.Join("&", dict.Select(kv => $"{kv.Key}={kv.Value}"));
            return GenerateSignature(rawData);
        }

    }
}
