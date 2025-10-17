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
        Task<ApiResponse<GetProjectResponse>> GetProjectByIdAsync(Guid projectId);
        Task<ApiResponse<PagingResponse<GetProjectResponse>>> GetAllProjectsAsync(PagingRequest request);
        Task<ApiResponse<PagingResponse<GetProjectResponse>>> GetProjectsByManagerIdAsync(PagingRequest request, Guid managerId);
        Task<ApiResponse<PagingResponse<GetProjectResponse>>> GetProjectsByBOIdAsync(PagingRequest request, Guid boId);
        Task<ApiResponse<PagingResponse<GetProjectResponse>>> GetProjectsByMemberIdAsync(PagingRequest request, Guid memberId);
        Task<ApiResponse<GetProjectMemberResponse>> AddProjectMemberAsync(AddProjectMemeberRequest request);
        Task<ApiResponse<string>> RemoveProjectMemberAsync(Guid pmId);
        Task<ApiResponse<List<GetProjectMemberResponse>>> GetProjectMembersAsync(Guid projectId);
    }
}
