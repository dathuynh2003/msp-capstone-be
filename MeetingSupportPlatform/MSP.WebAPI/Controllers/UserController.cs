using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSP.Application.Services.Interfaces.Users;
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
    }
}
