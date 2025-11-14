using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MSP.Application.Models.Requests.ProjectTask;
using MSP.Application.Models.Requests.TaskReassignRequest;
using MSP.Application.Models.Responses.Auth;
using MSP.Application.Models.Responses.Milestone;
using MSP.Application.Models.Responses.ProjectTask;
using MSP.Application.Models.Responses.TaskHistory;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.ProjectTask;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Models.Requests.Notification;
using MSP.Application.Services.Interfaces.TaskHistory;
using MSP.Domain.Entities;
using MSP.Shared.Common;
using MSP.Shared.Enums;

namespace MSP.Application.Services.Implementations.ProjectTask
{
    public class ProjectTaskService : IProjectTaskService
    {
        private readonly IProjectTaskRepository _projectTaskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IMilestoneRepository _milestoneRepository;
        private readonly ITodoRepository _todoRepository;
        private readonly ITaskHistoryService _taskHistoryService;
        private readonly UserManager<User> _userManager;
        private readonly INotificationService _notificationService;

        public ProjectTaskService(IProjectTaskRepository projectTaskRepository, IProjectRepository projectRepository, IMilestoneRepository milestoneRepository, UserManager<User> userManager, ITodoRepository todoRepository, ITaskHistoryService taskHistoryService, INotificationService notificationService)
        {
            _projectTaskRepository = projectTaskRepository;
            _projectRepository = projectRepository;
            _milestoneRepository = milestoneRepository;
            _userManager = userManager;
            _todoRepository = todoRepository;
            _taskHistoryService = taskHistoryService;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<GetTaskResponse>> CreateTaskAsync(CreateTaskRequest request)
        {
            var project = await _projectRepository.GetByIdAsync(request.ProjectId);
            if (project == null || project.IsDeleted)
            {
                return ApiResponse<GetTaskResponse>.ErrorResponse(null, "Project not found");
            }

            User? user = null;
            if (request.UserId.HasValue)
            {
                user = await _userManager.FindByIdAsync(request.UserId.Value.ToString());
                if (user == null)
                {
                    return ApiResponse<GetTaskResponse>.ErrorResponse(null, "User not found");
                }
            }

            var newTask = new Domain.Entities.ProjectTask
            {
                ProjectId = request.ProjectId,
                UserId = request.UserId,  // UserId nullable, có thể là null
                Title = request.Title,
                Description = request.Description,
                Status = request.Status,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CreatedAt = DateTime.UtcNow
            };

            // Gán milestone nếu có
            if (request.MilestoneIds != null && request.MilestoneIds.Any())
            {
                var milestones = await _milestoneRepository.GetMilestonesByIdsAsync(request.MilestoneIds);
                if (milestones == null || !milestones.Any() || milestones.Any(m => m.IsDeleted))
                {
                    return ApiResponse<GetTaskResponse>.ErrorResponse(null, "Some milestones not found or have been deleted");
                }

                foreach (var milestone in milestones)
                {
                    _milestoneRepository.Attach(milestone);
                    newTask.Milestones.Add(milestone);
                }
            }

            await _projectTaskRepository.AddAsync(newTask);
            await _projectTaskRepository.SaveChangesAsync();

            // Gửi notification nếu task được assign cho user
            if (request.UserId.HasValue && user != null)
            {
                var notificationRequest = new CreateNotificationRequest
                {
                    UserId = request.UserId.Value,
                    ActorId = request.ActorId,
                    Title = "Công việc mới được giao",
                    Message = $"Bạn đã được giao công việc: {newTask.Title} trong dự án {project.Name}",
                    Type = NotificationTypeEnum.TaskAssignment.ToString(),
                    EntityId = newTask.Id.ToString(),
                    Data = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        TaskId = newTask.Id,
                        TaskTitle = newTask.Title,
                        ProjectId = project.Id,
                        ProjectName = project.Name,
                        DueDate = newTask.EndDate
                    })
                };

                await _notificationService.CreateInAppNotificationAsync(notificationRequest);

                // Gửi email notification (async via Hangfire)
                _notificationService.SendEmailNotification(
                    user.Email!,
                    "Công việc mới được giao",
                    $"Xin chào {user.FullName},<br/><br/>" +
                    $"Bạn đã được giao công việc mới: <strong>{newTask.Title}</strong><br/>" +
                    $"Dự án: {project.Name}<br/>" +
                    $"Hạn chót: {newTask.EndDate:dd/MM/yyyy}<br/><br/>" +
                    $"Vui lòng kiểm tra bảng điều khiển để biết thêm chi tiết."
                );
            }

            // Tạo TaskHistory nếu có UserId
            if (request.UserId != null)
            {
                var newTaskHistory = new CreateTaskHistoryRequest
                {
                    TaskId = newTask.Id,
                    FromUserId = null,
                    ToUserId = request.UserId.Value
                };
                await _taskHistoryService.CreateTaskHistoryAsync(newTaskHistory);
            }

            var response = new GetTaskResponse
            {
                Id = newTask.Id,
                ProjectId = newTask.ProjectId,
                UserId = newTask.UserId,
                Title = newTask.Title,
                Description = newTask.Description,
                Status = newTask.Status,
                StartDate = newTask.StartDate,
                EndDate = newTask.EndDate,
                CreatedAt = newTask.CreatedAt,
                UpdatedAt = newTask.UpdatedAt,
                User = user == null ? null : new GetUserResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    AvatarUrl = user.AvatarUrl,
                },
                Milestones = newTask.Milestones?.Select(m => new GetMilestoneResponse
                {
                    Id = m.Id,
                    ProjectId = m.ProjectId,
                    Name = m.Name,
                    DueDate = m.DueDate
                }).ToArray()
            };

            return ApiResponse<GetTaskResponse>.SuccessResponse(response, "Task created successfully");
        }


        public async Task<ApiResponse<string>> DeleteTaskAsync(Guid taskId)
        {
            var task = await _projectTaskRepository.GetTaskByIdAsync(taskId);
            if (task == null)
            {
                return ApiResponse<string>.ErrorResponse(null, "Task not found");
            }
            await _projectTaskRepository.SoftDeleteAsync(task);
            await _projectTaskRepository.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse("Task deleted successfully");
        }

        public async Task<ApiResponse<GetTaskResponse>> GetTaskByIdAsync(Guid taskId)
        {
            var task = await _projectTaskRepository.GetTaskByIdAsync(taskId);
            if (task == null)
            {
                return ApiResponse<GetTaskResponse>.ErrorResponse(null, "Task not found");
            }
            var response = new GetTaskResponse
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                UserId = task.UserId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                User = task.User == null ? null : new GetUserResponse
                {
                    Id = task.User.Id,
                    Email = task.User.Email,
                    FullName = task.User.FullName,
                    AvatarUrl = task.User.AvatarUrl,
                },
                Milestones = task.Milestones?.Select(m => new GetMilestoneResponse
                {
                    Id = m.Id,
                    ProjectId = m.ProjectId,
                    Name = m.Name,
                    DueDate = m.DueDate
                }).ToArray(),
            };
            return ApiResponse<GetTaskResponse>.SuccessResponse(response, "Task retrieved successfully");
        }

        public async Task<ApiResponse<PagingResponse<GetTaskResponse>>> GetTasksByProjectIdAsync(PagingRequest request, Guid projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                return ApiResponse<PagingResponse<GetTaskResponse>>.ErrorResponse(null, "Project not found");
            }

            var tasks = await _projectTaskRepository.FindWithIncludePagedAsync(
                predicate: p => p.ProjectId == projectId && !p.IsDeleted,
                include: query => query
                    .Include(p => p.User)
                    .Include(p => p.Milestones),
                pageNumber: request.PageIndex,
                pageSize: request.PageSize,
                asNoTracking: true);

            if (tasks == null || !tasks.Any())
            {
                return ApiResponse<PagingResponse<GetTaskResponse>>.ErrorResponse(null, "No tasks found for the project");
            }

            var totalTasks = await _projectTaskRepository.CountAsync(p => p.ProjectId == projectId && !p.IsDeleted);

            var response = new PagingResponse<GetTaskResponse>
            {
                Items = tasks.Select(task => new GetTaskResponse
                {
                    Id = task.Id,
                    ProjectId = task.ProjectId,
                    UserId = task.UserId,
                    Title = task.Title,
                    Description = task.Description,
                    Status = task.Status,
                    StartDate = task.StartDate,
                    EndDate = task.EndDate,
                    CreatedAt = task.CreatedAt,
                    UpdatedAt = task.UpdatedAt,
                    User = task.User == null ? null : new GetUserResponse
                    {
                        Id = task.User.Id,
                        Email = task.User.Email,
                        FullName = task.User.FullName,
                        AvatarUrl = task.User.AvatarUrl,
                    },
                    Milestones = task.Milestones?.Select(m => new GetMilestoneResponse
                    {
                        Id = m.Id,
                        ProjectId = m.ProjectId,
                        Name = m.Name,
                        DueDate = m.DueDate
                    }).ToArray()
                }),
                TotalItems = totalTasks,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };

            return ApiResponse<PagingResponse<GetTaskResponse>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<PagingResponse<GetTaskResponse>>> GetTasksByUserIdAndProjectIdAsync(PagingRequest request, Guid userId, Guid projectId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return ApiResponse<PagingResponse<GetTaskResponse>>.ErrorResponse(null, "User not found");
            }

            var tasks = await _projectTaskRepository.FindWithIncludePagedAsync(
                predicate: p => p.UserId == userId && p.ProjectId == projectId && !p.IsDeleted,
                include: query => query
                    .Include(p => p.User)
                    .Include(p => p.Milestones),
                pageNumber: request.PageIndex,
                pageSize: request.PageSize,
                asNoTracking: true
            );

            if (tasks == null || !tasks.Any())
            {
                return ApiResponse<PagingResponse<GetTaskResponse>>.ErrorResponse(null, "No tasks found for this user and project");
            }

            var totalTasks = await _projectTaskRepository.CountAsync(p => p.UserId == userId && p.ProjectId == projectId && !p.IsDeleted);

            var response = new PagingResponse<GetTaskResponse>
            {
                Items = tasks.Select(task => new GetTaskResponse
                {
                    Id = task.Id,
                    ProjectId = task.ProjectId,
                    UserId = task.UserId,
                    Title = task.Title,
                    Description = task.Description,
                    Status = task.Status,
                    StartDate = task.StartDate,
                    EndDate = task.EndDate,
                    CreatedAt = task.CreatedAt,
                    UpdatedAt = task.UpdatedAt,
                    User = task.User == null ? null : new GetUserResponse
                    {
                        Id = task.User.Id,
                        Email = task.User.Email,
                        FullName = task.User.FullName,
                        AvatarUrl = task.User.AvatarUrl,
                    },
                    Milestones = task.Milestones?.Select(m => new GetMilestoneResponse
                    {
                        Id = m.Id,
                        ProjectId = m.ProjectId,
                        Name = m.Name,
                        DueDate = m.DueDate
                    }).ToArray()
                }),
                TotalItems = totalTasks,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };

            return ApiResponse<PagingResponse<GetTaskResponse>>.SuccessResponse(response);
        }


        public async Task<ApiResponse<GetTaskResponse>> UpdateTaskAsync(UpdateTaskRequest request)
        {
            var task = await _projectTaskRepository.GetTaskByIdAsync(request.Id);

            if (task == null || task.IsDeleted)
            {
                return ApiResponse<GetTaskResponse>.ErrorResponse(null, "Task not found");
            }
            var oldUserId = task.UserId;
            var project = await _projectRepository.GetByIdAsync(task.ProjectId);
            if (project == null || project.IsDeleted)
            {
                return ApiResponse<GetTaskResponse>.ErrorResponse(null, "Project not found or has been deleted");
            }

            // Lưu UserId cũ để so sánh
            var oldUserId = task.UserId;

            User? user = null;
            if (request.UserId.HasValue)
            {
                user = await _userManager.FindByIdAsync(request.UserId.Value.ToString());
                if (user == null)
                {
                    return ApiResponse<GetTaskResponse>.ErrorResponse(null, "User not found");
                }
            }

            task.Title = request.Title;
            task.Description = request.Description;
            task.Status = request.Status;
            task.StartDate = request.StartDate;
            task.EndDate = request.EndDate;
            task.UserId = request.UserId;
            task.UpdatedAt = DateTime.UtcNow;

            // Cập nhật milestones (xóa các milestone hiện tại, gán milestones mới)
            if (request.MilestoneIds != null)
            {
                var milestones = await _milestoneRepository.GetMilestonesByIdsAsync(request.MilestoneIds);
                if (milestones == null || milestones.Count() != request.MilestoneIds.Length || milestones.Any(m => m.IsDeleted))
                {
                    return ApiResponse<GetTaskResponse>.ErrorResponse(null, "Some milestones not found or have been deleted");
                }

                // Xóa các milestones cũ
                task.Milestones.Clear();
                // Thêm milestones mới
                foreach (var milestone in milestones)
                {
                    _milestoneRepository.Attach(milestone);
                    task.Milestones.Add(milestone);
                }
            }

            await _projectTaskRepository.UpdateAsync(task);
            await _projectTaskRepository.SaveChangesAsync();

            // Gửi notification nếu task được assign/reassign cho user khác
            if (request.UserId.HasValue && request.UserId != oldUserId && user != null)
            {
                string notificationTitle;
                string notificationMessage;
                string emailSubject;
                string emailBody;

                if (oldUserId.HasValue)
                {
                    // Task được reassign từ user khác
                    notificationTitle = "Công việc được giao lại";
                    notificationMessage = $"Công việc '{task.Title}' đã được giao lại cho bạn trong dự án {project.Name}";
                    emailSubject = "Công việc được giao lại";
                    emailBody = $"Xin chào {user.FullName},<br/><br/>" +
                               $"Công việc <strong>{task.Title}</strong> đã được giao lại cho bạn.<br/>" +
                               $"Dự án: {project.Name}<br/>" +
                               $"Hạn chót: {task.EndDate:dd/MM/yyyy}<br/><br/>" +
                               $"Vui lòng kiểm tra bảng điều khiển để biết thêm chi tiết.";
                }
                else
                {
                    // Task được assign lần đầu
                    notificationTitle = "Công việc mới được giao";
                    notificationMessage = $"Bạn đã được giao công việc: {task.Title} trong dự án {project.Name}";
                    emailSubject = "Công việc mới được giao";
                    emailBody = $"Xin chào {user.FullName},<br/><br/>" +
                               $"Bạn đã được giao công việc mới: <strong>{task.Title}</strong><br/>" +
                               $"Dự án: {project.Name}<br/>" +
                               $"Hạn chót: {task.EndDate:dd/MM/yyyy}<br/><br/>" +
                               $"Vui lòng kiểm tra bảng điều khiển để biết thêm chi tiết.";
                }

                var notificationRequest = new CreateNotificationRequest
                {
                    UserId = request.UserId.Value,
                    ActorId = request.ActorId,
                    Title = notificationTitle,
                    Message = notificationMessage,
                    Type = NotificationTypeEnum.TaskAssignment.ToString(),
                    EntityId = task.Id.ToString(),
                    Data = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        TaskId = task.Id,
                        TaskTitle = task.Title,
                        ProjectId = project.Id,
                        ProjectName = project.Name,
                        DueDate = task.EndDate,
                        IsReassignment = oldUserId.HasValue
                    })
                };

                await _notificationService.CreateInAppNotificationAsync(notificationRequest);

                // Gửi email notification (async via Hangfire)
                _notificationService.SendEmailNotification(
                    user.Email!,
                    emailSubject,
                    emailBody
                );
            }

            if (request.UserId != null)
            {
                if (oldUserId == request.UserId)
                {
                    // Nếu user không thay đổi, không tạo lịch sử
                    return ApiResponse<GetTaskResponse>.SuccessResponse(null, "Task updated successfully");
                }
                if (oldUserId != request.UserId)
                {
                    var newTaskHistory = new CreateTaskHistoryRequest
                    {
                        TaskId = request.Id,
                        FromUserId = oldUserId,
                        ToUserId = request.UserId.Value
                    };
                    await _taskHistoryService.CreateTaskHistoryAsync(newTaskHistory);
                }


            }
            
            var response = new GetTaskResponse
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                UserId = task.UserId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                User = task.User == null ? null : new GetUserResponse
                {
                    Id = task.User.Id,
                    Email = task.User.Email,
                    FullName = task.User.FullName,
                    AvatarUrl = task.User.AvatarUrl,
                },
                Milestones = task.Milestones?.Select(m => new GetMilestoneResponse
                {
                    Id = m.Id,
                    ProjectId = m.ProjectId,
                    Name = m.Name,
                    DueDate = m.DueDate
                }).ToArray()
            };

            return ApiResponse<GetTaskResponse>.SuccessResponse(response, "Task updated successfully");
        }


        public async Task<ApiResponse<List<GetTaskResponse>>> GetTasksByMilestoneIdAsync(Guid milestoneId)
        {
            var milestone = await _milestoneRepository.GetMilestoneByIdAsync(milestoneId);
            if (milestone == null)
            {
                return ApiResponse<List<GetTaskResponse>>.ErrorResponse(null, "Milestone not found");
            }
            var tasks = await _projectTaskRepository.GetTasksByMilestoneIdAsync(milestoneId);
            if (tasks == null || !tasks.Any())
            {
                return ApiResponse<List<GetTaskResponse>>.ErrorResponse(null, "No tasks found for the milestone");
            }
            var response = tasks.Select(task => new GetTaskResponse
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                UserId = task.UserId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                User = task.User == null ? null : new GetUserResponse
                {
                    Id = task.User.Id,
                    Email = task.User.Email,
                    FullName = task.User.FullName,
                    AvatarUrl = task.User.AvatarUrl,
                },
                Milestones = task.Milestones?.Select(m => new GetMilestoneResponse
                {
                    Id = m.Id,
                    ProjectId = m.ProjectId,
                    Name = m.Name,
                    DueDate = m.DueDate
                }).ToArray()
            }).ToList();

            return ApiResponse<List<GetTaskResponse>>.SuccessResponse(response, "Tasks retrieved successfully");
        }

        public async Task<ApiResponse<List<GetTaskResponse>>> GetTasksByTodoIdAsync(Guid todoId)
        {
            var todo = await _todoRepository.GetByIdAsync(todoId);
            if (todo == null)
            {
                return ApiResponse<List<GetTaskResponse>>.ErrorResponse(null, "Todo not found");
            }
            var tasks = await _projectTaskRepository.GetTasksByTodoIdAsync(todoId);
            tasks ??= new List<Domain.Entities.ProjectTask>();

            var response = tasks.Select(task => new GetTaskResponse
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                UserId = task.UserId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                User = task.User == null ? null : new GetUserResponse
                {
                    Id = task.User.Id,
                    Email = task.User.Email,
                    FullName = task.User.FullName,
                    AvatarUrl = task.User.AvatarUrl,
                },
                Milestones = task.Milestones?.Select(m => new GetMilestoneResponse
                {
                    Id = m.Id,
                    ProjectId = m.ProjectId,
                    Name = m.Name,
                    DueDate = m.DueDate
                }).ToArray()
            }).ToList();

            return ApiResponse<List<GetTaskResponse>>.SuccessResponse(response, "Tasks retrieved successfully");
        }
    }
}
