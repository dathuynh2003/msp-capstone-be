using Microsoft.AspNetCore.Identity;
using MSP.Application.Models.Requests.Milestone;
using MSP.Application.Models.Responses.Milestone;
using MSP.Application.Models.Responses.Project;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Milestone;
using MSP.Domain.Entities;
using MSP.Shared.Common;

namespace MSP.Application.Services.Implementations.Milestone
{
    public class MilestoneService : IMilestoneService
    {
        private readonly IMilestoneRepository _milestoneRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectTaskRepository _projectTaskRepository;
        private readonly UserManager<User> _userManager;

        public MilestoneService(IMilestoneRepository milestoneRepository, IProjectRepository projectRepository, UserManager<User> userManager, IProjectTaskRepository projectTaskRepository)
        {
            _milestoneRepository = milestoneRepository;
            _projectRepository = projectRepository;
            _userManager = userManager;
            _projectTaskRepository = projectTaskRepository;
        }

        public async Task<ApiResponse<GetMilestoneResponse>> CreateMilestoneAsync(CreateMilestoneRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return ApiResponse<GetMilestoneResponse>.ErrorResponse(null, "User not found");

            var project = await _projectRepository.GetByIdAsync(request.ProjectId);

            if (project == null || project.IsDeleted)
            {
                return ApiResponse<GetMilestoneResponse>.ErrorResponse(null, "Project not found");
            }

            var milestone = new Domain.Entities.Milestone
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                ProjectId = request.ProjectId,
                Name = request.Name,
                DueDate = request.DueDate,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _milestoneRepository.AddAsync(milestone);

            var response = new GetMilestoneResponse
            {
                Id = milestone.Id,
                ProjectId = milestone.ProjectId,
                Name = milestone.Name,
                DueDate = milestone.DueDate ?? DateTime.MinValue,
                Description = milestone.Description ?? string.Empty,
                CreatedAt = milestone.CreatedAt,
                UpdatedAt = milestone.UpdatedAt
            };

            await _milestoneRepository.SaveChangesAsync();

            return ApiResponse<GetMilestoneResponse>.SuccessResponse(response, "Milestone created successfully");
        }

        public async Task<ApiResponse<string>> DeleteMilestoneAsync(Guid milestoneId)
        {
            // Lấy các task liên quan đến milestone này trước
            var tasks = await _projectTaskRepository.GetTasksByMilestoneIdAsync(milestoneId);
            
            if (tasks != null && tasks.Any())
            {
                foreach (var task in tasks)
                {
                    // Tìm milestone trong collection của task (đã được track)
                    var milestoneToRemove = task.Milestones.FirstOrDefault(m => m.Id == milestoneId);
                    if (milestoneToRemove != null)
                    {
                        task.Milestones.Remove(milestoneToRemove);
                    }
                }
                await _projectTaskRepository.SaveChangesAsync();
            }

            // Lấy milestone để xóa (không cần AsNoTracking vì chỉ dùng để soft delete)
            var milestone = await _milestoneRepository.GetByIdAsync(milestoneId);
            if (milestone == null || milestone.IsDeleted)
            {
                return ApiResponse<string>.ErrorResponse(null, "Milestone not found");
            }

            await _milestoneRepository.SoftDeleteAsync(milestone);
            await _milestoneRepository.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse(null, "Milestone deleted successfully");
        }

        public async Task<ApiResponse<GetMilestoneResponse>> GetMilestoneByIdAsync(Guid milestoneId)
        {
            var milestone = await _milestoneRepository.GetMilestoneByIdAsync(milestoneId);
            if (milestone == null || milestone.IsDeleted)
            {
                return ApiResponse<GetMilestoneResponse>.ErrorResponse(null, "Milestone not found");
            }
            var response = new GetMilestoneResponse
            {
                Id = milestone.Id,
                ProjectId = milestone.ProjectId,
                Name = milestone.Name,
                DueDate = milestone.DueDate ?? DateTime.MinValue,
                Description = milestone.Description ?? string.Empty,
                CreatedAt = milestone.CreatedAt,
                UpdatedAt = milestone.UpdatedAt
            };
            return ApiResponse<GetMilestoneResponse>.SuccessResponse(response, "Milestone retrieved successfully");
        }

        public async Task<ApiResponse<List<GetMilestoneResponse>>> GetMilestonesByProjectIdAsync(Guid projectId)
        {
            var milestones = await _milestoneRepository.GetMilestonesByProjectIdAsync(projectId);
            if (milestones == null || !milestones.Any())
            {
                return ApiResponse<List<GetMilestoneResponse>>.ErrorResponse(null, "No milestones found for the project");
            }

            var response = milestones.Select(milestone => new GetMilestoneResponse
            {
                Id = milestone.Id,
                ProjectId = milestone.ProjectId,
                Name = milestone.Name,
                DueDate = milestone.DueDate ?? DateTime.MinValue,
                Description = milestone.Description ?? string.Empty,
                CreatedAt = milestone.CreatedAt,
                UpdatedAt = milestone.UpdatedAt
            }).ToList();
            return ApiResponse<List<GetMilestoneResponse>>.SuccessResponse(response, "Milestones retrieved successfully");
        }

        public async Task<ApiResponse<GetMilestoneResponse>> UpdateMilestoneAsync(UpdateMilestoneRequest request)
        {
            // Sử dụng GetByIdAsync thay vì GetMilestoneByIdAsync để tránh load ProjectTasks
            var milestone = await _milestoneRepository.GetByIdAsync(request.Id);
            if (milestone == null || milestone.IsDeleted)
            {
                return ApiResponse<GetMilestoneResponse>.ErrorResponse(null, "Milestone not found");
            }
            
            var project = await _projectRepository.GetByIdAsync(milestone.ProjectId);
            if (project == null || project.IsDeleted)
            {
                return ApiResponse<GetMilestoneResponse>.ErrorResponse(null, "Project not found");
            }

            // Chỉ cập nhật các thuộc tính cần thiết, không động chạm đến ProjectTasks collection
            milestone.Name = request.Name;
            milestone.DueDate = request.DueDate;
            milestone.Description = request.Description;
            milestone.UpdatedAt = DateTime.UtcNow;
            
            await _milestoneRepository.UpdateAsync(milestone);
            await _milestoneRepository.SaveChangesAsync();
            
            var response = new GetMilestoneResponse
            {
                Id = milestone.Id,
                ProjectId = milestone.ProjectId,
                Name = milestone.Name,
                DueDate = milestone.DueDate ?? DateTime.MinValue,
                Description = milestone.Description ?? string.Empty,
                CreatedAt = milestone.CreatedAt,
                UpdatedAt = milestone.UpdatedAt
            };
            
            return ApiResponse<GetMilestoneResponse>.SuccessResponse(response, "Milestone updated successfully");
        }
    }
}
