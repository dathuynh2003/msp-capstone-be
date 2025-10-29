using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MSP.Application.Models.Requests.Milestone;
using MSP.Application.Services.Interfaces.Milestone;

namespace MSP.WebAPI.Controllers
{
    [Route("api/v1/milestones")]
    [ApiController]
    public class MilestoneController : ControllerBase
    {
        private readonly IMilestoneService _milestoneService;
        private readonly ILogger<MilestoneController> _logger;

        public MilestoneController(IMilestoneService milestoneService, ILogger<MilestoneController> logger)
        {
            _milestoneService = milestoneService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMilestone([FromBody] CreateMilestoneRequest request)
        {
            var response = await _milestoneService.CreateMilestoneAsync(request);
            if (!response.Success)
            {
                _logger.LogError("CreateMilestone failed: {Message}", response.Message);
                return Ok(response);
            }
            return Ok(response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMilestone([FromBody] UpdateMilestoneRequest request)
        {
            var response = await _milestoneService.UpdateMilestoneAsync(request);
            if (!response.Success)
            {
                _logger.LogError("UpdateMilestone failed: {Message}", response.Message);
                return Ok(response);
            }
            return Ok(response);
        }

        [HttpGet("{milestoneId}")]
        public async Task<IActionResult> GetMilestoneById([FromRoute] Guid milestoneId)
        {
            var response = await _milestoneService.GetMilestoneByIdAsync(milestoneId);
            if (!response.Success)
            {
                _logger.LogError("GetMilestoneById failed: {Message}", response.Message);
                return Ok(response);
            }
            return Ok(response);
        }

        [HttpGet("by-project/{projectId}")]
        public async Task<IActionResult> GetMilestonesByProjectId([FromRoute] Guid projectId)
        {
            var response = await _milestoneService.GetMilestonesByProjectIdAsync(projectId);
            if (!response.Success)
            {
                _logger.LogError("GetMilestonesByProjectId failed: {Message}", response.Message);
                return Ok(response);
            }
            return Ok(response);
        }

        [HttpDelete("{milestoneId}")]
        public async Task<IActionResult> DeleteMilestone([FromRoute] Guid milestoneId)
        {
            var response = await _milestoneService.DeleteMilestoneAsync(milestoneId);
            if (!response.Success)
            {
                _logger.LogError("DeleteMilestone failed: {Message}", response.Message);
                return Ok(response);
            }
            return Ok(response);
        }
    }
}
