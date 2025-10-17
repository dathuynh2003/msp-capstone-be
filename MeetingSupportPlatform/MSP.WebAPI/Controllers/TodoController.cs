using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MSP.Application.Models.Requests.Todo;
using MSP.Application.Services.Interfaces.Todos;
using MSP.Shared.Common;

namespace MSP.WebAPI.Controllers
{
    [Route("api/v1/todos")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ITodoService _todoService;
        public TodoController(ITodoService todoService)
        {
            _todoService = todoService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTodo([FromBody] CreateTodoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request"));

            var result = await _todoService.CreateTodoAsync(request);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{todoId}")]
        public async Task<IActionResult> UpdateTodo(Guid todoId, [FromBody] UpdateTodoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request"));

            var result = await _todoService.UpdateTodoAsync(todoId, request);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpDelete("{todoId}")]
        public async Task<IActionResult> DeleteTodo(Guid todoId)
        {
            var result = await _todoService.DeleteTodoAsync(todoId);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("meeting/{meetingId}")]
        public async Task<IActionResult> GetTodosByMeetingId(Guid meetingId)
        {
            var result = await _todoService.GetTodoByMeetingIdAsync(meetingId);
            return Ok(result);
        }
    }

}
