using MSP.Application.Services.Interfaces.Meeting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MSP.Application.Models.Requests.Meeting;

namespace MeetingService.API.Controllers
{
    [Route("api/v1/stream")]
    [ApiController]
    public class StreamController : ControllerBase
    {
        private readonly IStreamService _streamService;

        public StreamController(IStreamService streamService)
        {
            _streamService = streamService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] StreamUserRequest request)
        {
            // 1. Tạo user bên Stream
            await _streamService.CreateOrUpdateUserAsync(request);

            // 2. Generate user token cho client
            var userToken = _streamService.GenerateUserToken(request.Id);

            return Ok(new
            {
                request.Id,
                Token = userToken
            });
        }

        [HttpPost("call/{type}/{id}/delete")]
        public async Task<IActionResult> DeleteCall(string type, string id, [FromQuery] bool hard = true)
        {
            await _streamService.DeleteCallAsync(type, id, hard);
            return NoContent(); // 204
        }

        [HttpGet("call/{type}/{id}/transcriptions")]
        public async Task<IActionResult> ListTranscriptions(string type, string id)
        {
            var result = await _streamService.ListTranscriptionsAsync(type, id);
            return Ok(result);
        }

    }
}