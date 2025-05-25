using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class InvitationService : IInvitationService
    {
        private readonly IInvitationRepository _repository;
        private readonly ILogger<InvitationService> _logger;

        public InvitationService(IInvitationRepository repository, ILogger<InvitationService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<string> InviteAsync(InviteUserDto dto, string invitedByUserId)
        {
            _logger.LogInformation("Creating invitation for: {Email}, Role: {Role}, Invited by: {UserId}",
       dto.Email, dto.Role, invitedByUserId);
            var token = Guid.NewGuid().ToString();

            var invitation = new Invitation
            {
                Email = dto.Email,
                Role = dto.Role,
                Token = token,
                ExpiryDate = DateTime.UtcNow.AddDays(3),
                InvitedByUserId = invitedByUserId
            };

            await _repository.AddAsync(invitation);
            _logger.LogInformation("Invitation created successfully for {Email}. Token: {Token}",
       dto.Email, token);
            return token;
        }
    }

}
