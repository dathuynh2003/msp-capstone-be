using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSP.Application.Services.Interfaces.Users;
using MSP.Shared.Common;
using MSP.Shared.Enums;

namespace MSP.WebAPI.Controllers
{
    [Route("api/v1/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }



        [HttpGet("business-owners")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBusinessOwners()
        {
            var result = await _userService.GetBusinessOwnersAsync();
            return Ok(result);
        }

        [HttpGet("pending-business-owners")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingBusinessOwners()
        {
            var result = await _userService.GetPendingBusinessOwnersAsync();
            return Ok(result);
        }

        [HttpPost("approve-business-owner/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveBusinessOwner(Guid userId)
        {
            var result = await _userService.ApproveBusinessOwnerAsync(userId);
            return Ok(result);
        }

        [HttpPost("reject-business-owner/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectBusinessOwner(Guid userId)
        {
            var result = await _userService.RejectBusinessOwnerAsync(userId);
            return Ok(result);
        }

        [HttpPut("{userId}/toggle-active")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleActive(Guid userId)
        {
            var result = await _userService.ToggleUserActiveStatusAsync(userId);
            return Ok(result);
        }
    }
}
