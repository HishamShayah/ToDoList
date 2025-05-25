using Application.DTOs;
using AutoMapper;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthRepository(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IdentityUser> GetUserByUsernameAsync(string username)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<bool> CheckPasswordAsync(IdentityUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<IdentityResult> AddUserAsync(IdentityUser user, string password)
        {
           var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded) {
                await _userManager.AddToRoleAsync(user, "Guest");
            }
            return result;
        }

        public async Task<List<string>> GetRolesAsync(IdentityUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }
    }
}
