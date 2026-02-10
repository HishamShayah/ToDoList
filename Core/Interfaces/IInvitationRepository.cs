using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IInvitationRepository
    {
        Task<Invitation> GetByTokenAsync(string token);
        Task AddAsync(Invitation invitation);
        Task<bool> TokenExistsAsync(string token);
    }

}
