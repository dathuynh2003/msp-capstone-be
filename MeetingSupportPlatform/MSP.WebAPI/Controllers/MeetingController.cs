using Microsoft.AspNetCore.Mvc;
using MSP.Application.Models.Requests.Meeting;
using MSP.Application.Services.Interfaces.Meeting;

namespace MSP.WebAPI.Controllers
{
    [Route("api/v1/meetings")]
    [ApiController]
    public class MeetingController : ControllerBase
    {
        private readonly IMeetingService _meetingService;
        private readonly ILogger<MeetingController> _logger;

        public MeetingController(IMeetingService meetingService, ILogger<MeetingController> logger)
        {
            _meetingService = meetingService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingRequest request)
        {
            var response = await _meetingService.CreateMeetingAsync(request);
            if (!response.Success)
            {
                _logger.LogError("CreateMeeting failed: {Message}", response.Message);
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPut("{meetingId}")]
        public async Task<IActionResult> UpdateMeeting([FromRoute] Guid meetingId, [FromBody] UpdateMeetingRequest request)
        {
            request.MeetingId = meetingId;
            var response = await _meetingService.UpdateMeetingAsync(request);
            if (!response.Success)
            {
                _logger.LogError("UpdateMeeting failed: {Message}", response.Message);
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("{meetingId}")]
        public async Task<IActionResult> GetMeetingById([FromRoute] Guid meetingId)
        {
            var response = await _meetingService.GetMeetingByIdAsync(meetingId);
            if (!response.Success)
            {
                _logger.LogError("GetMeetingById failed: {Message}", response.Message);
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpGet("by-project/{projectId}")]
        public async Task<IActionResult> GetMeetingsByProjectId([FromRoute] Guid projectId)
        {
            var response = await _meetingService.GetMeetingsByProjectIdAsync(projectId);
            if (!response.Success)
            {
                _logger.LogError("GetMeetingsByProjectId failed: {Message}", response.Message);
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpPatch("{meetingId}/cancel")]
        public async Task<IActionResult> PauseMeeting([FromRoute] Guid meetingId)
        {
            var response = await _meetingService.CancelMeetingAsync(meetingId);
            if (!response.Success)
            {
                _logger.LogError("CancelMeeting failed: {Message}", response.Message);
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete("{meetingId}")]
        public async Task<IActionResult> DeleteMeeting([FromRoute] Guid meetingId)
        {
            var response = await _meetingService.DeleteMeetingAsync(meetingId);
            if (!response.Success)
            {
                _logger.LogError("DeleteMeeting failed: {Message}", response.Message);
                return NotFound(response);
            }
            return Ok(response);
        }
    }
}
