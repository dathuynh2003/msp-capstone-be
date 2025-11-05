using Microsoft.AspNetCore.Identity;
using MSP.Application.Models.Requests.TaskReassignRequest;
using MSP.Application.Models.Responses.Auth;
using MSP.Application.Models.Responses.ProjectTask;
using MSP.Application.Models.Responses.TaskReassignRequest;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.TaskReassignRequest;
using MSP.Domain.Entities;
using MSP.Shared.Common;
using MSP.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Services.Implementations.TaskReassignRequest
{
    public class TaskReassignRequestService : ITaskReassignRequestService
    {
        private readonly ITaskReassignRequestRepository _taskReassignRequestRepo;
        private readonly IProjectTaskRepository _taskRepo;
        private readonly IProjectMemberRepository _memberRepo;
        private readonly UserManager<User> _userManager;

        public TaskReassignRequestService(ITaskReassignRequestRepository taskReassignRequestRepo,
            IProjectTaskRepository taskRepo,
            IProjectMemberRepository memberRepo,
            UserManager<User> userManager)
        {
            _taskReassignRequestRepo = taskReassignRequestRepo;
            _taskRepo = taskRepo;
            _memberRepo = memberRepo;
            _userManager = userManager;
        }

        public async Task<ApiResponse<GetTaskReassignRequestResponse>> AcceptTaskReassignRequest(Guid taskReassignRequestId, UpdateTaskReassignRequestRequest request)
        {
            var taskReassignRequest = await _taskReassignRequestRepo.GetTaskReassignRequestByIdAsync(taskReassignRequestId);
            if (taskReassignRequest == null)
                return ApiResponse<GetTaskReassignRequestResponse>.ErrorResponse(null, "Task Reassign Request not found");

            var task = taskReassignRequest.Task!;
            task.UserId = taskReassignRequest.ToUserId;
            task.UpdatedAt = DateTime.UtcNow;
            await _taskRepo.UpdateAsync(task);

            // Cập nhật trạng thái yêu cầu
            taskReassignRequest.ResponseMessage = request.ResponseMessage;
            taskReassignRequest.Status = TaskReassignEnum.Accepted.ToString();
            taskReassignRequest.RespondedAt = DateTime.UtcNow;
            taskReassignRequest.UpdatedAt = DateTime.UtcNow;

            await _taskReassignRequestRepo.UpdateAsync(taskReassignRequest);
            await _taskRepo.SaveChangesAsync();
            // Trả về response
            var rs = new GetTaskReassignRequestResponse
            {
                Id = taskReassignRequest.Id,
                TaskId = taskReassignRequest.TaskId,
                FromUserId = taskReassignRequest.FromUserId,
                ToUserId = taskReassignRequest.ToUserId,
                Status = taskReassignRequest.Status,
                Description = taskReassignRequest.Description,
                RespondedAt = taskReassignRequest.RespondedAt,
                ResponseMessage = taskReassignRequest.ResponseMessage,
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
                FromUser = new GetUserResponse
                {
                    Id = taskReassignRequest.FromUser.Id,
                    UserName = taskReassignRequest.FromUser.UserName,
                    FullName = taskReassignRequest.FromUser.FullName,
                    Email = taskReassignRequest.FromUser.Email,
                    AvatarUrl = taskReassignRequest.FromUser.AvatarUrl,
                    PhoneNumber = taskReassignRequest.FromUser.PhoneNumber
                },
                ToUser = new GetUserResponse
                {
                    Id = taskReassignRequest.ToUser.Id,
                    UserName = taskReassignRequest.ToUser.UserName,
                    FullName = taskReassignRequest.ToUser.FullName,
                    Email = taskReassignRequest.ToUser.Email,
                    AvatarUrl = taskReassignRequest.ToUser.AvatarUrl,
                    PhoneNumber = taskReassignRequest.ToUser.PhoneNumber
                }
            };
            return ApiResponse<GetTaskReassignRequestResponse>.SuccessResponse(rs);

        }

        public async Task<ApiResponse<GetTaskReassignRequestResponse>> CreateTaskReassignRequest(CreateTaskReassignRequestRequest request)
        {
            var task = await _taskRepo.GetByIdAsync(request.TaskId);
            if (task == null)
                return ApiResponse<GetTaskReassignRequestResponse>.ErrorResponse(null, "Task not found");
            var fromUser = await _userManager.FindByIdAsync(request.FromUserId.ToString());
            if (fromUser == null)
                return ApiResponse<GetTaskReassignRequestResponse>.ErrorResponse(null, "FromUser not found");
            var toUser = await _userManager.FindByIdAsync(request.ToUserId.ToString());
            if (fromUser == null)
                return ApiResponse<GetTaskReassignRequestResponse>.ErrorResponse(null, "ToUser not found");
            var taskReassignRequest = new MSP.Domain.Entities.TaskReassignRequest
            {
                TaskId = request.TaskId,
                FromUserId = request.FromUserId,
                ToUserId = request.ToUserId,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,

            };
            await _taskReassignRequestRepo.AddAsync(taskReassignRequest);
            await _taskReassignRequestRepo.SaveChangesAsync();
            var rs = new GetTaskReassignRequestResponse
            {
                TaskId = request.TaskId,
                FromUserId = request.FromUserId,
                ToUserId = request.ToUserId,
                Status = taskReassignRequest.Status,
                Description = taskReassignRequest.Description,
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
                FromUser = new GetUserResponse
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
            return ApiResponse<GetTaskReassignRequestResponse>.SuccessResponse(rs, "Đã gửi yêu cầu chuyển giao công việc thành công");
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

                if (roles.Contains("Member"))
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

        public async Task<ApiResponse<IEnumerable<GetTaskReassignRequestResponse>>> GetTaskReassignRequestsByTaskIdAsync(Guid taskId)
        {
            var requests = await _taskReassignRequestRepo.GetTaskReassignRequestsByTaskIdAsync(taskId);
            var rs = requests.Select(trr => new GetTaskReassignRequestResponse
            {
                Id = trr.Id,
                TaskId = trr.TaskId,
                FromUserId = trr.FromUserId,
                ToUserId = trr.ToUserId,
                Status = trr.Status,
                Description = trr.Description,
                RespondedAt = trr.RespondedAt,
                CreatedAt = trr.CreatedAt,
                ResponseMessage = trr.ResponseMessage,
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
                FromUser = new GetUserResponse
                {
                    Id = trr.FromUser.Id,
                    UserName = trr.FromUser.UserName,
                    FullName = trr.FromUser.FullName,
                    Email = trr.FromUser.Email,
                    AvatarUrl = trr.FromUser.AvatarUrl,
                    PhoneNumber = trr.FromUser.PhoneNumber
                },
                ToUser = new GetUserResponse
                {
                    Id = trr.ToUser.Id,
                    UserName = trr.ToUser.UserName,
                    FullName = trr.ToUser.FullName,
                    Email = trr.ToUser.Email,
                    AvatarUrl = trr.ToUser.AvatarUrl,
                    PhoneNumber = trr.ToUser.PhoneNumber
                }
            });
            return ApiResponse<IEnumerable<GetTaskReassignRequestResponse>>.SuccessResponse(rs, "Fetch history task reassign request for task successfully");
        }

        public async Task<ApiResponse<IEnumerable<GetTaskReassignRequestResponse>>> GetTaskReassignRequestsForUserAsync(Guid userId)
        {
            var requests = await _taskReassignRequestRepo.GetTaskReassignRequestsForUserAsync(userId);
            var rs = requests.Select(trr => new GetTaskReassignRequestResponse
            {
                Id = trr.Id,
                TaskId = trr.TaskId,
                FromUserId = trr.FromUserId,
                ToUserId = trr.ToUserId,
                Status = trr.Status,
                Description = trr.Description,
                RespondedAt = trr.RespondedAt,
                CreatedAt = trr.CreatedAt,
                ResponseMessage = trr.ResponseMessage,
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
                FromUser = new GetUserResponse
                {
                    Id = trr.FromUser.Id,
                    UserName = trr.FromUser.UserName,
                    FullName = trr.FromUser.FullName,
                    Email = trr.FromUser.Email,
                    AvatarUrl = trr.FromUser.AvatarUrl,
                    PhoneNumber = trr.FromUser.PhoneNumber
                },
                ToUser = new GetUserResponse
                {
                    Id = trr.ToUser.Id,
                    UserName = trr.ToUser.UserName,
                    FullName = trr.ToUser.FullName,
                    Email = trr.ToUser.Email,
                    AvatarUrl = trr.ToUser.AvatarUrl,
                    PhoneNumber = trr.ToUser.PhoneNumber
                }
            });
            return ApiResponse<IEnumerable<GetTaskReassignRequestResponse>>.SuccessResponse(rs, "Fetch task reassign request by user successfully");

        }

        public async Task<ApiResponse<GetTaskReassignRequestResponse>> RejectTaskReassignRequest(Guid taskReassignRequestId, UpdateTaskReassignRequestRequest request)
        {
            var taskReassignRequest = await _taskReassignRequestRepo.GetTaskReassignRequestByIdAsync(taskReassignRequestId);
            if (taskReassignRequest == null)
                return ApiResponse<GetTaskReassignRequestResponse>.ErrorResponse(null, "Task Reassign Request not found");

            var task = taskReassignRequest.Task!;
            task.UserId = taskReassignRequest.ToUserId;
            task.UpdatedAt = DateTime.UtcNow;
            await _taskRepo.UpdateAsync(task);
            // Cập nhật trạng thái yêu cầu
            taskReassignRequest.ResponseMessage = request.ResponseMessage;
            taskReassignRequest.Status = TaskReassignEnum.Rejected.ToString();
            taskReassignRequest.RespondedAt = DateTime.UtcNow;
            taskReassignRequest.UpdatedAt = DateTime.UtcNow;

            await _taskReassignRequestRepo.UpdateAsync(taskReassignRequest);
            await _taskRepo.SaveChangesAsync();
            // Trả về response
            var rs = new GetTaskReassignRequestResponse
            {
                Id = taskReassignRequest.Id,
                TaskId = taskReassignRequest.TaskId,
                FromUserId = taskReassignRequest.FromUserId,
                ToUserId = taskReassignRequest.ToUserId,
                Status = taskReassignRequest.Status,
                Description = taskReassignRequest.Description,
                RespondedAt = taskReassignRequest.RespondedAt,
                ResponseMessage = taskReassignRequest.ResponseMessage,
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
                FromUser = new GetUserResponse
                {
                    Id = taskReassignRequest.FromUser.Id,
                    UserName = taskReassignRequest.FromUser.UserName,
                    FullName = taskReassignRequest.FromUser.FullName,
                    Email = taskReassignRequest.FromUser.Email,
                    AvatarUrl = taskReassignRequest.FromUser.AvatarUrl,
                    PhoneNumber = taskReassignRequest.FromUser.PhoneNumber
                },
                ToUser = new GetUserResponse
                {
                    Id = taskReassignRequest.ToUser.Id,
                    UserName = taskReassignRequest.ToUser.UserName,
                    FullName = taskReassignRequest.ToUser.FullName,
                    Email = taskReassignRequest.ToUser.Email,
                    AvatarUrl = taskReassignRequest.ToUser.AvatarUrl,
                    PhoneNumber = taskReassignRequest.ToUser.PhoneNumber
                }
            };
            return ApiResponse<GetTaskReassignRequestResponse>.SuccessResponse(rs, "Từ chối yêu cầu chuyển giao công việc thành công");
        }
    }
}
