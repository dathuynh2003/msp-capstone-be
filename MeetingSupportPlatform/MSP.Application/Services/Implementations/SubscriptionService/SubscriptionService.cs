using MSP.Application.Models.Requests.Payment;
using MSP.Application.Models.Requests.Subscription;
using MSP.Application.Models.Responses.Limitation;
using MSP.Application.Models.Responses.Package;
using MSP.Application.Models.Responses.Subscription;
using MSP.Application.Models.Responses.Users;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Payment;
using MSP.Application.Services.Interfaces.Subscription;
using MSP.Domain.Entities;
using MSP.Shared.Common;
using MSP.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Services.Implementations.SubscriptionService
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IPaymentService _paymentService;
        private readonly IPackageRepository _packageRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        public SubscriptionService(IPaymentService paymentService, IPackageRepository packageRepository, ISubscriptionRepository subscriptionRepository)
        {
            _paymentService = paymentService;
            _packageRepository = packageRepository;
            _subscriptionRepository = subscriptionRepository;
        }
        public async Task<ApiResponse<GetSubscriptionResponse>> CreateSubscriptionAsync(CreateSubscriptionRequest request)
        {
            var package = await _packageRepository.GetByIdAsync(request.PackageId);
            if (package == null)
            {
                return ApiResponse<GetSubscriptionResponse>.ErrorResponse(null, "Package not found");
            }
            var subscription = new Subscription
            {
                UserId = request.UserId,
                PackageId = request.PackageId,
                TotalPrice = package.Price,
                TransactionID = string.Empty,
                PaymentMethod = string.Empty,
            };
            var paymentRequest = new CreatePaymentLinkRequest
            {
                Amount = (int)package.Price,
                Description = $"Subscription package",
                ReturnUrl = request.ReturnUrl,
                CancelUrl = request.CancelUrl
            };

            // 3. Tạo Payment Link với PayOS
            var paymentLink = await _paymentService.CreatePaymentLinkAsync(paymentRequest);

            // 4. Cập nhật TransactionID vào Subscription
            subscription.IsActive = false;
            subscription.CreatedAt = DateTime.UtcNow;
            subscription.TransactionID = paymentLink.OrderCode.ToString();
            subscription.Status = paymentLink.Status;
            await _subscriptionRepository.AddAsync(subscription);
            await _subscriptionRepository.SaveChangesAsync();
            // 5. Trả về response
            var rs = new GetSubscriptionResponse
            {
                SubscriptionId = subscription.Id,
                PaymentUrl = paymentLink.CheckoutUrl,
                QrCodeUrl = paymentLink.QrCode,
                OrderCode = (int)paymentLink.OrderCode,
                TotalAmount = package.Price,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };
            return ApiResponse<GetSubscriptionResponse>.SuccessResponse(rs, "Create subscription successfully");
        }

        public async Task<ApiResponse<GetSubscriptionDetailResponse>> GetActiveSubscriptionByUserIdAsync(Guid userId)
        {
            var subscription = await _subscriptionRepository.GetActiveSubscriptionByUserIdAsync(userId);
            if (subscription == null)
            {
                return ApiResponse<GetSubscriptionDetailResponse>.ErrorResponse(null, "No active subscription found for the user");
            }
            var response = new GetSubscriptionDetailResponse
            {
                Id = subscription.Id,
                UserId = subscription.UserId,
                PackageId = subscription.PackageId,
                TotalPrice = subscription.TotalPrice,
                IsActive = subscription.IsActive,
                Status = subscription.Status,
                PaymentMethod = subscription.PaymentMethod,
                TransactionID = subscription.TransactionID,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                PaidAt = subscription.PaidAt,
                User = new GetUserResponse
                {
                    Id = subscription.User.Id,
                    FullName = subscription.User.FullName,
                    Email = subscription.User.Email,
                    CreatedAt = subscription.User.CreatedAt,
                },
                Package = new GetPackageResponse
                {
                    Id = subscription.Package.Id,
                    Name = subscription.Package.Name,
                    Description = subscription.Package.Description,
                    Price = subscription.Package.Price,
                    BillingCycle = subscription.Package.BillingCycle,
                    Currency = subscription.Package.Currency,
                    Limitations = subscription.Package.Limitations.Select(l => new GetLimitationResponse
                    {
                        Id = l.Id,
                        Name = l.Name,
                        Description = l.Description,
                        IsUnlimited = l.IsUnlimited,
                        LimitValue = l.LimitValue,
                        LimitUnit = l.LimitUnit,
                        IsDeleted = l.IsDeleted
                    }).ToList()
                },
            };
            return ApiResponse<GetSubscriptionDetailResponse>.SuccessResponse(response, "Get active subscription successfully");
        }

        public async Task<ApiResponse<IEnumerable<GetSubscriptionDetailResponse>>> GetSubscriptionsByUserIdAsync(Guid userId)
        {
            var subscriptions = await _subscriptionRepository.GetByUserIdAsync(userId);
            var response = subscriptions.Select(subscription => new GetSubscriptionDetailResponse
            {
                Id = subscription.Id,
                UserId = subscription.UserId,
                PackageId = subscription.PackageId,
                TotalPrice = subscription.TotalPrice,
                IsActive = subscription.IsActive,
                Status = subscription.Status,
                PaymentMethod = subscription.PaymentMethod,
                TransactionID = subscription.TransactionID,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                PaidAt = subscription.PaidAt,
                User = new GetUserResponse
                {
                    Id = subscription.User.Id,
                    FullName = subscription.User.FullName,
                    Email = subscription.User.Email,
                    CreatedAt = subscription.User.CreatedAt,
                },
                Package = new GetPackageResponse
                {
                    Id = subscription.Package.Id,
                    Name = subscription.Package.Name,
                    Description = subscription.Package.Description,
                    Price = subscription.Package.Price,
                    BillingCycle = subscription.Package.BillingCycle,
                    Currency = subscription.Package.Currency,
                    Limitations = subscription.Package.Limitations.Select(l => new GetLimitationResponse
                    {
                        Id = l.Id,
                        Name = l.Name,
                        Description = l.Description,
                        IsUnlimited = l.IsUnlimited,
                        LimitValue = l.LimitValue,
                        LimitUnit = l.LimitUnit,
                        IsDeleted = l.IsDeleted
                    }).ToList()
                },
            });
            return ApiResponse<IEnumerable<GetSubscriptionDetailResponse>>.SuccessResponse(response, "Get subscriptions successfully");
        }
    }
}
