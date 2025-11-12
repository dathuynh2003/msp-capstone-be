using Microsoft.AspNetCore.Mvc;
using MSP.Application.Models.Requests.Payment;
using MSP.Application.Models.Responses.Payment;
using MSP.Application.Services.Interfaces.Payment;
using PayOS;
using PayOS.Exceptions;
using System.Text.Json;

namespace MSP.WebAPI.Controllers
{
    [Route("webhook-url")]
    [ApiController]
    public class PaymentWebhookController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentWebhookController> _logger;

        public PaymentWebhookController(
            IPaymentService paymentService, ILogger<PaymentWebhookController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// Webhook endpoint - PayOS gọi vào đây khi thanh toán hoàn tất
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> HandleWebhook([FromBody] JsonElement payload)
        {

            try
            {
                // 1. Verify webhook với Webhooks.VerifyAsync()
                var rs = _paymentService.VerifyPayOSWebhook(payload);
                if (!rs)
                {
                    return BadRequest(new { error = "Invalid signature" });
                }

                // 3. Xử lý cập nhật Subscription
                bool result = await _paymentService.HandlePaymentWebhookAsync(payload);

                if (result)
                {
                    return Ok(new { success = true, message = "Webhook processed" });
                }

                return BadRequest(new { success = false, message = "Failed to process" });
            }
            catch (PayOSException ex)
            {
                _logger.LogError(ex, "PayOSException occurred while handling webhook: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    errorCode = ex.GetType().Name
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in HandleWebhook: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Đăng ký webhook URL với PayOS
        /// Chỉ cần gọi 1 lần khi setup
        /// </summary>
        [HttpPost("/confirm-webhook")]
        public async Task<IActionResult> ConfirmWebhook([FromBody] ConfirmWebhookRequest request)
        {

            try
            {
                //var result = await _payOSClient.Webhooks.ConfirmAsync(webhookUrl);
                var result = await _paymentService.ConfirmWebhookAsync(request.WebhookUrl);

                _logger.LogInformation("Webhook confirmed successfully: {@Result}", result);
                if(result == false)
                {
                    return BadRequest(new { success = false, message = "Failed to confirm webhook" });
                }
                return Ok(new { success = result, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while confirming webhook URL: {WebhookUrl}", request.WebhookUrl);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
