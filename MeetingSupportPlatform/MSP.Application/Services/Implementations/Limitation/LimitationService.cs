﻿﻿using System;
﻿using MSP.Application.Models.Requests.Limitation;
using MSP.Application.Models.Responses.Limitation;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Limitation;
using MSP.Shared.Common;


namespace MSP.Application.Services.Implementations.Limitation
{
    public class LimitationService : ILimitationService
    {
        private readonly ILimitationRepository _limitationRepository;

        public LimitationService(ILimitationRepository limitationRepository)
        {
            _limitationRepository = limitationRepository;
        }

        public async Task<ApiResponse<GetLimitationResponse>> CreateLimitationAsync(CreateLimitationRequest request)
        {
            var limitation = new Domain.Entities.Limitation
            {
                Name = request.Name,
                Description = request.Description,
                IsUnlimited = request.IsUnlimited,
                LimitValue = request.LimitValue,
                LimitUnit = request.LimitUnit,
                LimitationType = request.LimitationType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _limitationRepository.AddAsync(limitation);

            var response = new GetLimitationResponse
            {
                Id = limitation.Id,
                Name = limitation.Name,
                Description = limitation.Description,
                IsUnlimited = limitation.IsUnlimited,
                LimitValue = limitation.LimitValue,
                LimitUnit = limitation.LimitUnit
            };

            await _limitationRepository.SaveChangesAsync();

            return ApiResponse<GetLimitationResponse>.SuccessResponse(response, "Limitation created successfully");
        }

        public async Task<ApiResponse<GetLimitationResponse>> UpdateLimitationAsync(UpdateLimitationRequest request)
        {
            var limitation = await _limitationRepository.GetByIdAsync(request.LimitationId);
            if (limitation == null || limitation.IsDeleted)
            {
                return ApiResponse<GetLimitationResponse>.ErrorResponse(null, "Limitation not found");
            }

            limitation.Name = request.Name;
            limitation.Description = request.Description;
            limitation.IsUnlimited = request.IsUnlimited;
            limitation.LimitValue = request.LimitValue;
            limitation.LimitUnit = request.LimitUnit;
            limitation.LimitationType = request.LimitationType;
            limitation.UpdatedAt = DateTime.UtcNow;

            await _limitationRepository.UpdateAsync(limitation);
            await _limitationRepository.SaveChangesAsync();

            var response = new GetLimitationResponse
            {
                Id = limitation.Id,
                Name = limitation.Name,
                Description = limitation.Description,
                IsUnlimited = limitation.IsUnlimited,
                LimitValue = limitation.LimitValue,
                LimitUnit = limitation.LimitUnit
            };

            return ApiResponse<GetLimitationResponse>.SuccessResponse(response, "Limitation updated successfully");
        }

        public async Task<ApiResponse<string>> DeleteLimitationAsync(Guid limitationId)
        {
            var limitation = await _limitationRepository.GetByIdAsync(limitationId);
            if (limitation == null || limitation.IsDeleted)
            {
                return ApiResponse<string>.ErrorResponse(null, "Limitation not found");
            }

            await _limitationRepository.SoftDeleteAsync(limitation);
            await _limitationRepository.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse(null, "Limitation deleted successfully");
        }

        public async Task<ApiResponse<GetLimitationResponse>> GetLimitationByIdAsync(Guid limitationId)
        {
            var limitation = await _limitationRepository.GetLimitationByIdAsync(limitationId);
            if (limitation == null || limitation.IsDeleted)
            {
                return ApiResponse<GetLimitationResponse>.ErrorResponse(null, "Limitation not found");
            }

            var response = new GetLimitationResponse
            {
                Id = limitation.Id,
                Name = limitation.Name,
                Description = limitation.Description,
                IsUnlimited = limitation.IsUnlimited,
                LimitValue = limitation.LimitValue,
                LimitUnit = limitation.LimitUnit,
                LimitationType = limitation.LimitationType,
                IsDeleted = limitation.IsDeleted,
            };

            return ApiResponse<GetLimitationResponse>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<GetLimitationResponse>>> GetLimitationsAsync()
        {
            try
            {
                var limitations = await _limitationRepository.GetAll();
                var response = limitations.Select(l => new GetLimitationResponse
                {
                    Id = l.Id,
                    Name = l.Name,
                    Description = l.Description,
                    IsUnlimited = l.IsUnlimited,
                    LimitValue = l.LimitValue,
                    LimitUnit = l.LimitUnit,
                    LimitationType = l.LimitationType,
                    IsDeleted = l.IsDeleted,
                }).ToList();

                return ApiResponse<List<GetLimitationResponse>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<GetLimitationResponse>>.ErrorResponse(null, ex.Message);
            }
        }
    }
}
