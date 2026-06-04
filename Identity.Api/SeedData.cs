using System.Security.Claims;
using Common.Data.DataContext;
using Common.Data.Repository;
using Duende.IdentityModel;
using Identity.Data.Entities;
using Microsoft.AspNetCore.Identity;
using RecipeBook.Data.Entities;

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
            await context.EnsureSqlServerDatabaseCreatedAsync();
            context.EnsureSchemaCreated();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var productCategoryRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<ProductCategory, Guid>>();
            var recipeCategoryRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<RecipeCategory, Guid>>();

            await CategorySeedData.SeedProductCategoriesAsync(context);
            await CategorySeedData.SeedRecipeCategoriesAsync(context);
            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager, productCategoryRepository, recipeCategoryRepository);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            await EnsureRoleAsync(roleManager, AdminRoleName);
            await EnsureRoleAsync(roleManager, MemberRoleName);
        }

        private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                return;

            var role = new IdentityRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant()
            };

            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
                throw new Exception(result.Errors.First().Description);
        }

        private static async Task SeedUsersAsync(
            UserManager<ApplicationUser> userManager,
            IAsyncRepository<ProductCategory, Guid> productCategoryRepository,
            IAsyncRepository<RecipeCategory, Guid> recipeCategoryRepository)
        {
            await EnsureUserAsync(
                userManager, productCategoryRepository, recipeCategoryRepository,
                userName: "admin",
                email: "admin@mealplanner.com",
                firstName: "Admin",
                lastName: "Admin",
                roleName: AdminRoleName);

            await EnsureUserAsync(
                userManager, productCategoryRepository, recipeCategoryRepository,
                userName: "member",
                email: "member@mealplanner.com",
                firstName: "Member first name",
                lastName: "Member last name",
                roleName: MemberRoleName);
        }

        private static async Task EnsureUserAsync(
            UserManager<ApplicationUser> userManager,
            IAsyncRepository<ProductCategory, Guid> productCategoryRepository,
            IAsyncRepository<RecipeCategory, Guid> recipeCategoryRepository,
            string userName,
            string email,
            string firstName,
            string lastName,
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
                    throw new Exception(createResult.Errors.First().Description);

                var claimsResult = await userManager.AddClaimsAsync(user,
                [
                    new(JwtClaimTypes.Subject, user.Id),
                    new(ClaimTypes.Name, user.UserName!),
                    new(JwtClaimTypes.GivenName, user.FirstName),
                    new(JwtClaimTypes.FamilyName, user.LastName),
                ]);

                if (!claimsResult.Succeeded)
                    throw new Exception(claimsResult.Errors.First().Description);

                await SeedUserCategoriesAsync(user.Id, productCategoryRepository, recipeCategoryRepository);

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
                    throw new Exception(roleResult.Errors.First().Description);

                var roleClaimResult = await userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, roleName));
                if (!roleClaimResult.Succeeded)
                    throw new Exception(roleClaimResult.Errors.First().Description);
            }
        }

        private static async Task SeedUserCategoriesAsync(
            string userId,
            IAsyncRepository<ProductCategory, Guid> productCategoryRepository,
            IAsyncRepository<RecipeCategory, Guid> recipeCategoryRepository)
        {
            var allProductCategories = await productCategoryRepository.GetAllAsync(CancellationToken.None);
            foreach (var template in allProductCategories.Where(c => c.UserId == null))
            {
                await productCategoryRepository.AddAsync(
                    new ProductCategory { Name = template.Name, UserId = userId },
                    CancellationToken.None);
            }

            var allRecipeCategories = await recipeCategoryRepository.GetAllAsync(CancellationToken.None);
            foreach (var template in allRecipeCategories.Where(c => c.UserId == null))
            {
                await recipeCategoryRepository.AddAsync(
                    new RecipeCategory { Name = template.Name, DisplaySequence = template.DisplaySequence, UserId = userId },
                    CancellationToken.None);
            }
        }
    }
}
