using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MSP.Application.Models.Requests.ProjectTask;
using MSP.Application.Models.Responses.Auth;
using MSP.Application.Models.Responses.Milestone;
using MSP.Application.Models.Responses.ProjectTask;
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
                ReviewerId = null,
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

            var strategy = _projectTaskRepository.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _projectTaskRepository.BeginTransactionAsync();
                try
                {
                    await _projectTaskRepository.AddAsync(newTask);
                    await _projectTaskRepository.SaveChangesAsync();

                    // AUTO TRACK: Tạo history cho task creation
                    await _taskHistoryService.TrackTaskCreationAsync(
                        newTask.Id,
                        request.ActorId,
                        request.UserId);

                    // AUTO TRACK: Nếu có assign ngay từ đầu
                    if (request.UserId.HasValue)
                    {
                        await _taskHistoryService.TrackTaskAssignmentAsync(
                            newTask.Id,
                            null, // fromUserId = null (first assignment)
                            request.UserId.Value,
                            request.ActorId);
                    }

                    await _projectTaskRepository.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Gửi notification nếu task được assign cho user
                    if (request.UserId.HasValue && user != null)
                    {
                        var notificationRequest = new CreateNotificationRequest
                        {
                            UserId = request.UserId.Value,
                            ActorId = request.ActorId,
                            Title = "New task assigned",
                            Message = $"You have been assigned a new task: {newTask.Title} in project {project.Name}",
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

                        _notificationService.SendEmailNotification(
                            user.Email!,
                            "New task assigned",
                            $"Hello {user.FullName},<br/><br/>" +
                            $"You have been assigned a new task: <strong>{newTask.Title}</strong><br/>" +
                            $"Project: {project.Name}<br/>" +
                            $"Due date: {newTask.EndDate:dd/MM/yyyy}<br/><br/>" +
                            $"Please check your dashboard for more details."
                        );
                    }
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            var response = new GetTaskResponse
            {
                Id = newTask.Id,
                ProjectId = newTask.ProjectId,
                UserId = newTask.UserId,
                ReviewerId = newTask.ReviewerId,
                Title = newTask.Title,
                Description = newTask.Description,
                Status = newTask.Status,
                StartDate = newTask.StartDate,
                EndDate = newTask.EndDate,
                IsOverdue = newTask.IsOverdue,
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
                ReviewerId = task.ReviewerId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                IsOverdue = task.IsOverdue,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                User = task.User == null ? null : new GetUserResponse
                {
                    Id = task.User.Id,
                    Email = task.User.Email,
                    FullName = task.User.FullName,
                    AvatarUrl = task.User.AvatarUrl,
                },
                Reviewer = task.Reviewer == null ? null : new GetUserResponse
                {
                    Id = task.Reviewer.Id,
                    Email = task.Reviewer.Email,
                    FullName = task.Reviewer.FullName,
                    AvatarUrl = task.Reviewer.AvatarUrl,
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
                    .Include(p => p.Reviewer)
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
                    ReviewerId = task.ReviewerId,
                    Title = task.Title,
                    Description = task.Description,
                    Status = task.Status,
                    StartDate = task.StartDate,
                    EndDate = task.EndDate,
                    IsOverdue = task.IsOverdue,
                    CreatedAt = task.CreatedAt,
                    UpdatedAt = task.UpdatedAt,
                    User = task.User == null ? null : new GetUserResponse
                    {
                        Id = task.User.Id,
                        Email = task.User.Email,
                        FullName = task.User.FullName,
                        AvatarUrl = task.User.AvatarUrl,
                    },
                    Reviewer = task.Reviewer == null ? null : new GetUserResponse
                    {
                        Id = task.Reviewer.Id,
                        Email = task.Reviewer.Email,
                        FullName = task.Reviewer.FullName,
                        AvatarUrl = task.Reviewer.AvatarUrl,
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
                    .Include(p => p.Reviewer)
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
                    ReviewerId = task.ReviewerId,
                    Title = task.Title,
                    Description = task.Description,
                    Status = task.Status,
                    StartDate = task.StartDate,
                    EndDate = task.EndDate,
                    IsOverdue = task.IsOverdue,
                    CreatedAt = task.CreatedAt,
                    UpdatedAt = task.UpdatedAt,
                    User = task.User == null ? null : new GetUserResponse
                    {
                        Id = task.User.Id,
                        Email = task.User.Email,
                        FullName = task.User.FullName,
                        AvatarUrl = task.User.AvatarUrl,
                    },
                    Reviewer = task.Reviewer == null ? null : new GetUserResponse
                    {
                        Id = task.Reviewer.Id,
                        Email = task.Reviewer.Email,
                        FullName = task.Reviewer.FullName,
                        AvatarUrl = task.Reviewer.AvatarUrl,
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

            var project = await _projectRepository.GetByIdAsync(task.ProjectId);
            if (project == null || project.IsDeleted)
            {
                return ApiResponse<GetTaskResponse>.ErrorResponse(null, "Project not found or has been deleted");
            }

            // Lưu giá trị cũ để track changes
            var oldTitle = task.Title;
            var oldDescription = task.Description;
            var oldStatus = task.Status;
            var oldStartDate = task.StartDate;
            var oldEndDate = task.EndDate;
            var oldUserId = task.UserId;
            var oldReviewerId = task.ReviewerId;

            User? newUser = null;
            if (request.UserId.HasValue)
            {
                newUser = await _userManager.FindByIdAsync(request.UserId.Value.ToString());
                if (newUser == null)
                {
                    return ApiResponse<GetTaskResponse>.ErrorResponse(null, "User not found");
                }
            }

            User? newReviewer = null;
            if (request.ReviewerId.HasValue)
            {
                newReviewer = await _userManager.FindByIdAsync(request.ReviewerId.Value.ToString());
                if (newReviewer == null)
                {
                    return ApiResponse<GetTaskResponse>.ErrorResponse(null, "Reviewer not found");
                }

                // Verify reviewer is a ProjectManager
                var reviewerRoles = await _userManager.GetRolesAsync(newReviewer);
                if (!reviewerRoles.Contains("ProjectManager"))
                {
                    return ApiResponse<GetTaskResponse>.ErrorResponse(null, "Reviewer must be a Project Manager");
                }
            }

            var strategy = _projectTaskRepository.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                // Bắt đầu transaction
                using var transaction = await _projectTaskRepository.BeginTransactionAsync();
                try
                {
                    // AUTO TRACK: Title change
                    if (!string.IsNullOrEmpty(request.Title) && task.Title != request.Title)
                    {
                        await _taskHistoryService.TrackFieldChangeAsync(
                            task.Id,
                            "Title",
                            oldTitle,
                            request.Title,
                            request.ActorId);
                        task.Title = request.Title;
                    }

                    // AUTO TRACK: Description change
                    if (!string.IsNullOrEmpty(request.Description) && task.Description != request.Description)
                    {
                        await _taskHistoryService.TrackFieldChangeAsync(
                            task.Id,
                            "Description",
                            oldDescription,
                            request.Description,
                            request.ActorId);
                        task.Description = request.Description;
                    }

                    // AUTO TRACK: Status change
                    if (!string.IsNullOrEmpty(request.Status) && task.Status != request.Status)
                    {
                        await _taskHistoryService.TrackStatusChangeAsync(
                            task.Id,
                            oldStatus,
                            request.Status,
                            request.ActorId);
                        task.Status = request.Status;

                        // Gửi notification cho reviewer khi status change to ReadyToReview
                        if (request.Status == TaskEnum.ReadyToReview.ToString() && task.ReviewerId.HasValue)
                        {
                            var reviewer = task.Reviewer ?? await _userManager.FindByIdAsync(task.ReviewerId.Value.ToString());
                            if (reviewer != null)
                            {
                                var reviewNotification = new CreateNotificationRequest
                                {
                                    UserId = task.ReviewerId.Value,
                                    ActorId = request.ActorId,
                                    Title = "Task review request",
                                    Message = $"{task.User?.FullName ?? "A team member"} is requesting you to review the task '{task.Title}' in project {project.Name}",
                                    Type = NotificationTypeEnum.TaskUpdate.ToString(),
                                    EntityId = task.Id.ToString(),
                                    Data = System.Text.Json.JsonSerializer.Serialize(new
                                    {
                                        TaskId = task.Id,
                                        TaskTitle = task.Title,
                                        ProjectId = project.Id,
                                        ProjectName = project.Name,
                                        AssigneeId = task.UserId,
                                        AssigneeName = task.User?.FullName,
                                        Status = request.Status
                                    })
                                };

                                await _notificationService.CreateInAppNotificationAsync(reviewNotification);

                                _notificationService.SendEmailNotification(
                                    reviewer.Email!,
                                    "Task review request",
                                    $"Hello {reviewer.FullName},<br/><br/>" +
                                    $"<strong>{task.User?.FullName ?? "A team member"}</strong> has completed and is requesting you to review the following task:<br/><br/>" +
                                    $"📋 <strong>Task:</strong> {task.Title}<br/>" +
                                    $"📁 <strong>Project:</strong> {project.Name}<br/>" +
                                    $"👤 <strong>Assignee:</strong> {task.User?.FullName ?? "N/A"}<br/>" +
                                    $"📅 <strong>Completion date:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}<br/><br/>" +
                                    $"Please access the system to review and provide feedback."
                                );
                            }
                        }

                        // Gửi notification khi PM reopen task 
                        if (request.Status == TaskEnum.ReOpened.ToString() &&
                            oldStatus == TaskEnum.ReadyToReview.ToString() &&
                            task.UserId.HasValue)
                        {
                            var assignee = task.User ?? await _userManager.FindByIdAsync(task.UserId.Value.ToString());
                            if (assignee != null)
                            {
                                var reopenNotification = new CreateNotificationRequest
                                {
                                    UserId = task.UserId.Value,
                                    ActorId = request.ActorId,
                                    Title = "Công việc được mở lại",
                                    Message = $"Công việc '{task.Title}' đã được mở lại bởi Project Manager trong dự án {project.Name}",
                                    Type = NotificationTypeEnum.TaskUpdate.ToString(),
                                    EntityId = task.Id.ToString(),
                                    Data = System.Text.Json.JsonSerializer.Serialize(new
                                    {
                                        TaskId = task.Id,
                                        TaskTitle = task.Title,
                                        ProjectId = project.Id,
                                        ProjectName = project.Name,
                                        OldStatus = oldStatus,
                                        NewStatus = request.Status,
                                        DueDate = task.EndDate
                                    })
                                };

                                await _notificationService.CreateInAppNotificationAsync(reopenNotification);

                                _notificationService.SendEmailNotification(
                                    assignee.Email!,
                                    "Công việc được mở lại",
                                    $"Xin chào {assignee.FullName},<br/><br/>" +
                                    $"Công việc <strong>{task.Title}</strong> đã được Project Manager mở lại.<br/><br/>" +
                                    $"📋 <strong>Công việc:</strong> {task.Title}<br/>" +
                                    $"📁 <strong>Dự án:</strong> {project.Name}<br/>" +
                                    $"🔄 <strong>Trạng thái cũ:</strong> {oldStatus}<br/>" +
                                    $"✅ <strong>Trạng thái mới:</strong> {request.Status}<br/>" +
                                    $"📅 <strong>Hạn chót:</strong> {task.EndDate:dd/MM/yyyy}<br/><br/>" +
                                    $"Vui lòng kiểm tra và tiếp tục thực hiện công việc này."
                                );
                            }
                        }
                    }

                    // AUTO TRACK: StartDate change
                    if (request.StartDate.HasValue && task.StartDate != request.StartDate.Value)
                    {
                        await _taskHistoryService.TrackFieldChangeAsync(
                            task.Id,
                            "StartDate",
                            oldStartDate?.ToString("dd/MM/yyyy"),
                            request.StartDate.Value.ToString("dd/MM/yyyy"),
                            request.ActorId);
                        task.StartDate = request.StartDate.Value;
                    }

                    // AUTO TRACK: EndDate change
                    if (request.EndDate.HasValue && task.EndDate != request.EndDate.Value)
                    {
                        await _taskHistoryService.TrackFieldChangeAsync(
                            task.Id,
                            "EndDate",
                            oldEndDate?.ToString("dd/MM/yyyy"),
                            request.EndDate.Value.ToString("dd/MM/yyyy"),
                            request.ActorId);
                        task.EndDate = request.EndDate.Value;

                        var currentDate = DateTime.UtcNow.Date;
                        if (request.EndDate.Value.Date > currentDate)
                        {
                            task.IsOverdue = false;
                        }
                        else if (request.EndDate.Value.Date < currentDate &&
                                 task.Status != TaskEnum.Done.ToString() &&
                                 task.Status != TaskEnum.Cancelled.ToString())
                        {
                            task.IsOverdue = true;
                        }
                    }

                    // AUTO TRACK: Assignment/Reassignment
                    if (request.UserId != oldUserId)
                    {
                        if (oldUserId.HasValue && request.UserId.HasValue)
                        {
                            // Reassignment
                            await _taskHistoryService.TrackTaskAssignmentAsync(
                                task.Id,
                                oldUserId.Value,
                                request.UserId.Value,
                                request.ActorId);
                        }
                        else if (request.UserId.HasValue)
                        {
                            // First assignment
                            await _taskHistoryService.TrackTaskAssignmentAsync(
                                task.Id,
                                null,
                                request.UserId.Value,
                                request.ActorId);
                        }
                        // Note: Không track khi unassign (UserId từ có → null)

                        task.UserId = request.UserId;
                    }

                    // Handle Reviewer changes
                    if (request.ReviewerId != oldReviewerId)
                    {
                        task.ReviewerId = request.ReviewerId;
                    }

                    task.UpdatedAt = DateTime.UtcNow;

                    // Cập nhật milestones
                    if (request.MilestoneIds != null)
                    {
                        var milestones = await _milestoneRepository.GetMilestonesByIdsAsync(request.MilestoneIds);
                        if (milestones == null || milestones.Count() != request.MilestoneIds.Length || milestones.Any(m => m.IsDeleted))
                        {
                            await transaction.RollbackAsync();
                            throw new Exception("Milestone invalid");
                        }

                        task.Milestones.Clear();
                        foreach (var milestone in milestones)
                        {
                            _milestoneRepository.Attach(milestone);
                            task.Milestones.Add(milestone);
                        }
                    }

                    await _projectTaskRepository.UpdateAsync(task);
                    await _projectTaskRepository.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Gửi notification nếu task được assign/reassign
                    if (request.UserId.HasValue && request.UserId != oldUserId && newUser != null)
                    {
                        string notificationTitle;
                        string notificationMessage;
                        string emailSubject;
                        string emailBody;

                        if (oldUserId.HasValue)
                        {
                            notificationTitle = "Task reassigned";
                            notificationMessage = $"Task '{task.Title}' has been reassigned to you in project {project.Name}";
                            emailSubject = "Task reassigned";
                            emailBody = $"Hello {newUser.FullName},<br/><br/>" +
                                       $"Task <strong>{task.Title}</strong> has been reassigned to you.<br/>" +
                                       $"Project: {project.Name}<br/>" +
                                       $"Due date: {task.EndDate:dd/MM/yyyy}<br/><br/>" +
                                       $"Please check your dashboard for more details.";
                        }
                        else
                        {
                            notificationTitle = "New task assigned";
                            notificationMessage = $"You have been assigned a new task: {task.Title} in project {project.Name}";
                            emailSubject = "New task assigned";
                            emailBody = $"Hello {newUser.FullName},<br/><br/>" +
                                       $"You have been assigned a new task: <strong>{task.Title}</strong><br/>" +
                                       $"Project: {project.Name}<br/>" +
                                       $"Due date: {task.EndDate:dd/MM/yyyy}<br/><br/>" +
                                       $"Please check your dashboard for more details.";
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
                        _notificationService.SendEmailNotification(newUser.Email!, emailSubject, emailBody);
                    }

                    // Gửi notification nếu reviewer changed
                    if (request.ReviewerId.HasValue && request.ReviewerId != oldReviewerId && newReviewer != null)
                    {
                        var reviewerNotification = new CreateNotificationRequest
                        {
                            UserId = request.ReviewerId.Value,
                            ActorId = request.ActorId,
                            Title = "You are requested to review a task",
                            Message = $"You are requested to review the task: {task.Title} in project {project.Name}",
                            Type = NotificationTypeEnum.TaskAssignment.ToString(),
                            EntityId = task.Id.ToString(),
                            Data = System.Text.Json.JsonSerializer.Serialize(new
                            {
                                TaskId = task.Id,
                                TaskTitle = task.Title,
                                ProjectId = project.Id,
                                ProjectName = project.Name,
                                DueDate = task.EndDate,
                                AssigneeId = task.UserId,
                                AssigneeName = task.User?.FullName
                            })
                        };

                        await _notificationService.CreateInAppNotificationAsync(reviewerNotification);

                        _notificationService.SendEmailNotification(
                            newReviewer.Email!,
                            "You have been requested to review a task",
                            $"Hello {newReviewer.FullName},<br/><br/>" +
                            $"You have been requested to review the following task: <strong>{task.Title}</strong><br/>" +
                            $"Project: {project.Name}<br/>" +
                            $"Assignee: {task.User?.FullName ?? "Unassigned"}<br/>" +
                            $"Deadline: {task.EndDate:dd/MM/yyyy}<br/><br/>" +
                            $"Please monitor the progress and review this task."
                        );
                    }
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            var response = new GetTaskResponse
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                UserId = task.UserId,
                ReviewerId = task.ReviewerId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                IsOverdue = task.IsOverdue,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                User = task.User == null ? null : new GetUserResponse
                {
                    Id = task.User.Id,
                    Email = task.User.Email,
                    FullName = task.User.FullName,
                    AvatarUrl = task.User.AvatarUrl,
                },
                Reviewer = task.Reviewer == null ? null : new GetUserResponse
                {
                    Id = task.Reviewer.Id,
                    Email = task.Reviewer.Email,
                    FullName = task.Reviewer.FullName,
                    AvatarUrl = task.Reviewer.AvatarUrl,
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
                ReviewerId = task.ReviewerId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                IsOverdue = task.IsOverdue,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                User = task.User == null ? null : new GetUserResponse
                {
                    Id = task.User.Id,
                    Email = task.User.Email,
                    FullName = task.User.FullName,
                    AvatarUrl = task.User.AvatarUrl,
                },
                Reviewer = task.Reviewer == null ? null : new GetUserResponse
                {
                    Id = task.Reviewer.Id,
                    Email = task.Reviewer.Email,
                    FullName = task.Reviewer.FullName,
                    AvatarUrl = task.Reviewer.AvatarUrl,
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
                ReviewerId = task.ReviewerId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                IsOverdue = task.IsOverdue,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                User = task.User == null ? null : new GetUserResponse
                {
                    Id = task.User.Id,
                    Email = task.User.Email,
                    FullName = task.User.FullName,
                    AvatarUrl = task.User.AvatarUrl,
                },
                Reviewer = task.Reviewer == null ? null : new GetUserResponse
                {
                    Id = task.Reviewer.Id,
                    Email = task.Reviewer.Email,
                    FullName = task.Reviewer.FullName,
                    AvatarUrl = task.Reviewer.AvatarUrl,
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
