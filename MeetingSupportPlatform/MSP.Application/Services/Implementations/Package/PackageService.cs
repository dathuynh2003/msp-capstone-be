using MSP.Application.Models.Requests.Package;
using MSP.Application.Models.Responses.Limitation;
using MSP.Application.Models.Responses.Package;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Package;
using MSP.Domain.Entities;
using MSP.Shared.Common;
using MSP.Shared.Enums;
using System.Linq;

namespace MSP.Application.Services.Implementations.Package
{
    public class PackageService : IPackageService
    {
        private readonly IPackageRepository _packageRepository;
        private readonly ILimitationRepository _limitationRepository;

        public PackageService(
            IPackageRepository packageRepository,
            ILimitationRepository limitationRepository)
        {
            _packageRepository = packageRepository;
            _limitationRepository = limitationRepository;
        }

        public async Task<ApiResponse<List<GetPackageResponse>>> GetAllAsync()
        {
            try
            {
                var packages = await _packageRepository.GetAll();
                var orderMap = new Dictionary<LimitationTypeEnum, int>
                {
                    { LimitationTypeEnum.NumberMemberInOrganization, 1 },
                    { LimitationTypeEnum.NumberProject, 2 },
                    { LimitationTypeEnum.NumberMemberInProject, 3 },
                    { LimitationTypeEnum.NumberMeeting, 4 },
                    { LimitationTypeEnum.NumberMemberInMeeting, 5 }
                };

                var response = packages.Select(p => new GetPackageResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Currency = p.Currency,
                    BillingCycle = p.BillingCycle,
                    isDeleted = p.IsDeleted,
                    Limitations = p.Limitations
                         .OrderBy(l =>
                         {
                             Enum.TryParse<LimitationTypeEnum>(l.LimitationType, out var enumValue);
                             return orderMap[enumValue];
                         })
                        .Select(l => new GetLimitationResponse
                        {
                            Id = l.Id,
                            Name = l.Name,
                            Description = l.Description,
                            IsUnlimited = l.IsUnlimited,
                            LimitValue = l.LimitValue,
                            LimitUnit = l.LimitUnit,
                            IsDeleted = l.IsDeleted
                        })
                        .ToList()
                }).ToList();

                return ApiResponse<List<GetPackageResponse>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<GetPackageResponse>>.ErrorResponse(null, ex.Message);
            }
        }

        public async Task<ApiResponse<GetPackageResponse>> GetByIdAsync(Guid id)
        {
            var package = await _packageRepository.GetPackageByIdAsync(id);

            if (package == null || package.IsDeleted)
                return ApiResponse<GetPackageResponse>.ErrorResponse(null, "Package not found");

            var orderMap = new Dictionary<LimitationTypeEnum, int>
            {
                { LimitationTypeEnum.NumberMemberInOrganization, 1 },
                { LimitationTypeEnum.NumberProject, 2 },
                { LimitationTypeEnum.NumberMemberInProject, 3 },
                { LimitationTypeEnum.NumberMeeting, 4 },
                { LimitationTypeEnum.NumberMemberInMeeting, 5 }
            };
            var response = new GetPackageResponse
            {
                Id = package.Id,
                Name = package.Name,
                Description = package.Description,
                Price = package.Price,
                Currency = package.Currency,
                BillingCycle = package.BillingCycle,
                isDeleted = package.IsDeleted,
                Limitations = package.Limitations
                        .OrderBy(l =>
                        {
                            // Parse string → enum
                            Enum.TryParse<LimitationTypeEnum>(l.LimitationType, out var enumValue);
                            return orderMap[enumValue];
                        })
                        .Select(l => new GetLimitationResponse
                        {
                            Id = l.Id,
                            Name = l.Name,
                            Description = l.Description,
                            IsUnlimited = l.IsUnlimited,
                            LimitValue = l.LimitValue,
                            LimitUnit = l.LimitUnit,
                            IsDeleted = l.IsDeleted
                        })
                        .ToList()
            };

            return ApiResponse<GetPackageResponse>.SuccessResponse(response);
        }

        public async Task<ApiResponse<GetPackageResponse>> CreateAsync(CreatePackageRequest request)
        {
            var packageEntity = new MSP.Domain.Entities.Package
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Currency = request.Currency,
                BillingCycle = request.BillingCycle,
                CreatedById = request.CreatedById,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add limitations (many-to-many)
            if (request.LimitationIds != null && request.LimitationIds.Any())
            {
                var limitations = await _limitationRepository.GetByIdsAsync(request.LimitationIds);
                packageEntity.Limitations = limitations.ToList();
            }

            await _packageRepository.AddAsync(packageEntity);
            await _packageRepository.SaveChangesAsync();

            var response = new GetPackageResponse
            {
                Id = packageEntity.Id,
                Name = packageEntity.Name,
                Description = packageEntity.Description,
                Price = packageEntity.Price,
                Currency = packageEntity.Currency,
                BillingCycle = packageEntity.BillingCycle,
                Limitations = packageEntity.Limitations.Select(l => new GetLimitationResponse
                {
                    Id = l.Id,
                    Name = l.Name,
                    Description = l.Description,
                    IsUnlimited = l.IsUnlimited,
                    LimitValue = l.LimitValue,
                    LimitUnit = l.LimitUnit,
                    IsDeleted = l.IsDeleted
                }).ToList()
            };

            return ApiResponse<GetPackageResponse>.SuccessResponse(response, "Package created successfully");
        }

        public async Task<ApiResponse<GetPackageResponse>> UpdateAsync(Guid id, UpdatePackageRequest request)
        {
            var packageEntity = await _packageRepository.GetPackageByIdAsync(id);

            if (packageEntity == null || packageEntity.IsDeleted)
                return ApiResponse<GetPackageResponse>.ErrorResponse(null, "Package not found");

            packageEntity.Name = request.Name;
            packageEntity.Description = request.Description;
            packageEntity.Price = request.Price;
            packageEntity.Currency = request.Currency;
            packageEntity.BillingCycle = request.BillingCycle;
            packageEntity.CreatedById = request.CreatedById;
            packageEntity.UpdatedAt = DateTime.UtcNow;

            // update many-to-many
            packageEntity.Limitations.Clear();

            if (request.LimitationIds.Any())
            {
                var limitations = await _limitationRepository.GetByIdsAsync(request.LimitationIds);
                packageEntity.Limitations = limitations.ToList();
            }

            await _packageRepository.UpdateAsync(packageEntity);
            await _packageRepository.SaveChangesAsync();

            var response = new GetPackageResponse
            {
                Id = packageEntity.Id,
                Name = packageEntity.Name,
                Description = packageEntity.Description,
                Price = packageEntity.Price,
                Currency = packageEntity.Currency,
                BillingCycle = packageEntity.BillingCycle,
                Limitations = packageEntity.Limitations.Select(l => new GetLimitationResponse
                {
                    Id = l.Id,
                    Name = l.Name,
                    Description = l.Description,
                    IsUnlimited = l.IsUnlimited,
                    LimitValue = l.LimitValue,
                    LimitUnit = l.LimitUnit,
                    IsDeleted = l.IsDeleted
                }).ToList()
            };

            return ApiResponse<GetPackageResponse>.SuccessResponse(response, "Package updated successfully");
        }

        public async Task<ApiResponse<string>> DeleteAsync(Guid id)
        {
            var packageEntity = await _packageRepository.GetByIdAsync(id);
            if (packageEntity == null || packageEntity.IsDeleted)
                return ApiResponse<string>.ErrorResponse(null, "Package not found");

            await _packageRepository.SoftDeleteAsync(packageEntity);
            await _packageRepository.SaveChangesAsync();

            return ApiResponse<string>.SuccessResponse(null, "Package deleted successfully");
        }
    }
}
