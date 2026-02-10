using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IAuthRepository
    {

        Task<IdentityUser> GetUserByUsernameAsync(string username);
        Task<bool> CheckPasswordAsync(IdentityUser user, string password);
        Task<IdentityResult> AddUserAsync(IdentityUser user, string password);
        Task<List<string>> GetRolesAsync(IdentityUser user);


    }
}
