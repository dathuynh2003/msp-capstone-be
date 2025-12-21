using Microsoft.AspNetCore.Mvc;
using MSP.Application.Models.Requests.Project;
using MSP.Application.Models.Responses.Project;
using MSP.Shared.Common;

namespace MSP.Application.Services.Interfaces.Project
{
    public interface IProjectService
    {
        Task<ApiResponse<GetProjectResponse>> CreateProjectAsync(CreateProjectRequest request);
        Task<ApiResponse<GetProjectResponse>> UpdateProjectAsync(UpdateProjectRequest request);
        Task<ApiResponse<string>> DeleteProjectAsync(Guid projectId);
        Task<ApiResponse<GetProjectResponse>> GetProjectByIdAsync(Guid projectId, Guid curUserId);
        Task<ApiResponse<PagingResponse<GetProjectResponse>>> GetAllProjectsAsync(PagingRequest request);
        Task<ApiResponse<PagingResponse<GetProjectResponse>>> GetProjectsByManagerIdAsync(PagingRequest request, Guid managerId);
        Task<ApiResponse<PagingResponse<GetProjectResponse>>> GetProjectsByBOIdAsync(PagingRequest request, Guid boId);
        Task<ApiResponse<PagingResponse<GetProjectResponse>>> GetProjectsByMemberIdAsync(PagingRequest request, Guid memberId);
        Task<ApiResponse<GetProjectMemberResponse>> AddProjectMemberAsync(AddProjectMemeberRequest request);
        Task<ApiResponse<string>> RemoveProjectMemberAsync(Guid pmId);
        Task<ApiResponse<List<GetProjectMemberResponse>>> GetProjectMembersAsync(Guid projectId);
        Task<ApiResponse<List<GetProjectMemberResponse>>> GetProjectMembersByRoleAsync(Guid projectId, string role);
        Task<ApiResponse<List<GetProjectMemberResponse>>> GetProjectManagersAsync(Guid projectId);
        Task<ApiResponse<ProjectDetailResponse>> GetProjectDetail(Guid projectId, Guid userId);
    }
}
