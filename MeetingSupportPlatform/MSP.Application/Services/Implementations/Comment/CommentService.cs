using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MSP.Application.Models.Requests.Comment;
using MSP.Application.Models.Requests.Notification;
using MSP.Application.Models.Responses.Auth;
using MSP.Application.Models.Responses.Comment;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Comment;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Domain.Entities;
using MSP.Shared.Common;
using MSP.Shared.Enums;

namespace MSP.Application.Services.Implementations.Comment
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IProjectTaskRepository _projectTaskRepository;
        private readonly INotificationService _notificationService;
        private readonly UserManager<User> _userManager;

        public CommentService(
            ICommentRepository commentRepository,
            IProjectTaskRepository projectTaskRepository,
            INotificationService notificationService,
            UserManager<User> userManager)
        {
            _commentRepository = commentRepository;
            _projectTaskRepository = projectTaskRepository;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public async Task<ApiResponse<GetCommentResponse>> CreateCommentAsync(CreateCommentRequest request)
        {
            // Validate user exists
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return ApiResponse<GetCommentResponse>.ErrorResponse(null, "User not found");
            }

            // Validate task exists
            var task = await _projectTaskRepository.GetTaskByIdAsync(request.TaskId);
            if (task == null || task.IsDeleted)
            {
                return ApiResponse<GetCommentResponse>.ErrorResponse(null, "Task not found");
            }

            var comment = new Domain.Entities.Comment
            {
                TaskId = request.TaskId,
                UserId = request.UserId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepository.AddAsync(comment);
            await _commentRepository.SaveChangesAsync();

            // Send notification to task assignee (if different from commenter)
            if (task.UserId.HasValue && task.UserId.Value != request.UserId)
            {
                try
                {
                    var notificationRequest = new CreateNotificationRequest
                    {
                        UserId = task.UserId.Value,
                        ActorId = request.UserId,
                        Title = "New Comment on Task",
                        Message = $"{user.FullName} commented on task '{task.Title}'",
                        Type = NotificationTypeEnum.TaskUpdate.ToString(),
                        EntityId = task.Id.ToString(),
                        Data = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            TaskId = task.Id,
                            TaskTitle = task.Title,
                            task.ProjectId,
                            CommentId = comment.Id,
                            CommentContent = comment.Content,
                            CommenterName = user.FullName
                        })
                    };

                    await _notificationService.CreateInAppNotificationAsync(notificationRequest);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send comment notification: {ex.Message}");
                }
            }

            // Send notification to reviewer (if different from commenter)
            if (task.ReviewerId.HasValue && task.ReviewerId.Value != request.UserId)
            {
                try
                {
                    var reviewerNotification = new CreateNotificationRequest
                    {
                        UserId = task.ReviewerId.Value,
                        ActorId = request.UserId,
                        Title = "New Comment on Task Under Review",
                        Message = $"{user.FullName} commented on task '{task.Title}' that you are reviewing",
                        Type = NotificationTypeEnum.TaskUpdate.ToString(),
                        EntityId = task.Id.ToString(),
                        Data = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            TaskId = task.Id,
                            TaskTitle = task.Title,
                            CommentId = comment.Id,
                            CommentContent = comment.Content,
                            CommenterName = user.FullName
                        })
                    };

                    await _notificationService.CreateInAppNotificationAsync(reviewerNotification);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send reviewer comment notification: {ex.Message}");
                }
            }

            var response = new GetCommentResponse
            {
                Id = comment.Id,
                TaskId = comment.TaskId,
                UserId = comment.UserId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                User = new GetUserResponse
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    AvatarUrl = user.AvatarUrl ?? string.Empty,
                    Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? string.Empty
                }
            };

            return ApiResponse<GetCommentResponse>.SuccessResponse(response, "Comment created successfully");
        }

        public async Task<ApiResponse<GetCommentResponse>> UpdateCommentAsync(UpdateCommentRequest request)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(request.Id);
            if (comment == null || comment.IsDeleted)
            {
                return ApiResponse<GetCommentResponse>.ErrorResponse(null, "Comment not found");
            }

            comment.Content = request.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            await _commentRepository.UpdateAsync(comment);
            await _commentRepository.SaveChangesAsync();

            var response = new GetCommentResponse
            {
                Id = comment.Id,
                TaskId = comment.TaskId,
                UserId = comment.UserId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                User = comment.User == null ? null : new GetUserResponse
                {
                    Id = comment.User.Id,
                    Email = comment.User.Email ?? string.Empty,
                    FullName = comment.User.FullName,
                    AvatarUrl = comment.User.AvatarUrl ?? string.Empty,
                    Role = (await _userManager.GetRolesAsync(comment.User)).FirstOrDefault() ?? string.Empty
                }
            };

            return ApiResponse<GetCommentResponse>.SuccessResponse(response, "Comment updated successfully");
        }

        public async Task<ApiResponse<string>> DeleteCommentAsync(Guid commentId, Guid userId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null || comment.IsDeleted)
            {
                return ApiResponse<string>.ErrorResponse(null, "Comment not found");
            }

            // Only comment owner can delete
            if (comment.UserId != userId)
            {
                return ApiResponse<string>.ErrorResponse(null, "You don't have permission to delete this comment");
            }

            await _commentRepository.SoftDeleteAsync(comment);
            await _commentRepository.SaveChangesAsync();

            return ApiResponse<string>.SuccessResponse("Comment deleted successfully");
        }

        public async Task<ApiResponse<GetCommentResponse>> GetCommentByIdAsync(Guid commentId)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(commentId);
            if (comment == null)
            {
                return ApiResponse<GetCommentResponse>.ErrorResponse(null, "Comment not found");
            }

            var response = new GetCommentResponse
            {
                Id = comment.Id,
                TaskId = comment.TaskId,
                UserId = comment.UserId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                User = comment.User == null ? null : new GetUserResponse
                {
                    Id = comment.User.Id,
                    Email = comment.User.Email ?? string.Empty,
                    FullName = comment.User.FullName,
                    AvatarUrl = comment.User.AvatarUrl ?? string.Empty,
                    Role = (await _userManager.GetRolesAsync(comment.User)).FirstOrDefault() ?? string.Empty
                }
            };

            return ApiResponse<GetCommentResponse>.SuccessResponse(response, "Comment retrieved successfully");
        }

        public async Task<ApiResponse<PagingResponse<GetCommentResponse>>> GetCommentsByTaskIdAsync(PagingRequest request, Guid taskId)
        {
            // Validate task exists
            var task = await _projectTaskRepository.GetByIdAsync(taskId);
            if (task == null || task.IsDeleted)
            {
                return ApiResponse<PagingResponse<GetCommentResponse>>.ErrorResponse(null, "Task not found");
            }

            var comments = await _commentRepository.FindWithIncludePagedAsync(
                predicate: c => c.TaskId == taskId && !c.IsDeleted,
                include: query => query.Include(c => c.User),
                orderBy: query => query.OrderByDescending(c => c.CreatedAt),
                pageNumber: request.PageIndex,
                pageSize: request.PageSize,
                asNoTracking: true
            );

            if (comments == null || !comments.Any())
            {
                return ApiResponse<PagingResponse<GetCommentResponse>>.ErrorResponse(null, "No comments found for this task");
            }

            var response = new List<GetCommentResponse>();

            foreach (var comment in comments)
            {
                response.Add(new GetCommentResponse
                {
                    Id = comment.Id,
                    TaskId = comment.TaskId,
                    UserId = comment.UserId,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt,
                    UpdatedAt = comment.UpdatedAt,
                    User = comment.User == null ? null : new GetUserResponse
                    {
                        Id = comment.User.Id,
                        Email = comment.User.Email ?? string.Empty,
                        FullName = comment.User.FullName,
                        AvatarUrl = comment.User.AvatarUrl ?? string.Empty,
                        Role = (await _userManager.GetRolesAsync(comment.User)).FirstOrDefault() ?? string.Empty
                    }
                });
            }

            var totalCount = await _commentRepository.CountAsync(c => c.TaskId == taskId && !c.IsDeleted);

            var pagingResponse = new PagingResponse<GetCommentResponse>
            {
                Items = response,
                TotalItems = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };

            return ApiResponse<PagingResponse<GetCommentResponse>>.SuccessResponse(pagingResponse);
        }

        public async Task<ApiResponse<int>> GetCommentCountByTaskIdAsync(Guid taskId)
        {
            // Validate task exists
            var task = await _projectTaskRepository.GetByIdAsync(taskId);
            if (task == null || task.IsDeleted)
            {
                return ApiResponse<int>.ErrorResponse(0, "Task not found");
            }

            var count = await _commentRepository.CountCommentsByTaskIdAsync(taskId);
            return ApiResponse<int>.SuccessResponse(count, "Comment count retrieved successfully");
        }
    }
}
