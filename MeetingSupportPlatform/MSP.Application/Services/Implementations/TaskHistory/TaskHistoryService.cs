using Microsoft.AspNetCore.Identity;
using MSP.Application.Models.Responses.Auth;
using MSP.Application.Models.Responses.ProjectTask;
using MSP.Application.Models.Responses.TaskHistory;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.TaskHistory;
using MSP.Domain.Entities;
using MSP.Shared.Common;
using MSP.Shared.Enums;

namespace MSP.Application.Services.Implementations.TaskHistory
{
    public class TaskHistoryService : ITaskHistoryService
    {
        private readonly ITaskHistoryRepository _taskHistoryRepo;
        private readonly IProjectTaskRepository _taskRepo;
        private readonly IProjectMemberRepository _memberRepo;
        private readonly UserManager<User> _userManager;

        public TaskHistoryService(ITaskHistoryRepository taskHistoryRepo,
            IProjectTaskRepository taskRepo,
            IProjectMemberRepository memberRepo,
            UserManager<User> userManager)
        {
            _taskHistoryRepo = taskHistoryRepo;
            _taskRepo = taskRepo;
            _memberRepo = memberRepo;
            _userManager = userManager;
        }

        /// <summary>
        /// Track when creating a new task
        /// </summary>
        public async Task<MSP.Domain.Entities.TaskHistory> TrackTaskCreationAsync(
            Guid taskId,
            Guid createdByUserId,
            Guid? assignedToUserId)
        {
            var history = new MSP.Domain.Entities.TaskHistory
            {
                TaskId = taskId,
                Action = TaskHistoryAction.Created.ToString(),
                ChangedById = createdByUserId,
                ToUserId = assignedToUserId,
                CreatedAt = DateTime.UtcNow
            };

            await _taskHistoryRepo.AddAsync(history);
            return history;
        }

        /// <summary>
        /// Track when assigning/reassigning task
        /// </summary>
        public async Task<MSP.Domain.Entities.TaskHistory> TrackTaskAssignmentAsync(
            Guid taskId,
            Guid? fromUserId,
            Guid toUserId,
            Guid changedByUserId)
        {
            var action = fromUserId == null
                ? TaskHistoryAction.Assigned
                : TaskHistoryAction.Reassigned;

            var history = new MSP.Domain.Entities.TaskHistory
            {
                TaskId = taskId,
                Action = action.ToString(),
                FromUserId = fromUserId,
                ToUserId = toUserId,
                ChangedById = changedByUserId,
                CreatedAt = DateTime.UtcNow
            };

            await _taskHistoryRepo.AddAsync(history);
            return history;
        }

        /// <summary>
        /// Track when change fields (Title, Description, StartDate, EndDate...)
        /// </summary>
        public async Task<MSP.Domain.Entities.TaskHistory> TrackFieldChangeAsync(
            Guid taskId,
            string fieldName,
            string? oldValue,
            string? newValue,
            Guid changedByUserId)
        {
            var history = new MSP.Domain.Entities.TaskHistory
            {
                TaskId = taskId,
                Action = TaskHistoryAction.Updated.ToString(),
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValue,
                ChangedById = changedByUserId,
                CreatedAt = DateTime.UtcNow
            };

            await _taskHistoryRepo.AddAsync(history);
            return history;
        }

        /// <summary>
        /// Track when change status
        /// </summary>
        public async Task<MSP.Domain.Entities.TaskHistory> TrackStatusChangeAsync(
            Guid taskId,
            string oldStatus,
            string newStatus,
            Guid changedByUserId)
        {
            var history = new MSP.Domain.Entities.TaskHistory
            {
                TaskId = taskId,
                Action = TaskHistoryAction.StatusChanged.ToString(),
                FieldName = "Status",
                OldValue = oldStatus,
                NewValue = newStatus,
                ChangedById = changedByUserId,
                CreatedAt = DateTime.UtcNow
            };

            await _taskHistoryRepo.AddAsync(history);
            return history;
        }

        public async Task<ApiResponse<IEnumerable<GetUserResponse>>> GetAvailableUsersForReassignmentAsync(Guid taskId, Guid fromUserId)
        {
            // Lấy task hiện tại
            var task = await _taskRepo.GetByIdAsync(taskId);
            if (task == null)
                throw new Exception("Task not found");

            var projectId = task.ProjectId;
            var startDate = task.StartDate ?? DateTime.UtcNow;
            var endDate = task.EndDate ?? DateTime.UtcNow.AddDays(1);

            // Lấy danh sách thành viên đang active trong project (trừ người hiện tại)
            var projectMembers = await _memberRepo.GetProjectMembersByProjectIdAsync(projectId);
            var potentialMembers = projectMembers
                .Where(m => m.MemberId != fromUserId)
                .ToList();

            // Lọc user có role là "Member"
            var potentialUsers = new List<User>();

            foreach (var member in potentialMembers)
            {
                var user = member.Member;
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains(UserRoleEnum.Member.ToString()))
                {
                    potentialUsers.Add(user);
                }
            }

            // Kiểm tra trùng lịch và chọn user khả dụng
            var availableUsers = new List<GetUserResponse>();

            foreach (var user in potentialUsers)
            {
                bool hasOverlap = await _taskRepo.HasTaskOverlapAsync(
                    user.Id, projectId, task.Id, startDate, endDate);

                if (!hasOverlap)
                {
                    availableUsers.Add(new GetUserResponse
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        FullName = user.FullName,
                        Email = user.Email,
                        AvatarUrl = user.AvatarUrl,
                        PhoneNumber = user.PhoneNumber
                    });
                }
            }

            return ApiResponse<IEnumerable<GetUserResponse>>.SuccessResponse(availableUsers, "Fetch list available user successfully");
        }

        public async Task<ApiResponse<IEnumerable<GetTaskHistoryResponse>>> GetTaskHistoriesByTaskIdAsync(Guid taskId)
        {
            var requests = await _taskHistoryRepo.GetTaskHistoriesByTaskIdAsync(taskId);
            var rs = requests.Select(trr => new GetTaskHistoryResponse
            {
                Id = trr.Id,
                TaskId = trr.TaskId,
                FromUserId = trr.FromUserId,
                ToUserId = trr.ToUserId,
                Action = trr.Action,
                ChangedById = trr.ChangedById,
                FieldName = trr.FieldName,
                OldValue = trr.OldValue,
                NewValue = trr.NewValue,
                CreatedAt = trr.CreatedAt,
                Task = trr.Task == null ? null : new GetTaskResponse
                {
                    Id = trr.Task.Id,
                    Title = trr.Task.Title,
                    Description = trr.Task.Description,
                    StartDate = trr.Task.StartDate,
                    EndDate = trr.Task.EndDate,
                    Status = trr.Task.Status,
                    IsOverdue = trr.Task.IsOverdue,
                    UserId = trr.Task.UserId,
                    ProjectId = trr.Task.ProjectId
                },
                FromUser = trr.FromUser == null ? null : new GetUserResponse
                {
                    Id = trr.FromUser.Id,
                    UserName = trr.FromUser.UserName,
                    FullName = trr.FromUser.FullName,
                    Email = trr.FromUser.Email,
                    AvatarUrl = trr.FromUser.AvatarUrl,
                    PhoneNumber = trr.FromUser.PhoneNumber
                },
                ToUser = trr.ToUser == null ? null : new GetUserResponse
                {
                    Id = trr.ToUser.Id,
                    UserName = trr.ToUser.UserName,
                    FullName = trr.ToUser.FullName,
                    Email = trr.ToUser.Email,
                    AvatarUrl = trr.ToUser.AvatarUrl,
                    PhoneNumber = trr.ToUser.PhoneNumber
                },
                ChangedBy = trr.ChangedBy == null ? null : new GetUserResponse
                {
                    Id = trr.ChangedBy.Id,
                    UserName = trr.ChangedBy.UserName,
                    FullName = trr.ChangedBy.FullName,
                    Email = trr.ChangedBy.Email,
                    AvatarUrl = trr.ChangedBy.AvatarUrl,
                    PhoneNumber = trr.ChangedBy.PhoneNumber
                }
            });
            return ApiResponse<IEnumerable<GetTaskHistoryResponse>>.SuccessResponse(rs, "Fetch task histories for task successfully");
        }

    }
}
