using Microsoft.AspNetCore.Mvc;
using MSP.Application.Models.Requests.Package;
using MSP.Application.Services.Interfaces.Package;

namespace MSP.WebAPI.Controllers
{
    [Route("api/v1/packages")]
    [ApiController]
    public class PackageController : ControllerBase
    {
        private readonly IPackageService _packageService;
        private readonly ILogger<PackageController> _logger;

        public PackageController(IPackageService packageService, ILogger<PackageController> logger)
        {
            _packageService = packageService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePackage([FromBody] CreatePackageRequest request)
        {
            var response = await _packageService.CreateAsync(request);
            if (!response.Success)
            {
                _logger.LogError("CreatePackage failed: {Message}", response.Message);
                return Ok(response);
            }
            return Ok(response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePackage(Guid id, [FromBody] UpdatePackageRequest request)
        {
            var response = await _packageService.UpdateAsync(id, request);
            if (!response.Success)
            {
                _logger.LogError("UpdatePackage failed: {Message}", response.Message);
                return Ok(response);
            }
            return Ok(response);
        }

        [HttpGet("{packageId}")]
        public async Task<IActionResult> GetPackageById([FromRoute] Guid packageId)
        {
            var response = await _packageService.GetByIdAsync(packageId);
            if (!response.Success)
            {
                _logger.LogError("GetPackageById failed: {Message}", response.Message);
                return Ok(response);
            }
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetPackages()
        {
            var response = await _packageService.GetAllAsync();
            if (!response.Success)
            {
                _logger.LogError("GetPackages failed: {Message}", response.Message);
                return Ok(response);
            }
            return Ok(response);
        }

        [HttpDelete("{packageId}")]
        public async Task<IActionResult> DeletePackage([FromRoute] Guid packageId)
        {
            var response = await _packageService.DeleteAsync(packageId);
            if (!response.Success)
            {
                _logger.LogError("DeletePackage failed: {Message}", response.Message);
                return Ok(response);
            }
            return Ok(response);
        }
    }
}
