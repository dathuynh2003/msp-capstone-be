using MSP.Application.Services.Interfaces.Meeting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MeetingService.API.Controllers
{
    [Route("api/v1/whisper")]
    [ApiController]
    public class WhisperController : ControllerBase
    {
        private readonly IWhisperService _service;

        public WhisperController(IWhisperService service)
        {
            _service = service;
        }

        [HttpPost("video")]
        public async Task<IActionResult> TranscribeVideo(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var tempFile = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(tempFile))
            {
                await file.CopyToAsync(stream);
            }

            var result = await _service.TranscribeVideoAsync(tempFile);

            System.IO.File.Delete(tempFile);

            return Ok(result);
        }
    }
}
