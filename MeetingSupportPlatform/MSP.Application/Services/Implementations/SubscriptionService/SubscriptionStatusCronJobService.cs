using Microsoft.Extensions.Logging;
using MSP.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Services.Implementations.SubscriptionService
{
    public class SubscriptionStatusCronJobService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ILogger<SubscriptionStatusCronJobService> _logger;
        public SubscriptionStatusCronJobService(
            ISubscriptionRepository subscriptionRepository,
            ILogger<SubscriptionStatusCronJobService> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _logger = logger;
        }

        public async Task ExpireSubscriptionsAsync()
        {
            try
            {
                _logger.LogInformation("Starting to expire subscriptions at {Time}", DateTime.UtcNow);
                var expiredSubscriptions = await _subscriptionRepository.GetExpiredTodayAsync();
                if (expiredSubscriptions.Any())
                {
                    _logger.LogInformation("Found {Count} subscriptions to expire", expiredSubscriptions.Count());
                    foreach (var subscription in expiredSubscriptions)
                    {
                        subscription.IsActive = false;
                        subscription.UpdatedAt = DateTime.UtcNow;
                        await _subscriptionRepository.UpdateAsync(subscription);
                        _logger.LogInformation(
                            "Expired subscription {SubscriptionId} for User {UserId}. ExpirationDate was {ExpirationDate}",
                            subscription.Id,
                            subscription.UserId,
                            subscription.EndDate);
                    }
                    await _subscriptionRepository.SaveChangesAsync();
                }

                else
                {
                    _logger.LogInformation("No subscriptions to expire today.");
                }
                _logger.LogInformation("Finished expiring subscriptions at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while expiring subscriptions at {Time}", DateTime.UtcNow);
            }
        }
    }
}
