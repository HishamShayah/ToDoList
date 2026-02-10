using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class InvitationRepository : IInvitationRepository
    {
        private readonly ApplicationDbContext _db;

        public InvitationRepository(ApplicationDbContext context)
        {
            _db = context;
        }

        public async Task AddAsync(Invitation invitation)
        {
            _db.Invitations.Add(invitation);
            await _db.SaveChangesAsync();
        }

        public async Task<Invitation> GetByTokenAsync(string token)
        {
            return await _db.Invitations.FirstOrDefaultAsync(i => i.Token == token);
        }

        public async Task<bool> TokenExistsAsync(string token)
        {
            return await _db.Invitations.AnyAsync(i => i.Token == token);
        }
    }

}
