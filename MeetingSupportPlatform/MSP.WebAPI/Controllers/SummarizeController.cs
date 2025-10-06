using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MSP.Application.Models.Requests.Summarize;
using MSP.Application.Models.Responses.Summarize;
using MSP.Application.Services.Interfaces.Summarize;

namespace SummarizeService.API.Controllers
{
    [Route("api/v1/summarize")]
    [ApiController]
    public class SummarizeController : ControllerBase
    {
        private readonly ISummarizeTextService _service;

        public SummarizeController(ISummarizeTextService service)
        {
            _service = service;
        }

        [HttpPost("create-todolist")]
        [Consumes("application/json", "text/plain")]
        public async Task<IActionResult> CreateTodoList([FromBody] SummarizeTextRequest request)
        {
            var result = await _service.CreateTodoListAsync(request.Text);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }

        [HttpPost("summary")]
        [Consumes("application/json", "text/plain")]
        public async Task<IActionResult> Summary([FromBody] SummarizeTextRequest request)
        {
            var result = await _service.SummarizeAsync(request.Text);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("video-text-analysis")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> VideoTextAnalysis([FromForm] SummarizeVideoTextRequest request)
        {
            var result = await _service.SummarizeVideoTextAsync(request.Text, request.Video);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
