using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MSP.Application.Models.Requests.OrganizationInvitatio;
using MSP.Application.Services.Interfaces.OrganizationInvitation;

namespace MSP.WebAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class OrganizationInvitationsController : ControllerBase
    {
        private readonly IOrganizationInvitationService _organizationInvitationService;
        public OrganizationInvitationsController(IOrganizationInvitationService organizationInvitationService)
        {
            _organizationInvitationService = organizationInvitationService;
        }

        [HttpPost("request-join")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> RequestJoinOrganization([FromQuery] Guid memberId, [FromQuery] Guid businessOwnerId)
        {
            var result = await _organizationInvitationService.RequestJoinOrganizeAsync(memberId, businessOwnerId);
            return Ok(result);
        }

        [HttpPost("send-invitation")]
        [Authorize(Roles = "BusinessOwner")]
        public async Task<IActionResult> SendInvitation([FromQuery] string memberEmail)
        {
            var curUserId = Guid.Parse(User.Claims.First(c => c.Type == "userId").Value);
            var result = await _organizationInvitationService.SendInvitationAsync(curUserId, memberEmail);
            return Ok(result);
        }

        [HttpPost("send-invitations")]
        [Authorize(Roles = "BusinessOwner")]
        public async Task<IActionResult> SendInvitations([FromBody] SendInvitationListRequest request)
        {
            var curUserId = Guid.Parse(User.Claims.First(c => c.Type == "userId").Value);
            var result = await _organizationInvitationService.SendInvitationListAsync(curUserId, request.MemberEmails);
            return Ok(result);
        }

        [HttpGet("sent-invitations")]
        [Authorize(Roles = "BusinessOwner")]
        public async Task<IActionResult> GetSentInvitationsByBusinessOwnerId()
        {
            var businessOwnerId = Guid.Parse(User.Claims.First(c => c.Type == "userId").Value);
            var result = await _organizationInvitationService.GetSentInvitationsByBusinessOwnerIdAsync(businessOwnerId);
            return Ok(result);
        }
        [HttpGet("pending-requests")]
        [Authorize(Roles = "BusinessOwner")]
        public async Task<IActionResult> GetPendingRequestsByBusinessOwnerId()
        {
            var curUserId = Guid.Parse(User.Claims.First(c => c.Type == "userId").Value);
            var result = await _organizationInvitationService.GetPendingRequestsByBusinessOwnerIdAsync(curUserId);
            return Ok(result);
        }
        [HttpGet("received-invitations/{memberId}")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetReceivedInvitationsByMemberId(Guid memberId)
        {
            var result = await _organizationInvitationService.GetReceivedInvitationsByMemberIdAsync(memberId);
            return Ok(result);
        }
        [HttpGet("sent-requests/{memberId}")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetSentRequestsByMemberId(Guid memberId)
        {
            var result = await _organizationInvitationService.GetSentRequestsByMemberIdAsync(memberId);
            return Ok(result);
        }

        [HttpPost("accept-invitation/{invitationId}")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> AcceptInvitation(Guid invitationId)
        {
            var curUserId = Guid.Parse(User.Claims.First(c => c.Type == "userId").Value);
            var result = await _organizationInvitationService.MemberAcceptInvitationAsync(curUserId, invitationId);

            return Ok(result);
        }

        /// <summary>
        /// [BusinessOwner] Chấp nhận (accept) request từ member muốn join organization
        /// </summary>
        /// <param name="invitationId"></param>
        /// <returns></returns>
        [HttpPost("accept-request/{invitationId}")]
        [Authorize(Roles = "BusinessOwner")]
        public async Task<IActionResult> AcceptRequest(Guid invitationId)
        {
            var curUserId = Guid.Parse(User.Claims.First(c => c.Type == "userId").Value);
            var result = await _organizationInvitationService.BusinessOwnerAcceptRequestAsync(curUserId, invitationId);
            return Ok(result);
        }

        [HttpPost("leave-organization")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> LeaveOrganization()
        {
            var curUserId = Guid.Parse(User.Claims.First(c => c.Type == "userId").Value);
            var result = await _organizationInvitationService.MemberLeaveOrganizationAsync(curUserId);
            return Ok(result);
        }

        /// <summary>
        /// [Member] Từ chối (reject) invitation từ Business Owner
        /// </summary>
        [HttpPost("reject-invitation/{invitationId}")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> RejectInvitation(Guid invitationId)
        {
            var curUserId = Guid.Parse(User.Claims.First(c => c.Type == "userId").Value);
            var result = await _organizationInvitationService.MemberRejectInvitationAsync(curUserId, invitationId);
            return Ok(result);
        }

        /// <summary>
        /// [BusinessOwner] Từ chối (reject) request xin join từ member
        /// </summary>
        [HttpPost("reject-request/{invitationId}")]
        [Authorize(Roles = "BusinessOwner")]
        public async Task<IActionResult> RejectRequest(Guid invitationId)
        {
            var curUserId = Guid.Parse(User.Claims.First(c => c.Type == "userId").Value);
            var result = await _organizationInvitationService.BusinessOwnerRejectRequestAsync(curUserId, invitationId);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

    }
}
