using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MSP.Application.Models.Requests.Notification;
using MSP.Application.Models.Requests.Project;
using MSP.Application.Models.Responses.Auth;
using MSP.Application.Models.Responses.Project;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Services.Interfaces.Project;
using MSP.Domain.Entities;
using MSP.Shared.Common;
using MSP.Shared.Enums;

namespace MSP.Application.Services.Implementations.Project
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly IProjectTaskRepository _projectTaskRepository;
        private readonly INotificationService _notificationService;
        private readonly UserManager<User> _userManager;

        public ProjectService(
            IProjectRepository projectRepository,
            IProjectMemberRepository projectMemberRepository,
            IProjectTaskRepository projectTaskRepository,
            INotificationService notificationService,
            UserManager<User> userManager)
        {
            _projectRepository = projectRepository;
            _projectMemberRepository = projectMemberRepository;
            _projectTaskRepository = projectTaskRepository;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public async Task<ApiResponse<GetProjectMemberResponse>> AddProjectMemberAsync(AddProjectMemeberRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return ApiResponse<GetProjectMemberResponse>.ErrorResponse(null, "User not found");

            var project = await _projectRepository.GetByIdAsync(request.ProjectId);
            if (project == null)
            {
                return ApiResponse<GetProjectMemberResponse>.ErrorResponse(null, "Project not found");
            }

            var projectMember = new ProjectMember
            {
                ProjectId = request.ProjectId,
                MemberId = request.UserId,
                JoinedAt = DateTime.UtcNow
            };

            _ = await _projectMemberRepository.AddAsync(projectMember);

            var response = new GetProjectMemberResponse
            {
                Id = projectMember.Id,
                ProjectId = projectMember.ProjectId,
                UserId = projectMember.MemberId,
                JoinedAt = projectMember.JoinedAt,
            };

            await _projectMemberRepository.SaveChangesAsync();
            return ApiResponse<GetProjectMemberResponse>.SuccessResponse(response);
        }

        public async Task<ApiResponse<GetProjectResponse>> CreateProjectAsync(CreateProjectRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.CreatedById.ToString());
            if (user == null)
                return ApiResponse<GetProjectResponse>.ErrorResponse(null, "User not found");

            if (!user.ManagedById.HasValue)
                return ApiResponse<GetProjectResponse>.ErrorResponse(null, "User does not have a Business Owner assigned");

            var project = new Domain.Entities.Project
            {
                Name = request.Name,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = request.Status,
                CreatedById = request.CreatedById,
                OwnerId = user.ManagedById.Value,
                CreatedAt = DateTime.UtcNow
            };

            _ = await _projectRepository.AddAsync(project);
            await _projectRepository.SaveChangesAsync();

            var projectMember = new ProjectMember
            {
                ProjectId = project.Id,
                MemberId = request.CreatedById,
                JoinedAt = DateTime.UtcNow
            };

            await _projectMemberRepository.AddAsync(projectMember);
            await _projectMemberRepository.SaveChangesAsync();

            var response = new GetProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                OwnerId = project.OwnerId,
                CreatedById = project.CreatedById,
                CreatedAt = project.CreatedAt,
                Status = project.Status
            };

            return ApiResponse<GetProjectResponse>.SuccessResponse(response);
        }

        public async Task<ApiResponse<string>> DeleteProjectAsync(Guid projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                return ApiResponse<string>.ErrorResponse(null, "Project not found");
            }

            await _projectRepository.SoftDeleteAsync(project);
            await _projectRepository.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse("Project deleted successfully");
        }

        public async Task<ApiResponse<PagingResponse<GetProjectResponse>>> GetAllProjectsAsync(PagingRequest request)
        {
            var projects = await _projectRepository.FindWithIncludePagedAsync(
                predicate: p => !p.IsDeleted,
                include: query => query
                    .Include(p => p.Owner)
                    .Include(p => p.CreatedBy),
                pageNumber: request.PageIndex,
                pageSize: request.PageSize,
                asNoTracking: true);

            if (projects == null || !projects.Any())
            {
                return ApiResponse<PagingResponse<GetProjectResponse>>.ErrorResponse(null, "No projects found");
            }

            var response = new List<GetProjectResponse>();

            foreach (var project in projects)
            {
                var ownerRole = (await _userManager.GetRolesAsync(project.Owner)).FirstOrDefault() ?? string.Empty;
                var createdByRole = (await _userManager.GetRolesAsync(project.CreatedBy)).FirstOrDefault() ?? string.Empty;

                response.Add(new GetProjectResponse
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    OwnerId = project.OwnerId,
                    CreatedById = project.CreatedById,
                    Status = project.Status,
                    Owner = new GetUserResponse
                    {
                        Id = project.Owner.Id,
                        FullName = project.Owner.FullName,
                        Email = project.Owner.Email,
                        AvatarUrl = project.Owner.AvatarUrl,
                        Role = ownerRole
                    },
                    CreatedBy = new GetUserResponse
                    {
                        Id = project.CreatedBy.Id,
                        FullName = project.CreatedBy.FullName,
                        Email = project.CreatedBy.Email,
                        AvatarUrl = project.CreatedBy.AvatarUrl,
                        Role = createdByRole
                    },
                    CreatedAt = project.CreatedAt,
                    UpdatedAt = project.UpdatedAt
                });
            }

            if (response.Count == 0)
            {
                return ApiResponse<PagingResponse<GetProjectResponse>>.ErrorResponse(null, "No results found. Try adjusting your search criteria.");
            }

            var pagingResponse = new PagingResponse<GetProjectResponse>
            {
                Items = response,
                TotalItems = await _projectRepository.CountAsync(p => !p.IsDeleted),
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };

            return ApiResponse<PagingResponse<GetProjectResponse>>.SuccessResponse(pagingResponse);
        }

        public async Task<ApiResponse<GetProjectResponse>> GetProjectByIdAsync(Guid projectId)
        {
            var project = await _projectRepository.GetProjectByIdAsync(projectId);

            if (project == null)
            {
                return ApiResponse<GetProjectResponse>.ErrorResponse(null, "Project not found");
            }

            var response = new GetProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                OwnerId = project.OwnerId,
                CreatedById = project.CreatedById,
                Status = project.Status,
                Owner = new GetUserResponse
                {
                    Id = project.Owner.Id,
                    FullName = project.Owner.FullName,
                    Email = project.Owner.Email,
                    AvatarUrl = project.Owner.AvatarUrl,
                    Role = (await _userManager.GetRolesAsync(project.Owner)).FirstOrDefault() ?? string.Empty
                },
                CreatedBy = new GetUserResponse
                {
                    Id = project.CreatedBy.Id,
                    FullName = project.CreatedBy.FullName,
                    Email = project.CreatedBy.Email,
                    AvatarUrl = project.CreatedBy.AvatarUrl,
                    Role = (await _userManager.GetRolesAsync(project.CreatedBy)).FirstOrDefault() ?? string.Empty
                },
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            };

            return ApiResponse<GetProjectResponse>.SuccessResponse(response);
        }

        public async Task<ApiResponse<PagingResponse<GetProjectResponse>>> GetProjectsByBOIdAsync(PagingRequest request, Guid boId)
        {
            var user = await _userManager.FindByIdAsync(boId.ToString());
            if (user == null)
                return ApiResponse<PagingResponse<GetProjectResponse>>.ErrorResponse(null, "User not found");
            var projects = await _projectRepository.FindWithIncludePagedAsync(
                predicate: p => p.OwnerId == boId && !p.IsDeleted,
                include: query => query
                    .Include(p => p.Owner)
                    .Include(p => p.CreatedBy),
                pageNumber: request.PageIndex,
                pageSize: request.PageSize,
                asNoTracking: true);

            if (projects == null || !projects.Any())
            {
                return ApiResponse<PagingResponse<GetProjectResponse>>.ErrorResponse(null, "No projects found for the specified Business Owner");
            }

            var response = new List<GetProjectResponse>();

            foreach (var project in projects)
            {
                var ownerRole = (await _userManager.GetRolesAsync(project.Owner)).FirstOrDefault() ?? string.Empty;
                var createdByRole = (await _userManager.GetRolesAsync(project.CreatedBy)).FirstOrDefault() ?? string.Empty;

                response.Add(new GetProjectResponse
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    OwnerId = project.OwnerId,
                    CreatedById = project.CreatedById,
                    Status = project.Status,
                    Owner = new GetUserResponse
                    {
                        Id = project.Owner.Id,
                        FullName = project.Owner.FullName,
                        Email = project.Owner.Email,
                        AvatarUrl = project.Owner.AvatarUrl,
                        Role = ownerRole
                    },
                    CreatedBy = new GetUserResponse
                    {
                        Id = project.CreatedBy.Id,
                        FullName = project.CreatedBy.FullName,
                        Email = project.CreatedBy.Email,
                        AvatarUrl = project.CreatedBy.AvatarUrl,
                        Role = createdByRole
                    },
                    CreatedAt = project.CreatedAt,
                    UpdatedAt = project.UpdatedAt
                });
            }

            if (response.Count == 0)
            {
                return ApiResponse<PagingResponse<GetProjectResponse>>.ErrorResponse(null, "No results found. Try adjusting your search criteria.");
            }

            var pagingResponse = new PagingResponse<GetProjectResponse>
            {
                Items = response,
                TotalItems = await _projectRepository.CountAsync(p => p.OwnerId == boId && !p.IsDeleted),
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };

            return ApiResponse<PagingResponse<GetProjectResponse>>.SuccessResponse(pagingResponse);
        }

        public async Task<ApiResponse<PagingResponse<GetProjectResponse>>> GetProjectsByManagerIdAsync(PagingRequest request, Guid managerId)
        {
            var user = await _userManager.FindByIdAsync(managerId.ToString());
            if (user == null)
                return ApiResponse<PagingResponse<GetProjectResponse>>.ErrorResponse(null, "User not found");
            // Lấy danh sách ProjectMember của member
            var projectMembers = await _projectMemberRepository.FindWithIncludePagedAsync(
                predicate: pm => pm.MemberId == managerId,
                include: query => query.Include(pm => pm.Project).ThenInclude(p => p.Owner)
                                       .Include(pm => pm.Project).ThenInclude(p => p.CreatedBy),
                pageNumber: request.PageIndex,
                pageSize: request.PageSize,
                asNoTracking: true);

            if (projectMembers == null || !projectMembers.Any())
            {
                return ApiResponse<PagingResponse<GetProjectResponse>>.ErrorResponse(null, "No projects found for the specified manager");
            }
            var response = new List<GetProjectResponse>();

            foreach (var pm in projectMembers)
            {
                var project = pm.Project;

                if (project == null || project.IsDeleted)
                    continue;

                var ownerRole = (await _userManager.GetRolesAsync(project.Owner)).FirstOrDefault() ?? string.Empty;
                var createdByRole = (await _userManager.GetRolesAsync(project.CreatedBy)).FirstOrDefault() ?? string.Empty;

                response.Add(new GetProjectResponse
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    OwnerId = project.OwnerId,
                    CreatedById = project.CreatedById,
                    Status = project.Status,
                    Owner = new GetUserResponse
                    {
                        Id = project.Owner.Id,
                        FullName = project.Owner.FullName,
                        Email = project.Owner.Email,
                        AvatarUrl = project.Owner.AvatarUrl,
                        Role = ownerRole
                    },
                    CreatedBy = new GetUserResponse
                    {
                        Id = project.CreatedBy.Id,
                        FullName = project.CreatedBy.FullName,
                        Email = project.CreatedBy.Email,
                        AvatarUrl = project.CreatedBy.AvatarUrl,
                        Role = createdByRole
                    },
                    CreatedAt = project.CreatedAt,
                    UpdatedAt = project.UpdatedAt
                });
            }

            if (response.Count == 0)
            {
                return ApiResponse<PagingResponse<GetProjectResponse>>.ErrorResponse(null, "No results found. Try adjusting your search criteria.");
            }

            var totalItems = await _projectMemberRepository.CountAsync(pm => pm.MemberId == managerId && !pm.Project.IsDeleted);

            var pagingResponse = new PagingResponse<GetProjectResponse>
            {
                Items = response,
                TotalItems = totalItems,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };

            return ApiResponse<PagingResponse<GetProjectResponse>>.SuccessResponse(pagingResponse);
        }

        public async Task<ApiResponse<PagingResponse<GetProjectResponse>>> GetProjectsByMemberIdAsync(PagingRequest request, Guid memberId)
        {
            var user = await _userManager.FindByIdAsync(memberId.ToString());
            if (user == null)
                return ApiResponse<PagingResponse<GetProjectResponse>>.ErrorResponse(null, "User not found");

            // Lấy danh sách ProjectMember của member
            var projectMembers = await _projectMemberRepository.FindWithIncludePagedAsync(
                predicate: pm => pm.MemberId == memberId,
                include: query => query.Include(pm => pm.Project).ThenInclude(p => p.Owner)
                                       .Include(pm => pm.Project).ThenInclude(p => p.CreatedBy),
                pageNumber: request.PageIndex,
                pageSize: request.PageSize,
                asNoTracking: true);

            if (projectMembers == null || !projectMembers.Any())
            {
                return ApiResponse<PagingResponse<GetProjectResponse>>.ErrorResponse(null, "No projects found for the specified member");
            }

            var response = new List<GetProjectResponse>();

            foreach (var pm in projectMembers)
            {
                var project = pm.Project;

                if (project == null || project.IsDeleted)
                    continue;

                var ownerRole = (await _userManager.GetRolesAsync(project.Owner)).FirstOrDefault() ?? string.Empty;
                var createdByRole = (await _userManager.GetRolesAsync(project.CreatedBy)).FirstOrDefault() ?? string.Empty;

                response.Add(new GetProjectResponse
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    OwnerId = project.OwnerId,
                    CreatedById = project.CreatedById,
                    Status = project.Status,
                    Owner = new GetUserResponse
                    {
                        Id = project.Owner.Id,
                        FullName = project.Owner.FullName,
                        Email = project.Owner.Email,
                        AvatarUrl = project.Owner.AvatarUrl,
                        Role = ownerRole
                    },
                    CreatedBy = new GetUserResponse
                    {
                        Id = project.CreatedBy.Id,
                        FullName = project.CreatedBy.FullName,
                        Email = project.CreatedBy.Email,
                        AvatarUrl = project.CreatedBy.AvatarUrl,
                        Role = createdByRole
                    },
                    CreatedAt = project.CreatedAt,
                    UpdatedAt = project.UpdatedAt
                });
            }

            var totalItems = await _projectMemberRepository.CountAsync(pm => pm.MemberId == memberId && !pm.Project.IsDeleted);

            var pagingResponse = new PagingResponse<GetProjectResponse>
            {
                Items = response,
                TotalItems = totalItems,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };

            return ApiResponse<PagingResponse<GetProjectResponse>>.SuccessResponse(pagingResponse);
        }


        public async Task<ApiResponse<string>> RemoveProjectMemberAsync(Guid pmId)
        {
            var projectMember = await _projectMemberRepository.GetByIdAsync(pmId);
            if (projectMember == null)
            {
                return ApiResponse<string>.ErrorResponse(null, "Project member not found");
            }

            await _projectMemberRepository.HardDeleteAsync(projectMember);
            await _projectMemberRepository.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse("Project member removed successfully");

        }

        public async Task<ApiResponse<GetProjectResponse>> UpdateProjectAsync(UpdateProjectRequest request)
        {
            var project = await _projectRepository.GetProjectByIdAsync(request.Id);
            if (project == null)
            {
                return ApiResponse<GetProjectResponse>.ErrorResponse(null, "Project not found");
            }

            var oldStatus = project.Status;
            
            project.Name = request.Name;
            project.Description = request.Description;
            project.StartDate = request.StartDate;
            project.EndDate = request.EndDate;
            project.Status = request.Status;
            project.UpdatedAt = DateTime.UtcNow;

            await _projectRepository.UpdateAsync(project);

            // TRIGGER: Cancel all active tasks (except Done) when project status changes to Completed or Cancelled
            if (oldStatus != request.Status && 
                (request.Status == ProjectStatusEnum.Completed.ToString() || 
                 request.Status == ProjectStatusEnum.Cancelled.ToString()))
            {
                try
                {
                    // Get all tasks of the project
                    var projectTasks = await _projectTaskRepository.GetTasksByProjectIdAsync(request.Id);
                    
                    // Filter tasks that are not Done or already Cancelled
                    var tasksToCancel = projectTasks
                        .Where(t => t.Status != TaskEnum.Done.ToString() && 
                                   t.Status != TaskEnum.Cancelled.ToString())
                        .ToList();

                    if (tasksToCancel.Any())
                    {
                        foreach (var task in tasksToCancel)
                        {
                            task.Status = TaskEnum.Cancelled.ToString();
                            task.UpdatedAt = DateTime.UtcNow;
                            await _projectTaskRepository.UpdateAsync(task);

                            // Send notification to assigned user if exists
                            if (task.UserId.HasValue)
                            {
                                var taskNotification = new CreateNotificationRequest
                                {
                                    UserId = task.UserId.Value,
                                    Title = request.Status == ProjectStatusEnum.Completed.ToString() 
                                        ? "Công việc bị hủy do dự án hoàn thành"
                                        : "Công việc bị hủy do dự án bị hủy",
                                    Message = $"Công việc '{task.Title}' đã bị hủy do dự án '{project.Name}' {(request.Status == ProjectStatusEnum.Completed.ToString() ? "đã hoàn thành" : "đã bị hủy")}.",
                                    Type = NotificationTypeEnum.TaskUpdate.ToString(),
                                    EntityId = task.Id.ToString(),
                                    Data = System.Text.Json.JsonSerializer.Serialize(new
                                    {
                                        TaskId = task.Id,
                                        TaskTitle = task.Title,
                                        ProjectId = project.Id,
                                        ProjectName = project.Name,
                                        NewStatus = TaskEnum.Cancelled.ToString(),
                                        Reason = request.Status == ProjectStatusEnum.Completed.ToString() 
                                            ? "ProjectCompleted" 
                                            : "ProjectCancelled"
                                    })
                                };

                                await _notificationService.CreateInAppNotificationAsync(taskNotification);
                            }

                            // Send notification to reviewer if exists
                            if (task.ReviewerId.HasValue)
                            {
                                var reviewerNotification = new CreateNotificationRequest
                                {
                                    UserId = task.ReviewerId.Value,
                                    Title = request.Status == ProjectStatusEnum.Completed.ToString() 
                                        ? "Công việc review bị hủy do dự án hoàn thành"
                                        : "Công việc review bị hủy do dự án bị hủy",
                                    Message = $"Công việc '{task.Title}' mà bạn đang review đã bị hủy do dự án '{project.Name}' {(request.Status == ProjectStatusEnum.Completed.ToString() ? "đã hoàn thành" : "đã bị hủy")}.",
                                    Type = NotificationTypeEnum.TaskUpdate.ToString(),
                                    EntityId = task.Id.ToString(),
                                    Data = System.Text.Json.JsonSerializer.Serialize(new
                                    {
                                        TaskId = task.Id,
                                        TaskTitle = task.Title,
                                        ProjectId = project.Id,
                                        ProjectName = project.Name,
                                        NewStatus = TaskEnum.Cancelled.ToString(),
                                        Reason = request.Status == ProjectStatusEnum.Completed.ToString() 
                                            ? "ProjectCompleted" 
                                            : "ProjectCancelled"
                                    })
                                };

                                await _notificationService.CreateInAppNotificationAsync(reviewerNotification);
                            }
                        }

                        await _projectTaskRepository.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    // Log but don't fail the update operation
                    Console.WriteLine($"Failed to cancel tasks for project {request.Id}: {ex.Message}");
                }
            }

            // Check if project status changed to Completed
            if (oldStatus != ProjectStatusEnum.Completed.ToString() && 
                request.Status == ProjectStatusEnum.Completed.ToString())
            {
                try
                {
                    // Send notification to Project Owner
                    var owner = project.Owner;
                    if (owner != null)
                    {
                        var ownerNotification = new CreateNotificationRequest
                        {
                            UserId = owner.Id,
                            Title = "🎉 Dự án hoàn thành",
                            Message = $"Dự án '{project.Name}' đã được đánh dấu là hoàn thành. Làm tốt lắm!",
                            Type = NotificationTypeEnum.ProjectUpdate.ToString(),
                            EntityId = project.Id.ToString(),
                            Data = System.Text.Json.JsonSerializer.Serialize(new
                            {
                                ProjectId = project.Id,
                                ProjectName = project.Name,
                                CompletedAt = DateTime.UtcNow,
                                EventType = "ProjectCompleted"
                            })
                        };

                        await _notificationService.CreateInAppNotificationAsync(ownerNotification);

                        // Send email to owner
                        _notificationService.SendEmailNotification(
                            owner.Email!,
                            "Dự án hoàn thành",
                            $"Xin chào {owner.FullName},<br/><br/>" +
                            $"Chúc mừng! Dự án <strong>{project.Name}</strong> đã được hoàn thành thành công.<br/><br/>" +
                            $"<strong>Ngày bắt đầu:</strong> {project.StartDate:dd/MM/yyyy}<br/>" +
                            $"<strong>Ngày kết thúc:</strong> {project.EndDate:dd/MM/yyyy}<br/>" +
                            $"<strong>Hoàn thành vào:</strong> {DateTime.UtcNow:dd/MM/yyyy}<br/><br/>" +
                            $"Cảm ơn bạn đã lãnh đạo và giám sát dự án này.");
                    }

                    // Send notifications to all active Project Members
                    if (project.ProjectMembers?.Any() == true)
                    {
                        var activeMembers = project.ProjectMembers
                            .Where(pm => !pm.LeftAt.HasValue) // Only active members
                            .ToList();

                        foreach (var projectMember in activeMembers)
                        {
                            try
                            {
                                var memberNotification = new CreateNotificationRequest
                                {
                                    UserId = projectMember.MemberId,
                                    Title = "🎉 Dự án hoàn thành",
                                    Message = $"Dự án '{project.Name}' đã hoàn thành. Cảm ơn sự đóng góp của bạn!",
                                    Type = NotificationTypeEnum.ProjectUpdate.ToString(),
                                    EntityId = project.Id.ToString(),
                                    Data = System.Text.Json.JsonSerializer.Serialize(new
                                    {
                                        ProjectId = project.Id,
                                        ProjectName = project.Name,
                                        CompletedAt = DateTime.UtcNow,
                                        EventType = "ProjectCompleted"
                                    })
                                };

                                await _notificationService.CreateInAppNotificationAsync(memberNotification);

                                // Send email to member
                                var member = projectMember.Member;
                                if (member != null)
                                {
                                    _notificationService.SendEmailNotification(
                                        member.Email!,
                                        "Dự án hoàn thành",
                                        $"Xin chào {member.FullName},<br/><br/>" +
                                        $"Tin tuyệt vời! Dự án <strong>{project.Name}</strong> đã được hoàn thành thành công.<br/><br/>" +
                                        $"Cảm ơn sự chăm chỉ và cống hiến của bạn cho dự án này. Đóng góp của bạn rất quan trọng!");
                                }
                            }
                            catch (Exception ex)
                            {
                                // Log but continue processing other members
                                Console.WriteLine($"Failed to send notification to member {projectMember.MemberId}: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log but don't fail the update operation
                    Console.WriteLine($"Failed to send completion notifications: {ex.Message}");
                }
            }

            // Check if project status changed to Cancelled
            if (oldStatus != ProjectStatusEnum.Cancelled.ToString() && 
                request.Status == ProjectStatusEnum.Cancelled.ToString())
            {
                try
                {
                    // Send notification to Project Owner
                    var owner = project.Owner;
                    if (owner != null)
                    {
                        var ownerNotification = new CreateNotificationRequest
                        {
                            UserId = owner.Id,
                            Title = "⚠️ Dự án đã bị hủy",
                            Message = $"Dự án '{project.Name}' đã được đánh dấu là hủy bỏ.",
                            Type = NotificationTypeEnum.ProjectUpdate.ToString(),
                            EntityId = project.Id.ToString(),
                            Data = System.Text.Json.JsonSerializer.Serialize(new
                            {
                                ProjectId = project.Id,
                                ProjectName = project.Name,
                                CancelledAt = DateTime.UtcNow,
                                EventType = "ProjectCancelled"
                            })
                        };

                        await _notificationService.CreateInAppNotificationAsync(ownerNotification);

                        // Send email to owner
                        _notificationService.SendEmailNotification(
                            owner.Email!,
                            "Dự án đã bị hủy",
                            $"Xin chào {owner.FullName},<br/><br/>" +
                            $"Dự án <strong>{project.Name}</strong> đã được đánh dấu là hủy bỏ.<br/><br/>" +
                            $"<strong>Ngày bắt đầu:</strong> {project.StartDate:dd/MM/yyyy}<br/>" +
                            $"<strong>Ngày kết thúc dự kiến:</strong> {project.EndDate:dd/MM/yyyy}<br/>" +
                            $"<strong>Hủy bỏ vào:</strong> {DateTime.UtcNow:dd/MM/yyyy}<br/><br/>" +
                            $"Tất cả công việc chưa hoàn thành đã được tự động hủy.");
                    }

                    // Send notifications to all active Project Members
                    if (project.ProjectMembers?.Any() == true)
                    {
                        var activeMembers = project.ProjectMembers
                            .Where(pm => !pm.LeftAt.HasValue) // Only active members
                            .ToList();

                        foreach (var projectMember in activeMembers)
                        {
                            try
                            {
                                var memberNotification = new CreateNotificationRequest
                                {
                                    UserId = projectMember.MemberId,
                                    Title = "⚠️ Dự án đã bị hủy",
                                    Message = $"Dự án '{project.Name}' đã bị hủy. Tất cả công việc chưa hoàn thành đã được tự động hủy.",
                                    Type = NotificationTypeEnum.ProjectUpdate.ToString(),
                                    EntityId = project.Id.ToString(),
                                    Data = System.Text.Json.JsonSerializer.Serialize(new
                                    {
                                        ProjectId = project.Id,
                                        ProjectName = project.Name,
                                        CancelledAt = DateTime.UtcNow,
                                        EventType = "ProjectCancelled"
                                    })
                                };

                                await _notificationService.CreateInAppNotificationAsync(memberNotification);

                                // Send email to member
                                var member = projectMember.Member;
                                if (member != null)
                                {
                                    _notificationService.SendEmailNotification(
                                        member.Email!,
                                        "Dự án đã bị hủy",
                                        $"Xin chào {member.FullName},<br/><br/>" +
                                        $"Dự án <strong>{project.Name}</strong> đã được đánh dấu là hủy bỏ.<br/><br/>" +
                                        $"Tất cả công việc chưa hoàn thành của bạn trong dự án này đã được tự động hủy.");
                                }
                            }
                            catch (Exception ex)
                            {
                                // Log but continue processing other members
                                Console.WriteLine($"Failed to send notification to member {projectMember.MemberId}: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log but don't fail the update operation
                    Console.WriteLine($"Failed to send cancellation notifications: {ex.Message}");
                }
            }

            var response = new GetProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                OwnerId = project.OwnerId,
                CreatedById = project.CreatedById,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                Status = project.Status
            };

            await _projectRepository.SaveChangesAsync();
            return ApiResponse<GetProjectResponse>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<GetProjectMemberResponse>>> GetProjectMembersAsync(Guid projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                return ApiResponse<List<GetProjectMemberResponse>>.ErrorResponse(null, "Project not found");
            }

            var projectMembers = await _projectMemberRepository.GetProjectMembersByProjectIdAsync(projectId);
            if (projectMembers == null || !projectMembers.Any())
            {
                return ApiResponse<List<GetProjectMemberResponse>>.ErrorResponse(null, "No members found for the specified project");
            }

            var response = new List<GetProjectMemberResponse>();

            foreach (var pm in projectMembers)
            {
                var roles = await _userManager.GetRolesAsync(pm.Member);
                var role = roles.FirstOrDefault() ?? string.Empty;

                response.Add(new GetProjectMemberResponse
                {
                    Id = pm.Id,
                    ProjectId = pm.ProjectId,
                    UserId = pm.MemberId,
                    JoinedAt = pm.JoinedAt,
                    Member = new GetUserResponse
                    {
                        Id = pm.Member.Id,
                        FullName = pm.Member.FullName,
                        Email = pm.Member.Email,
                        AvatarUrl = pm.Member.AvatarUrl,
                        Role = role
                    }
                });
            }

            return ApiResponse<List<GetProjectMemberResponse>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<GetProjectMemberResponse>>> GetProjectMembersByRoleAsync(Guid projectId, string role)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                return ApiResponse<List<GetProjectMemberResponse>>.ErrorResponse(null, "Project not found");
            }

            var projectMembers = await _projectMemberRepository.GetProjectMembersByProjectIdAsync(projectId);
            if (projectMembers == null || !projectMembers.Any())
            {
                return ApiResponse<List<GetProjectMemberResponse>>.ErrorResponse(null, "No members found for the specified project");
            }

            var response = new List<GetProjectMemberResponse>();

            foreach (var pm in projectMembers)
            {
                var roles = await _userManager.GetRolesAsync(pm.Member);
                var userRole = roles.FirstOrDefault() ?? string.Empty;

                // Filter by role
                if (!string.IsNullOrEmpty(role) && !userRole.Equals(role, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                response.Add(new GetProjectMemberResponse
                {
                    Id = pm.Id,
                    ProjectId = pm.ProjectId,
                    UserId = pm.MemberId,
                    JoinedAt = pm.JoinedAt,
                    Member = new GetUserResponse
                    {
                        Id = pm.Member.Id,
                        FullName = pm.Member.FullName,
                        Email = pm.Member.Email,
                        AvatarUrl = pm.Member.AvatarUrl,
                        Role = userRole
                    }
                });
            }

            if (response.Count == 0)
            {
                return ApiResponse<List<GetProjectMemberResponse>>.ErrorResponse(null, $"No {role}s found for the specified project");
            }

            return ApiResponse<List<GetProjectMemberResponse>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<GetProjectMemberResponse>>> GetProjectManagersAsync(Guid projectId)
        {
            return await GetProjectMembersByRoleAsync(projectId, "ProjectManager");
        }

    }
}
