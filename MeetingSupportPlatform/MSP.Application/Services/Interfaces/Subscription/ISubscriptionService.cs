using MSP.Application.Models.Requests.Subscription;
using MSP.Application.Models.Responses.Subscription;
using MSP.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Services.Interfaces.Subscription
{
    public interface ISubscriptionService
    {
        Task<ApiResponse<GetSubscriptionResponse>> CreateSubscriptionAsync(CreateSubscriptionRequest request);
        Task<ApiResponse<IEnumerable<GetSubscriptionDetailResponse>>> GetSubscriptionsByUserIdAsync(Guid userId);
        Task<ApiResponse<IEnumerable<GetSubscriptionDetailResponse>>> GetAllSubscriptionsAsync();
        Task<ApiResponse<GetSubscriptionDetailResponse>> GetActiveSubscriptionByUserIdAsync(Guid userId);
        Task<ApiResponse<GetSubscriptionUsageResponse>> GetActiveSubscriptionWithUsageAsync(Guid userId);
    }
}
