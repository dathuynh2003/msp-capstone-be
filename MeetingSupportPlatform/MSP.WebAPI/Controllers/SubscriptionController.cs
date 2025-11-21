using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MSP.Application.Models.Requests.Subscription;
using MSP.Application.Services.Interfaces.Subscription;

namespace MSP.WebAPI.Controllers
{
    [Route("api/v1/subscriptions")]
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
        [HttpPost]
        public async Task<IActionResult> PurchasePackage([FromBody] CreateSubscriptionRequest request)
        {
            var response = await _subscriptionService.CreateSubscriptionAsync(request);
            return Ok(response);
        }
        [HttpGet("active/{userId}")]
        public async Task<IActionResult> GetActiveSubscription(Guid userId)
        {
            var response = await _subscriptionService.GetActiveSubscriptionByUserIdAsync(userId);

            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }
        [HttpGet("active/{userId}/usage")]
        public async Task<IActionResult> GetActiveSubscriptionWithUsage(Guid userId)
        {
            var response = await _subscriptionService.GetActiveSubscriptionWithUsageAsync(userId);

            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetSubscriptionsByUser(Guid userId)
        {
            var response = await _subscriptionService.GetSubscriptionsByUserIdAsync(userId);

            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSubscriptions()
        {
            var response = await _subscriptionService.GetAllSubscriptionsAsync();

            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }
    }
}
