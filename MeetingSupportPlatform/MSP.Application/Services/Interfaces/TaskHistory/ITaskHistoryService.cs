using MSP.Application.Models.Requests.TaskReassignRequest;
using MSP.Application.Models.Responses.Auth;
using MSP.Application.Models.Responses.TaskHistory;
using MSP.Domain.Entities;
using MSP.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Services.Interfaces.TaskHistory
{
    public interface ITaskHistoryService
    {
        Task<ApiResponse<IEnumerable<GetUserResponse>>> GetAvailableUsersForReassignmentAsync(Guid taskId, Guid fromUserId);
        Task<ApiResponse<IEnumerable<GetTaskHistoryResponse>>> GetTaskHistoriesByTaskIdAsync(Guid taskId);
        Task<ApiResponse<GetTaskHistoryResponse>> CreateTaskHistoryAsync(CreateTaskHistoryRequest request);
    }
}
