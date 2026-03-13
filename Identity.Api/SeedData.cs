using System.Security.Claims;
using Common.Data.DataContext;
using Common.Data.Entities;
using Duende.IdentityModel;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api
{
    public static class SeedData
    {
        private const string AdminRoleName = "admin";
        private const string MemberRoleName = "member";
        private const string DefaultPassword = "Test123!";

        public static async Task EnsureSeedDataAsync(IServiceScope scope)
        {
            var context = scope.ServiceProvider.GetRequiredService<MealPlannerDbContext>();
            context.Database.EnsureCreated();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            await EnsureRoleAsync(roleManager, AdminRoleName);
            await EnsureRoleAsync(roleManager, MemberRoleName);
        }

        private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                return;
            }

            var role = new IdentityRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant()
            };

            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
        }

        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
        {
            await EnsureUserAsync(
                userManager,
                userName: "admin",
                email: "admin@mealplanner.com",
                firstName: "Admin",
                lastName: "Admin",
                website: "http://admin.com",
                roleName: AdminRoleName);

            await EnsureUserAsync(
                userManager,
                userName: "member",
                email: "member@mealplanner.com",
                firstName: "Member first name",
                lastName: "Member last name",
                website: "http://member.com",
                roleName: MemberRoleName);
        }

        private static async Task EnsureUserAsync(
            UserManager<ApplicationUser> userManager,
            string userName,
            string email,
            string firstName,
            string lastName,
            string website,
            string roleName)
        {
            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName,
                    IsActive = true
                };

                var createResult = await userManager.CreateAsync(user, DefaultPassword);
                if (!createResult.Succeeded)
                {
                    throw new Exception(createResult.Errors.First().Description);
                }

                var claimsResult = await userManager.AddClaimsAsync(user,
                [
                    new(JwtClaimTypes.Subject, user.Id),
                    new(ClaimTypes.Name, user.UserName!),
                    new(JwtClaimTypes.GivenName, user.FirstName),
                    new(JwtClaimTypes.FamilyName, user.LastName),
                    new(JwtClaimTypes.WebSite, website)
                ]);

                if (!claimsResult.Succeeded)
                {
                    throw new Exception(claimsResult.Errors.First().Description);
                }

                Serilog.Log.Debug("{UserName} created", userName);
            }
            else
            {
                Serilog.Log.Debug("{UserName} already exists", userName);
            }

            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                var roleResult = await userManager.AddToRoleAsync(user, roleName);
                if (!roleResult.Succeeded)
                {
                    throw new Exception(roleResult.Errors.First().Description);
                }

                var roleClaimResult = await userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, roleName));
                if (!roleClaimResult.Succeeded)
                {
                    throw new Exception(roleClaimResult.Errors.First().Description);
                }
            }
        }
    }
}