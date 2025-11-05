using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MSP.Application.Models.Requests.TaskReassignRequest;
using MSP.Application.Services.Interfaces.TaskReassignRequest;

namespace MSP.WebAPI.Controllers
{
    [Route("api/v1/TaskReassignRequests")]
    [ApiController]
    public class TaskReassignRequestController : ControllerBase
    {
        private readonly ITaskReassignRequestService _taskReassignRequestService;
        public TaskReassignRequestController(ITaskReassignRequestService taskReassignRequestService)
        {
            _taskReassignRequestService = taskReassignRequestService;
        }

        [HttpGet("available-users")]
        public async Task<IActionResult> GetAvailableUsersForReassignment([FromQuery] Guid taskId, [FromQuery] Guid fromUserId)
        {
            try
            {
                var users = await _taskReassignRequestService.GetAvailableUsersForReassignmentAsync(taskId, fromUserId);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTaskReassignRequest([FromBody] CreateTaskReassignRequestRequest request)
        {
            try
            {
                var result = await _taskReassignRequestService.CreateTaskReassignRequest(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("{taskReassignRequestId}/accept")]
        public async Task<IActionResult> AcceptTaskReassignRequest([FromRoute] Guid taskReassignRequestId, [FromBody] UpdateTaskReassignRequestRequest request)
        {
            try
            {
                var result = await _taskReassignRequestService.AcceptTaskReassignRequest(taskReassignRequestId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("{taskReassignRequestId}/reject")]
        public async Task<IActionResult> RejectTaskReassignRequest([FromRoute] Guid taskReassignRequestId, [FromBody] UpdateTaskReassignRequestRequest request)
        {
            try
            {
                var result = await _taskReassignRequestService.RejectTaskReassignRequest(taskReassignRequestId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("by-task/{taskId}")]
        public async Task<IActionResult> GetTaskReassignRequestsByTaskId([FromRoute] Guid taskId)
        {
            try
            {
                var result = await _taskReassignRequestService.GetTaskReassignRequestsByTaskIdAsync(taskId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("for-user/{userId}")]
        public async Task<IActionResult> GetTaskReassignRequestsForUser([FromRoute] Guid userId)
        {
            try
            {
                var result = await _taskReassignRequestService.GetTaskReassignRequestsForUserAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
