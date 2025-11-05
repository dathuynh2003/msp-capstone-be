using MSP.Application.Models.Requests.TaskReassignRequest;
using MSP.Application.Models.Responses.Auth;
using MSP.Application.Models.Responses.TaskReassignRequest;
using MSP.Domain.Entities;
using MSP.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Services.Interfaces.TaskReassignRequest
{
    public interface ITaskReassignRequestService
    {
        Task<ApiResponse<IEnumerable<GetUserResponse>>> GetAvailableUsersForReassignmentAsync(Guid taskId, Guid fromUserId);
        Task<ApiResponse<IEnumerable<GetTaskReassignRequestResponse>>> GetTaskReassignRequestsForUserAsync(Guid userId);
        Task<ApiResponse<IEnumerable<GetTaskReassignRequestResponse>>> GetTaskReassignRequestsByTaskIdAsync(Guid taskId);
        Task<ApiResponse<GetTaskReassignRequestResponse>> CreateTaskReassignRequest(CreateTaskReassignRequestRequest request);
        Task<ApiResponse<GetTaskReassignRequestResponse>> AcceptTaskReassignRequest(Guid taskReassignRequestId, UpdateTaskReassignRequestRequest request);
        Task<ApiResponse<GetTaskReassignRequestResponse>> RejectTaskReassignRequest(Guid taskReassignRequestId, UpdateTaskReassignRequestRequest request);
    }
}
