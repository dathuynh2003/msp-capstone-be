using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MSP.Application.Models.Requests.Project;
using MSP.Application.Models.Requests.ProjectTask;
using MSP.Application.Services.Interfaces.ProjectTask;
using MSP.Domain.Entities;
using MSP.Shared.Common;

namespace MSP.WebAPI.Controllers
{
    [Route("api/v1/tasks")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly IProjectTaskService _projectTaskService;
        private readonly ILogger<TaskController> _logger;
        public TaskController(IProjectTaskService projectTaskService, ILogger<TaskController> logger)
        {
            _projectTaskService = projectTaskService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
        {
            var response = await _projectTaskService.CreateTaskAsync(request);

            if (!response.Success)
            {
                _logger.LogError("CreateTask failed: {Message}", response.Message);
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTask([FromBody] UpdateTaskRequest request)
        {
            var response = await _projectTaskService.UpdateTaskAsync(request);

            if (!response.Success)
            {
                _logger.LogError("UpdateTask failed: {Message}", response.Message);
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpDelete("{taskId}")]
        public async Task<IActionResult> DeleteTask([FromRoute] Guid taskId)
        {
            var response = await _projectTaskService.DeleteTaskAsync(taskId);

            if (!response.Success)
            {
                _logger.LogError("DeleteTask failed: {Message}", response.Message);
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetTaskById(Guid taskId)
        {
            var response = await _projectTaskService.GetTaskByIdAsync(taskId);
            if (!response.Success)
            {
                _logger.LogError("GetTaskById failed: {Message}", response.Message);
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("by-project/{projectId}")]
        public async Task<IActionResult> GetTasksByProjectId([FromQuery] PagingRequest request, [FromRoute] Guid projectId)
        {
            var response = await _projectTaskService.GetTasksByProjectIdAsync(request, projectId);
            if (!response.Success)
            {
                _logger.LogError("GetTasksByProjectId failed: {Message}", response.Message);
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetTasksByUserId([FromQuery] PagingRequest request, [FromRoute] Guid userId)
        {
            var response = await _projectTaskService.GetTasksByUserIdAsync(request, userId);
            if (!response.Success)
            {
                _logger.LogError("GetTasksByUserId failed: {Message}", response.Message);
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("by-milestone/{milestoneId}")]
        public async Task<IActionResult> GetTasksByMilestoneId([FromRoute] Guid milestoneId)
        {
            var response = await _projectTaskService.GetTasksByMilestoneIdAsync(milestoneId);
            if (!response.Success)
            {
                _logger.LogError("GetTasksByMilestoneId failed: {Message}", response.Message);
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
