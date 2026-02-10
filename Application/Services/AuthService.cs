using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Core.Entities;
using Core.Helpers;
using Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly JWT _jwt;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;


        public AuthService(IAuthRepository authRepository, IOptions<JWT> jwt, IMapper mapper
            , ILogger<AuthService> logger)
        {
            _authRepository = authRepository;
            _jwt = jwt.Value;
            _mapper = mapper;
            _logger = logger;

        }

        public async Task<AuthDto> GetTokenAsync(TokenRequestModel model)
        {
            _logger.LogInformation("Attempting login for user: {Username}", model.Username);

            var user = await _authRepository.GetUserByUsernameAsync(model.Username);
            if (user == null || !await _authRepository.CheckPasswordAsync(user, model.Password))
            {
                _logger.LogWarning("Login failed for user: {Username}", model.Username);
                return new AuthDto { Message = "UserName or Password is incorrect!" };
            }
            var token = await CreateJwtToken(user);
            _logger.LogInformation("Login successful for user: {Username}", user.UserName);

            var authDto = _mapper.Map<AuthDto>(user);
            authDto.IsAuthenticated = true;
            authDto.Token = new JwtSecurityTokenHandler().WriteToken(token);
            return authDto;
        }

        private async Task<JwtSecurityToken> CreateJwtToken(IdentityUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id)
            };
            var roles = await _authRepository.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role)); 
            }

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(_jwt.DurationInDays),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }

        public async Task<AuthDto> RegisterAsync(RegisterDto register)
        {
            _logger.LogInformation("Registration attempt for user: {Email}", register.Email);

            var user = new IdentityUser { UserName = register.UserName, Email = register.Email };
            var result = await _authRepository.AddUserAsync(user, register.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(error => error.Description));
                _logger.LogWarning("Registration failed for {Email}. Errors: {Errors}", register.Email, errors);
                return new AuthDto { Message = errors };
            }
            var token = await CreateJwtToken(user);
            _logger.LogInformation("Registration successful for user: {Email}", register.Email);

            var authDto = _mapper.Map<AuthDto>(user);
            authDto.IsAuthenticated = true;
            authDto.Token = new JwtSecurityTokenHandler().WriteToken(token);
            return authDto;
        }

    }
}
