using Microsoft.AspNetCore.Mvc;
using MSP.Application.Models.Requests.Limitation;
using MSP.Application.Services.Interfaces.Limitation;

namespace MSP.WebAPI.Controllers
{
        [Route("api/v1/limitations")]
        [ApiController]
        public class LimitationController : ControllerBase
        {
            private readonly ILimitationService _limitationService;
            private readonly ILogger<LimitationController> _logger;

            public LimitationController(ILimitationService limitationService, ILogger<LimitationController> logger)
            {
                _limitationService = limitationService;
                _logger = logger;
            }

            [HttpPost]
            public async Task<IActionResult> CreateLimitation([FromBody] CreateLimitationRequest request)
            {
                var response = await _limitationService.CreateLimitationAsync(request);
                if (!response.Success)
                {
                    _logger.LogError("CreateLimitation failed: {Message}", response.Message);
                    return Ok(response);
                }
                return Ok(response);
            }

            [HttpPut]
            public async Task<IActionResult> UpdateLimitation([FromBody] UpdateLimitationRequest request)
            {
                var response = await _limitationService.UpdateLimitationAsync(request);
                if (!response.Success)
                {
                    _logger.LogError("UpdateLimitation failed: {Message}", response.Message);
                    return Ok(response);
                }
                return Ok(response);
            }

            [HttpGet("{limitationId}")]
            public async Task<IActionResult> GetLimitationById([FromRoute] Guid limitationId)
            {
                var response = await _limitationService.GetLimitationByIdAsync(limitationId);
                if (!response.Success)
                {
                    _logger.LogError("GetLimitationById failed: {Message}", response.Message);
                    return Ok(response);
                }
                return Ok(response);
            }

            [HttpGet]
            public async Task<IActionResult> GetLimitations()
            {
                var response = await _limitationService.GetLimitationsAsync();
                if (!response.Success)
                {
                    _logger.LogError("GetLimitations failed: {Message}", response.Message);
                    return Ok(response);
                }
                return Ok(response);
            }

            [HttpDelete("{limitationId}")]
            public async Task<IActionResult> DeleteLimitation([FromRoute] Guid limitationId)
            {
                var response = await _limitationService.DeleteLimitationAsync(limitationId);
                if (!response.Success)
                {
                    _logger.LogError("DeleteLimitation failed: {Message}", response.Message);
                    return Ok(response);
                }
                return Ok(response);
            }
        }
    }
