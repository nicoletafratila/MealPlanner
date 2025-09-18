using System.Security.Claims;
using Common.Data.DataContext;
using Common.Data.Entities;
using Duende.IdentityModel;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api
{
    public static class SeedData
    {
        public static async Task EnsureSeedDataAsync(IServiceScope scope)
        {
            var context = scope.ServiceProvider.GetService<MealPlannerDbContext>();
            context?.Database.EnsureCreated();
            await SeedRolesAsync(scope);
            await SeedUsersAsync(scope);
        }

        private static async Task SeedRolesAsync(IServiceScope scope)
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var admin = await roleManager.FindByIdAsync("admin");
            if (admin == null)
            {
                admin = new IdentityRole
                {
                    Id = "admin",
                    Name = "admin"
                };
                var result = await roleManager.CreateAsync(admin);
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
            }

            var member = await roleManager.FindByIdAsync("member");
            if (member == null)
            {
                member = new IdentityRole
                {
                    Id = "member",
                    Name = "member"
                };
                var result = await roleManager.CreateAsync(member);
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
            }
        }

        private static async Task SeedUsersAsync(IServiceScope scope)
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var admin = await userManager.FindByNameAsync("admin");
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
                var result = await userManager.CreateAsync(admin, "Test123!");
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = await userManager.AddClaimsAsync(admin, new Claim[]
                {
                    new Claim(JwtClaimTypes.Subject, admin.Id),
                    new Claim(ClaimTypes.Name, admin.UserName),
                    new Claim(JwtClaimTypes.GivenName, admin.FirstName),
                    new Claim(JwtClaimTypes.FamilyName, admin.LastName),
                    new Claim(JwtClaimTypes.WebSite, "http://admin.com")
                });
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                if (!await userManager.IsInRoleAsync(admin, "admin"))
                {
                    var roleResult = await userManager.AddToRoleAsync(admin, "admin");
                    if (!roleResult.Succeeded)
                    {
                        throw new Exception(roleResult.Errors.First().Description);
                    }
                    await userManager.AddClaimAsync(admin, new Claim(ClaimTypes.Role, "admin"));
                }

                Serilog.Log.Debug("Admin created");
            }
            else
            {
                Serilog.Log.Debug("Admin already exists");
            }

            var member = await userManager.FindByNameAsync("member");
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
                var result = await userManager.CreateAsync(member, "Test123!");
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = await userManager.AddClaimsAsync(member, new Claim[]
                {
                    new Claim(JwtClaimTypes.Subject, member.Id),
                    new Claim(ClaimTypes.Name, member.UserName),
                    new Claim(JwtClaimTypes.GivenName, member.FirstName),
                    new Claim(JwtClaimTypes.FamilyName, member.LastName),
                    new Claim(JwtClaimTypes.WebSite, "http://member.com")
                });
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                if (!await userManager.IsInRoleAsync(member, "member"))
                {
                    var roleResult = await userManager.AddToRoleAsync(member, "member");
                    if (!roleResult.Succeeded)
                    {
                        throw new Exception(roleResult.Errors.First().Description);
                    }
                    await userManager.AddClaimAsync(member, new Claim(ClaimTypes.Role, "member"));
                }

                Serilog.Log.Debug("Member created");
            }
            else
            {
                Serilog.Log.Debug("Member already exists");
            }
        }
    }
}
