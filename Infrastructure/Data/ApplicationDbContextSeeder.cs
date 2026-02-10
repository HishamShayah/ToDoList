using Application.DTOs;
using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public static class ApplicationDbContextSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Add roles
            if (!await roleManager.RoleExistsAsync("Owner"))
            {
                await roleManager.CreateAsync(new IdentityRole("Owner"));
            }

            if (!await roleManager.RoleExistsAsync("Guest"))
            {
                await roleManager.CreateAsync(new IdentityRole("Guest"));
            }

            // Add users
            if (await userManager.FindByEmailAsync("owner@elkood.com") == null)
            {
                var owner = new IdentityUser
                {
                    UserName = "owner@elkood.com",
                    Email = "owner@elkood.com",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(owner, "P@ssw0rd");
                await userManager.AddToRoleAsync(owner, "Owner");
            }

            if (await userManager.FindByEmailAsync("guest@elkood.com") == null)
            {
                var guest = new IdentityUser
                {
                    UserName = "guest@elkood.com",
                    Email = "guest@elkood.com",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(guest, "P@ssw0rd");
                await userManager.AddToRoleAsync(guest, "Guest");
            }

            // Add sample tasks
            if (!context.Tasks.Any())
            {
                var owner = await userManager.FindByEmailAsync("owner@elkood.com");
                context.Tasks.AddRange(
                    new TaskItem { Title = "Review performance reports", IsCompleted = false,  CreatedAt = DateTime.UtcNow, Description = "Description", Category = "Main", Priority = 1 },
                    new TaskItem { Title = "Send follow-up emails to clients", IsCompleted = false,  CreatedAt = DateTime.UtcNow, Description = "Description", Category = "Main", Priority = 2 },
                    new TaskItem { Title = "Schedule weekly team meeting", IsCompleted = false,  CreatedAt = DateTime.UtcNow, Description = "Description", Category = "Main", Priority = 3 },
                    new TaskItem { Title = "Draft project development plan", IsCompleted = false,  CreatedAt = DateTime.UtcNow, Description = "Description", Category = "Main", Priority = 4 }
                );

                await context.SaveChangesAsync();
            }
        }
    }

}
