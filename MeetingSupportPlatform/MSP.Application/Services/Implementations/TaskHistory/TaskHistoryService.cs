using Microsoft.AspNetCore.Identity;
using MSP.Application.Models.Requests.TaskReassignRequest;
using MSP.Application.Models.Responses.Auth;
using MSP.Application.Models.Responses.ProjectTask;
using MSP.Application.Models.Responses.TaskHistory;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.TaskHistory;
using MSP.Domain.Entities;
using MSP.Shared.Common;
using MSP.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


        public async Task<ApiResponse<GetTaskHistoryResponse>> CreateTaskHistoryAsync(CreateTaskHistoryRequest request)
        {
            var task = await _taskRepo.GetByIdAsync(request.TaskId);
            if (task == null)
                return ApiResponse<GetTaskHistoryResponse>.ErrorResponse(null, "Task not found");
            var fromUser = null as User;
            if (request.FromUserId != null)
                fromUser = await _userManager.FindByIdAsync(request.FromUserId.ToString());
            var toUser = await _userManager.FindByIdAsync(request.ToUserId.ToString());
            if (toUser == null)
                return ApiResponse<GetTaskHistoryResponse>.ErrorResponse(null, "ToUser not found");
            var taskHistory = new MSP.Domain.Entities.TaskHistory
            {
                TaskId = request.TaskId,
                FromUserId = request.FromUserId,
                ToUserId = request.ToUserId,
                CreatedAt = DateTime.UtcNow,

            };
            await _taskHistoryRepo.AddAsync(taskHistory);
            await _taskHistoryRepo.SaveChangesAsync();
            var rs = new GetTaskHistoryResponse
            {
                TaskId = request.TaskId,
                FromUserId = request.FromUserId,
                ToUserId = request.ToUserId,
                Task = new GetTaskResponse
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    StartDate = task.StartDate,
                    EndDate = task.EndDate,
                    Status = task.Status,
                    UserId = task.UserId,
                    ProjectId = task.ProjectId
                },
                FromUser = fromUser == null ? null : new GetUserResponse
                {
                    Id = fromUser.Id,
                    UserName = fromUser.UserName,
                    FullName = fromUser.FullName,
                    Email = fromUser.Email,
                    AvatarUrl = fromUser.AvatarUrl,
                    PhoneNumber = fromUser.PhoneNumber
                },
                ToUser = new GetUserResponse
                {
                    Id = toUser.Id,
                    UserName = toUser.UserName,
                    FullName = toUser.FullName,
                    Email = toUser.Email,
                    AvatarUrl = toUser.AvatarUrl,
                    PhoneNumber = toUser.PhoneNumber
                },
            };
            return ApiResponse<GetTaskHistoryResponse>.SuccessResponse(rs, "Create task history successfully");
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
                AssignedAt = trr.CreatedAt,
                Task = new GetTaskResponse
                {
                    Id = trr.Task!.Id,
                    Title = trr.Task.Title,
                    Description = trr.Task.Description,
                    StartDate = trr.Task.StartDate,
                    EndDate = trr.Task.EndDate,
                    Status = trr.Task.Status,
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
                }
            });
            return ApiResponse<IEnumerable<GetTaskHistoryResponse>>.SuccessResponse(rs, "Fetch task histories for task successfully");
        }

    }
}
