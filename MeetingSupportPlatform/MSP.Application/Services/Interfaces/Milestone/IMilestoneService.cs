using MSP.Application.Models.Requests.Milestone;
using MSP.Application.Models.Responses.Milestone;
using MSP.Shared.Common;

namespace MSP.Application.Services.Interfaces.Milestone
{
    public interface IMilestoneService
    {
        Task<ApiResponse<GetMilestoneResponse>> CreateMilestoneAsync(CreateMilestoneRequest request);
        Task<ApiResponse<GetMilestoneResponse>> UpdateMilestoneAsync(UpdateMilestoneRequest request);
        Task<ApiResponse<string>> DeleteMilestoneAsync(Guid milestoneId);
        Task<ApiResponse<GetMilestoneResponse>> GetMilestoneByIdAsync(Guid milestoneId);
        Task<ApiResponse<List<GetMilestoneResponse>>> GetMilestonesByProjectIdAsync(Guid projectId);
    }
}
