using MSP.Application.Models.Responses.Limitation;
using MSP.Application.Models.Responses.Package;
using MSP.Domain.Entities;
using System.Text.Json;

namespace MSP.Application.Helpers
{
    public static class SubscriptionSnapshotHelper
    {
        public static void CapturePackageSnapshot(Subscription subscription, Package package)
        {
            if (package == null)
                return;

            var packageSnapshot = new
            {
                package.Id,
                package.Name,
                package.Description,
                package.Price,
                package.BillingCycle,
                package.Currency
            };

            subscription.SnapshotPackageJson = JsonSerializer.Serialize(packageSnapshot);

            var limitationsSnapshot = package.Limitations
                .Where(l => !l.IsDeleted)
                .Select(l => new
                {
                    l.Id,
                    l.Name,
                    l.Description,
                    l.IsUnlimited,
                    l.LimitValue,
                    l.LimitUnit,
                    l.LimitationType
                })
                .ToList();

            subscription.SnapshotLimitationsJson = JsonSerializer.Serialize(limitationsSnapshot);
        }

        public static GetPackageResponse DeserializePackageSnapshot(string snapshotJson)
        {
            if (string.IsNullOrEmpty(snapshotJson))
                return null;

            try
            {
                var packageData = JsonSerializer.Deserialize<JsonElement>(snapshotJson);
                return new GetPackageResponse
                {
                    Id = packageData.GetProperty("Id").GetGuid(),
                    Name = packageData.GetProperty("Name").GetString(),
                    Description = packageData.GetProperty("Description").GetString(),
                    Price = packageData.GetProperty("Price").GetDecimal(),
                    BillingCycle = packageData.GetProperty("BillingCycle").GetInt32(),
                    Currency = packageData.GetProperty("Currency").GetString(),
                    Limitations = new List<GetLimitationResponse>()
                };
            }
            catch
            {
                return null;
            }
        }

        public static List<GetLimitationResponse> DeserializeLimitationsSnapshot(string snapshotJson)
        {
            if (string.IsNullOrEmpty(snapshotJson))
                return new List<GetLimitationResponse>();

            try
            {
                var limitationsData = JsonSerializer.Deserialize<List<JsonElement>>(snapshotJson);
                return limitationsData?.Select(l => new GetLimitationResponse
                {
                    Id = l.GetProperty("Id").GetGuid(),
                    Name = l.GetProperty("Name").GetString(),
                    Description = l.GetProperty("Description").GetString(),
                    IsUnlimited = l.GetProperty("IsUnlimited").GetBoolean(),
                    LimitValue = l.GetProperty("LimitValue").ValueKind == JsonValueKind.Null 
                        ? null 
                        : l.GetProperty("LimitValue").GetInt32(),
                    LimitUnit = l.GetProperty("LimitUnit").GetString(),
                    IsDeleted = false
                }).ToList() ?? new List<GetLimitationResponse>();
            }
            catch
            {
                return new List<GetLimitationResponse>();
            }
        }
    }
}
