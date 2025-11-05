using MSP.Application.Models.Requests.Limitation;
using MSP.Application.Models.Requests.Meeting;
using MSP.Application.Models.Responses.Limitation;
using MSP.Application.Models.Responses.Meeting;
using MSP.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Services.Interfaces.Limitation
{
    public interface ILimitationService
    {
        Task<ApiResponse<GetLimitationResponse>> CreateLimitationAsync(CreateLimitationRequest request);
        Task<ApiResponse<GetLimitationResponse>> UpdateLimitationAsync(UpdateLimitationRequest request);
        Task<ApiResponse<string>> DeleteLimitationAsync(Guid limitationId);
        Task<ApiResponse<GetLimitationResponse>> GetLimitationByIdAsync(Guid limitationId);
        Task<ApiResponse<List<GetLimitationResponse>>> GetLimitationsAsync();
    }
}
