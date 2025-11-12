using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public PaymentWebhookController(IPaymentService paymentService, PayOSClient payOSClient)
        {
            _paymentService = paymentService;
            _payOSClient = payOSClient;
        }
        /// <summary>
        /// Webhook endpoint - PayOS gọi vào đây khi thanh toán hoàn tất
        /// </summary>

        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook([FromBody] Webhook webhookBody)
        {
            try
            {
                // 1.  Verify webhook với Webhooks.VerifyAsync()
                var webhookData = await _payOSClient.Webhooks.VerifyAsync(webhookBody);

                // 2. Parse dữ liệu
                var paymentData = new PaymentWebhookData
                {
                    OrderCode = webhookData.OrderCode,
                    Amount = webhookData.Amount,
                    Description = webhookData.Description,
                    TransactionDateTime = webhookData.TransactionDateTime,
                    Reference = webhookData.Reference,
                    Status = webhookData.Code == "00" ? "PAID" : "CANCELLED"
                };

                // 3. Xử lý cập nhật Subscription
                bool result = await _paymentService.HandlePaymentWebhookAsync(paymentData);

                if (result)
                {
                    return Ok(new { success = true, message = "Webhook processed" });
                }

                return BadRequest(new { success = false, message = "Failed to process" });
            }
            catch (PayOSException ex)
            {
                // Log PayOS specific errors
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    errorCode = ex.GetType().Name
                });
            }
            catch (Exception ex)
            {
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
                var result = await _payOSClient.Webhooks.ConfirmAsync(webhookUrl);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


    }
}
