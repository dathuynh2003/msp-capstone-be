using MSP.Application.Models.Responses.Auth;
using MSP.Application.Models.Responses.TaskHistory;
using MSP.Shared.Common;

namespace MSP.Application.Services.Interfaces.TaskHistory
{
    public interface ITaskHistoryService
    {
        Task<ApiResponse<IEnumerable<GetUserResponse>>> GetAvailableUsersForReassignmentAsync(Guid taskId, Guid fromUserId);
        Task<ApiResponse<IEnumerable<GetTaskHistoryResponse>>> GetTaskHistoriesByTaskIdAsync(Guid taskId);

        // Internal methods - dùng bởi ProjectTaskService để tự động tạo history
        Task<MSP.Domain.Entities.TaskHistory> TrackTaskCreationAsync(
            Guid taskId,
            Guid createdByUserId,
            Guid? assignedToUserId);

        Task<MSP.Domain.Entities.TaskHistory> TrackTaskAssignmentAsync(
            Guid taskId,
            Guid? fromUserId,
            Guid toUserId,
            Guid changedByUserId);

        Task<MSP.Domain.Entities.TaskHistory> TrackFieldChangeAsync(
            Guid taskId,
            string fieldName,
            string? oldValue,
            string? newValue,
            Guid changedByUserId);

        Task<MSP.Domain.Entities.TaskHistory> TrackStatusChangeAsync(
            Guid taskId,
            string oldStatus,
            string newStatus,
            Guid changedByUserId);
    }
}
