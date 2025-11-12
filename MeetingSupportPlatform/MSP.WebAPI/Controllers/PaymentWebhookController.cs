using Microsoft.AspNetCore.Mvc;
using MSP.Application.Models.Requests.Payment;
using MSP.Application.Models.Responses.Payment;
using MSP.Application.Services.Interfaces.Payment;
using PayOS;
using PayOS.Exceptions;
using PayOS.Models.Webhooks;

namespace MSP.WebAPI.Controllers
{
    [Route("api/v1/payment")]
    [ApiController]
    public class PaymentWebhookController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly PayOSClient _payOSClient;
        private readonly ILogger<PaymentWebhookController> _logger;

        public PaymentWebhookController(
            IPaymentService paymentService,
            PayOSClient payOSClient,
            ILogger<PaymentWebhookController> logger)
        {
            _paymentService = paymentService;
            _payOSClient = payOSClient;
            _logger = logger;
        }

        /// <summary>
        /// Webhook endpoint - PayOS gọi vào đây khi thanh toán hoàn tất
        /// </summary>
        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook([FromBody] WebhookRequest request)
        {

            try
            {
                // 1. Verify webhook với Webhooks.VerifyAsync()
                var rs = _paymentService.VerifyPayOSWebhook(request);
                if (!rs)
                {
                    return BadRequest(new { error = "Invalid signature" });
                }
                // 2. Parse dữ liệu
                var paymentData = new PaymentWebhookData
                {
                    OrderCode = request.Data.OrderCode,
                    Amount = request.Data.Amount,
                    Description = request.Data.Description,
                    TransactionDateTime = request.Data.TransactionDateTime,
                    Reference = request.Data.Reference,
                    Status = request.Data.Code == "00" ? "PAID" : "CANCELLED"
                };


                // 3. Xử lý cập nhật Subscription
                bool result = await _paymentService.HandlePaymentWebhookAsync(paymentData);

                if (result)
                {
                    _logger.LogInformation("Webhook processed successfully for OrderCode: {OrderCode}", paymentData.OrderCode);
                    return Ok(new { success = true, message = "Webhook processed" });
                }

                _logger.LogWarning("Failed to process webhook for OrderCode: {OrderCode}", paymentData.OrderCode);
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
        [HttpPost("webhook/confirm")]
        public async Task<IActionResult> ConfirmWebhook([FromQuery] string webhookUrl)
        {

            try
            {
                //var result = await _payOSClient.Webhooks.ConfirmAsync(webhookUrl);
                var result = await _paymentService.ConfirmWebhookAsync(webhookUrl);

                _logger.LogInformation("Webhook confirmed successfully: {@Result}", result);

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while confirming webhook URL: {WebhookUrl}", webhookUrl);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
