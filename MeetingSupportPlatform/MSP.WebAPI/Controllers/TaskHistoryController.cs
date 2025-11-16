using Microsoft.AspNetCore.Mvc;
using MSP.Application.Services.Interfaces.TaskHistory;

namespace MSP.WebAPI.Controllers
{
    [Route("api/v1/TaskHistories")]
    [ApiController]
    public class TaskHistoryController : ControllerBase
    {
        private readonly ITaskHistoryService _taskHistoryService;
        public TaskHistoryController(ITaskHistoryService taskHistoryService)
        {
            _taskHistoryService = taskHistoryService;
        }

        [HttpGet("available-users")]
        public async Task<IActionResult> GetAvailableUsersForReassignment([FromQuery] Guid taskId, [FromQuery] Guid fromUserId)
        {
            try
            {
                var users = await _taskHistoryService.GetAvailableUsersForReassignmentAsync(taskId, fromUserId);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        [HttpGet("by-task/{taskId}")]
        public async Task<IActionResult> GetTaskHistoriesByTaskId([FromRoute] Guid taskId)
        {
            try
            {
                var result = await _taskHistoryService.GetTaskHistoriesByTaskIdAsync(taskId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
