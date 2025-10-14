using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MSP.Application.Models.Requests.Project;
using MSP.Application.Models.Responses.Auth;
using MSP.Application.Models.Responses.Project;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Project;
using MSP.Domain.Entities;
using MSP.Shared.Common;

namespace MSP.Application.Services.Implementations.Project
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly UserManager<User> _userManager;

        public ProjectService(IProjectRepository projectRepository, IProjectMemberRepository projectMemberRepository, UserManager<User> userManager)
        {
            _projectRepository = projectRepository;
            _projectMemberRepository = projectMemberRepository;
            _userManager = userManager;
        }

        public async Task<ApiResponse<GetProjectMemberResponse>> AddProjectMemberAsync(AddProjectMemeberRequest request)
        {
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
            var project = new Domain.Entities.Project
            {
                Name = request.Name,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = request.Status,
                CreatedById = request.CreatedById,
                OwnerId = request.OwnerId,
                CreatedAt = DateTime.UtcNow
            };

            _ = await _projectRepository.AddAsync(project);

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

            await _projectRepository.SaveChangesAsync();
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
            var projects = await _projectRepository.FindWithIncludePagedAsync(
                predicate: p => p.CreatedById == managerId && !p.IsDeleted,
                include: query => query
                    .Include(p => p.Owner)
                    .Include(p => p.CreatedBy),
                pageNumber: request.PageIndex,
                pageSize: request.PageSize,
                asNoTracking: true);
            if (projects == null || !projects.Any())
            {
                return ApiResponse<PagingResponse<GetProjectResponse>>.ErrorResponse(null, "No projects found for the specified Manager");
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
                TotalItems = await _projectRepository.CountAsync(p => p.CreatedById == managerId && !p.IsDeleted),
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
            var project = await _projectRepository.GetByIdAsync(request.Id);
            if (project == null)
            {
                return ApiResponse<GetProjectResponse>.ErrorResponse(null, "Project not found");
            }

            project.Name = request.Name;
            project.Description = request.Description;
            project.StartDate = request.StartDate;
            project.EndDate = request.EndDate;
            project.Status = request.Status;
            project.UpdatedAt = DateTime.UtcNow;

            await _projectRepository.UpdateAsync(project);

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

    }
}
