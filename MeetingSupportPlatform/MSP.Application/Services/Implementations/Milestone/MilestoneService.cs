using MSP.Application.Models.Requests.Milestone;
using MSP.Application.Models.Responses.Milestone;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Milestone;
using MSP.Shared.Common;

namespace MSP.Application.Services.Implementations.Milestone
{
    public class MilestoneService : IMilestoneService
    {
        private readonly IMilestoneRepository _milestoneRepository;
        private readonly IProjectRepository _projectRepository;

        public MilestoneService(IMilestoneRepository milestoneRepository, IProjectRepository projectRepository)
        {
            _milestoneRepository = milestoneRepository;
            _projectRepository = projectRepository;
        }

        public async Task<ApiResponse<GetMilestoneResponse>> CreateMilestoneAsync(CreateMilestoneRequest request)
        {
            var project = await _projectRepository.GetByIdAsync(request.ProjectId);

            if (project == null)
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
            var milestone = await _milestoneRepository.GetMilestoneByIdAsync(milestoneId);
            if (milestone == null)
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
            if (milestone == null)
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
            var milestone = await _milestoneRepository.GetMilestoneByIdAsync(request.Id);
            if (milestone == null)
            {
                return ApiResponse<GetMilestoneResponse>.ErrorResponse(null, "Milestone not found");
            }
            var project = await _projectRepository.GetByIdAsync(milestone.ProjectId);
            if (project == null)
            {
                return ApiResponse<GetMilestoneResponse>.ErrorResponse(null, "Project not found");
            }

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
