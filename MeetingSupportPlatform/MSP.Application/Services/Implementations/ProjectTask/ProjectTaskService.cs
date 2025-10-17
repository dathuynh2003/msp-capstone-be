using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MSP.Application.Models.Requests.ProjectTask;
using MSP.Application.Models.Responses.Auth;
using MSP.Application.Models.Responses.Milestone;
using MSP.Application.Models.Responses.ProjectTask;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.ProjectTask;
using MSP.Domain.Entities;
using MSP.Shared.Common;

namespace MSP.Application.Services.Implementations.ProjectTask
{
    public class ProjectTaskService : IProjectTaskService
    {
        private readonly IProjectTaskRepository _projectTaskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IMilestoneRepository _milestoneRepository;
        private readonly UserManager<User> _userManager;

        public ProjectTaskService(IProjectTaskRepository projectTaskRepository, IProjectRepository projectRepository, IMilestoneRepository milestoneRepository, UserManager<User> userManager)
        {
            _projectTaskRepository = projectTaskRepository;
            _projectRepository = projectRepository;
            _milestoneRepository = milestoneRepository;
            _userManager = userManager;
        }

        public async Task<ApiResponse<GetTaskResponse>> CreateTaskAsync(CreateTaskRequest request)
        {
            var project = await _projectRepository.GetByIdAsync(request.ProjectId);
            if (project == null)
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
                if (milestones == null || !milestones.Any())
                {
                    return ApiResponse<GetTaskResponse>.ErrorResponse(null, "Some milestones not found");
                }
                newTask.Milestones = milestones.ToList();
            }

            await _projectTaskRepository.AddAsync(newTask);
            await _projectTaskRepository.SaveChangesAsync();

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
                }).ToArray()
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
            if (task == null)
            {
                return ApiResponse<GetTaskResponse>.ErrorResponse(null, "Task not found");
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
                if (milestones == null || milestones.Count() != request.MilestoneIds.Length)
                {
                    return ApiResponse<GetTaskResponse>.ErrorResponse(null, "Some milestones not found");
                }

                // Xóa các milestones cũ
                task.Milestones.Clear();
                // Thêm milestones mới
                foreach (var milestone in milestones)
                {
                    task.Milestones.Add(milestone);
                }
            }

            await _projectTaskRepository.UpdateAsync(task);
            await _projectTaskRepository.SaveChangesAsync();

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
    }
}
