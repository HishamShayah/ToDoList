using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [Authorize(Roles = "Owner")]
    [ApiController]
    [Route("api/[controller]")]
    public class InvitationsController : ControllerBase
    {
        private readonly IInvitationService _invitationService;

        public InvitationsController(IInvitationService invitationService)
        {
            _invitationService = invitationService;
        }

        [HttpPost("invite")]
        public async Task<IActionResult> Invite([FromBody] InviteUserDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var token = await _invitationService.InviteAsync(dto, userId);

            var inviteLink = $"https://elkood.com/register?token={token}";

            return Ok(new { message = "Invitation sent", link = inviteLink });
        }
    }

}
