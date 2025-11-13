using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MSP.Application.Models.Requests.Subscription;
using MSP.Application.Services.Interfaces.Subscription;

namespace MSP.WebAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }
        /// <summary>
        /// API mua package
        /// </summary>
        [HttpPost("purchase")]
        public async Task<IActionResult> PurchasePackage([FromBody] CreateSubscriptionRequest request)
        {
            try
            {
                var response = await _subscriptionService.CreateSubscriptionAsync(request);

                return Ok(new
                {
                    success = true,
                    data = response,
                    message = "Vui lòng quét QR hoặc truy cập link để thanh toán"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
