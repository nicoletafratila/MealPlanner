using System.Security.Claims;
using Common.Data.DataContext;
using Common.Data.Entities;
using Duende.IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api
{
    public class SeedData
    {
        public static async Task EnsureSeedDataAsync(IServiceScope scope)
        {
            var context = scope.ServiceProvider.GetService<MealPlannerDbContext>();
            context?.Database.EnsureCreated();
            if (context!.Database.IsRelational())
            {
                context?.Database.Migrate();
            }
            await SeedRolesAsync(scope);
            await SeedUsersAsync(scope);
        }

        private static async Task SeedRolesAsync(IServiceScope scope)
        {
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var admin = await roleMgr.FindByIdAsync("admin");
            if (admin == null)
            {
                admin = new IdentityRole
                {
                    Id = "admin",
                    Name = "admin"
                };
                var result = await roleMgr.CreateAsync(admin);
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
            }

            var member = await roleMgr.FindByIdAsync("member");
            if (member == null)
            {
                member = new IdentityRole
                {
                    Id = "member",
                    Name = "member"
                };
                var result = await roleMgr.CreateAsync(member);
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
            }
        }

        private static async Task SeedUsersAsync(IServiceScope scope)
        {
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var admin = await userMgr.FindByNameAsync("admin");
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@mealplanner.com",
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "Admin",
                    IsActive = true
                };
                var result = await userMgr.CreateAsync(admin, "Test123!");
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = await userMgr.AddClaimsAsync(admin, new Claim[]
                {
                    new Claim(JwtClaimTypes.Name, "Admin MealPlanner"),
                    new Claim(JwtClaimTypes.GivenName, "Admin"),
                    new Claim(JwtClaimTypes.FamilyName, "Admin"),
                    new Claim(JwtClaimTypes.WebSite, "http://Admin.com")
                });
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                if (!await userMgr.IsInRoleAsync(admin, "admin"))
                {
                    var roleResult = await userMgr.AddToRoleAsync(admin, "admin");
                    if (!roleResult.Succeeded)
                    {
                        throw new Exception(roleResult.Errors.First().Description);
                    }
                    await userMgr.AddClaimAsync(admin, new Claim(ClaimTypes.Role, "admin"));
                }

                Serilog.Log.Debug("Admin created");
            }
            else
            {
                Serilog.Log.Debug("Admin already exists");
            }

            var member = await userMgr.FindByNameAsync("member");
            if (member == null)
            {
                member = new ApplicationUser
                {
                    UserName = "member",
                    Email = "member@mealplanner.com",
                    EmailConfirmed = true,
                    FirstName = "Member first name",
                    LastName = "Member last name",
                    IsActive = true
                };
                var result = await userMgr.CreateAsync(member, "Test123!");
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = await userMgr.AddClaimsAsync(member, new Claim[]
                {
                    new Claim(JwtClaimTypes.Name, "Member MealPlanner"),
                    new Claim(JwtClaimTypes.GivenName, "Member"),
                    new Claim(JwtClaimTypes.FamilyName, "Member"),
                    new Claim(JwtClaimTypes.WebSite, "http://Member.com")
                });
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                if (!await userMgr.IsInRoleAsync(member, "member"))
                {
                    var roleResult = await userMgr.AddToRoleAsync(member, "member");
                    if (!roleResult.Succeeded)
                    {
                        throw new Exception(roleResult.Errors.First().Description);
                    }
                    await userMgr.AddClaimAsync(member, new Claim(ClaimTypes.Role, "member"));
                }

                Serilog.Log.Debug("member created");
            }
            else
            {
                Serilog.Log.Debug("member already exists");
            }
        }
    }
}