using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSP.Application.Models.Requests.User;
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

        [HttpGet("get-members-managed-by/{businessOwnerId}")]
        public async Task<IActionResult> GetMembersManagedBy([FromRoute] Guid businessOwnerId)
        {
            var result = await _userService.GetMembersManagedByAsync(businessOwnerId);
            return Ok(result);
        }

        [HttpPut("re-assign-role")]
        [Authorize(Roles = "BusinessOwner")]
        public async Task<IActionResult> ReAssignRole([FromBody] ReAssignRoleRequest request)
        {
            var result = await _userService.ReAssignRoleAsync(request);
            return Ok(result);
        }
        [HttpGet("detail/{id}")]
        [Authorize(Roles = "Admin,BusinessOwner,ProjectManager,Member")]
        public async Task<IActionResult> GetUserDetail([FromRoute] Guid id)
        {
            var result = await _userService.GetUserDetailByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("business-list")]
        [Authorize(Roles = "ProjectManager,Member")]
        public async Task<IActionResult> GetBusinessList()
        {
            var curUserId = Guid.Parse(User.Claims.First(c => c.Type == "userId").Value);
            var result = await _userService.GetBusinessList(curUserId);
            return Ok(result);
        }

        [HttpGet("business-detail/{ownerId}")]
        [Authorize(Roles = "Admin,BusinessOwner,ProjectManager,Member")]
        public async Task<IActionResult> GetBusinessDetail([FromRoute] Guid ownerId)
        {
            var result = await _userService.GetBusinessDetail(ownerId);
            return Ok(result);
        }

        [HttpPost("remove-member/{memberId}")]
        [Authorize(Roles = "BusinessOwner")]
        public async Task<IActionResult> RemoveMemberFromOrganization(Guid memberId)
        {
            var businessOwnerId = Guid.Parse(User.Claims.First(c => c.Type == "userId").Value);
            var result = await _userService.RemoveMemberFromOrganizationAsync(businessOwnerId, memberId);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

    }
}
