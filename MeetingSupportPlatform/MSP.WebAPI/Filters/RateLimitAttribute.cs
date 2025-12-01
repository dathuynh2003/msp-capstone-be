using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace MSP.WebAPI.Filters
{
    /// <summary>
    /// Rate limiting attribute for API endpoints
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RateLimitAttribute : ActionFilterAttribute
    {
        private readonly int _maxRequests;
        private readonly int _timeWindowSeconds;

        public RateLimitAttribute(int maxRequests = 5, int timeWindowSeconds = 60)
        {
            _maxRequests = maxRequests;
            _timeWindowSeconds = timeWindowSeconds;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var cache = context.HttpContext.RequestServices.GetService(typeof(IMemoryCache)) as IMemoryCache;
            
            if (cache == null)
            {
                base.OnActionExecuting(context);
                return;
            }

            // Use IP address + endpoint as key
            var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            var endpoint = context.HttpContext.Request.Path.ToString();
            var cacheKey = $"RateLimit_{ipAddress}_{endpoint}";

            if (!cache.TryGetValue(cacheKey, out int requestCount))
            {
                requestCount = 0;
            }

            if (requestCount >= _maxRequests)
            {
                context.Result = new ContentResult
                {
                    Content = "Too many requests. Please try again later.",
                    StatusCode = (int)HttpStatusCode.TooManyRequests
                };
                return;
            }

            requestCount++;
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(_timeWindowSeconds));
            
            cache.Set(cacheKey, requestCount, cacheOptions);
            
            base.OnActionExecuting(context);
        }
    }
}
