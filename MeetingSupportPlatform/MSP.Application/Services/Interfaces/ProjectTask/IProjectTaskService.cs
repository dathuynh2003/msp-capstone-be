using MSP.Application.Models.Requests.ProjectTask;
using MSP.Application.Models.Responses.ProjectTask;
using MSP.Shared.Common;

namespace MSP.Application.Services.Interfaces.ProjectTask
{
    public interface IProjectTaskService
    {
        Task<ApiResponse<GetTaskResponse>> CreateTaskAsync(CreateTaskRequest request);
        Task<ApiResponse<GetTaskResponse>> UpdateTaskAsync(UpdateTaskRequest request);
        Task<ApiResponse<string>> DeleteTaskAsync(Guid taskId);
        Task<ApiResponse<GetTaskResponse>> GetTaskByIdAsync(Guid taskId);
        Task<ApiResponse<PagingResponse<GetTaskResponse>>> GetTasksByProjectIdAsync(PagingRequest request, Guid projectId);
        Task<ApiResponse<PagingResponse<GetTaskResponse>>> GetTasksByUserIdAndProjectIdAsync(PagingRequest request, Guid userId, Guid projectId);
        Task<ApiResponse<List<GetTaskResponse>>> GetTasksByMilestoneIdAsync(Guid milestoneId);
    }
}
