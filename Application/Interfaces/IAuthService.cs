using Application.DTOs;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthDto> GetTokenAsync(TokenRequestModel model);
        Task<AuthDto> RegisterAsync(RegisterDto register); 
    }
}
