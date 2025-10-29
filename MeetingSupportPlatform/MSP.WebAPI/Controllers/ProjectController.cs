using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MSP.Application.Models.Requests.Project;
using MSP.Application.Services.Interfaces.Project;
using MSP.Shared.Common;

namespace MSP.WebAPI.Controllers
{
    [Route("api/v1/projects")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(IProjectService projectService, ILogger<ProjectController> logger)
        {
            _projectService = projectService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            var response = await _projectService.CreateProjectAsync(request);

            if (!response.Success)
            {
                _logger.LogError("CreateProject failed: {Message}", response.Message);
                return Ok(response);
            }

            return Ok(response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProject([FromBody] UpdateProjectRequest request)
        {
            var response = await _projectService.UpdateProjectAsync(request);

            if (!response.Success)
            {
                _logger.LogError("UpdateProject failed: {Message}", response.Message);
                return Ok(response);
            }

            return Ok(response);
        }

        [HttpDelete("{projectId}")]
        public async Task<IActionResult> DeleteProject([FromRoute] Guid projectId)
        {
            var response = await _projectService.DeleteProjectAsync(projectId);

            if (!response.Success)
            {
                _logger.LogError("DeleteProject failed: {Message}", response.Message);
                return Ok(response);
            }

            return Ok(response);
        }

        [HttpGet("{projectId}")]
        public async Task<IActionResult> GetProjectById([FromRoute] Guid projectId)
        {
            var response = await _projectService.GetProjectByIdAsync(projectId);

            if (!response.Success)
            {
                _logger.LogError("GetProjectById failed: {Message}", response.Message);
                return Ok(response);
            }

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjects([FromQuery] PagingRequest request)
        {
            var response = await _projectService.GetAllProjectsAsync(request);
            if (!response.Success)
            {
                _logger.LogError("GetAllProjects failed: {Message}", response.Message);
                return Ok(response);
            }
            return Ok(response);
        }

        [HttpGet("by-manager/{managerId}")]
        public async Task<IActionResult> GetProjectsByManagerId([FromQuery] PagingRequest request, [FromRoute] Guid managerId)
        {
            var response = await _projectService.GetProjectsByManagerIdAsync(request, managerId);
            if (!response.Success)
            {
                _logger.LogError("GetProjectsByManagerId failed: {Message}", response.Message);
                return Ok(response);
            }
            return Ok(response);
        }

        [HttpGet("by-bo/{boId}")]
        public async Task<IActionResult> GetProjectsByBOId([FromQuery] PagingRequest request, [FromRoute] Guid boId)
        {
            var response = await _projectService.GetProjectsByBOIdAsync(request, boId);
            if (!response.Success)
            {
                _logger.LogError("GetProjectsByBOId failed: {Message}", response.Message);
                return Ok(response);
            }
            return Ok(response);
        }

        [HttpGet("by-member/{memberId}")]
        public async Task<IActionResult> GetProjectsByMemberId([FromQuery] PagingRequest request, [FromRoute] Guid memberId)
        {
            var response = await _projectService.GetProjectsByMemberIdAsync(request, memberId);
            if (!response.Success)
            {
                _logger.LogError("GetProjectsByMemberId failed: {Message}", response.Message);
                return Ok(response);
            }
            return Ok(response);
        }

        [HttpPost("project-member")]
        public async Task<IActionResult> AddProjectMember([FromBody] AddProjectMemeberRequest request)
        {
            var response = await _projectService.AddProjectMemberAsync(request);
            if (!response.Success)
            {
                _logger.LogError("AddProjectMember failed: {Message}", response.Message);
                return Ok(response);
            }
            return Ok(response);
        }

        [HttpDelete("project-member/{pmId}")]
        public async Task<IActionResult> RemoveProjectMember([FromRoute] Guid pmId)
        {
            var response = await _projectService.RemoveProjectMemberAsync(pmId);
            if (!response.Success)
            {
                _logger.LogError("RemoveProjectMember failed: {Message}", response.Message);
                return Ok(response);
            }
            return Ok(response);
        }

        [HttpGet("project-member/{projectId}")]
        public async Task<IActionResult> GetProjectMembers([FromRoute] Guid projectId)
        {
            var response = await _projectService.GetProjectMembersAsync(projectId);
            if (!response.Success)
            {
                _logger.LogError("GetProjectMembers failed: {Message}", response.Message);
                return Ok(response);
            }
            return Ok(response);
        }
    }
}
