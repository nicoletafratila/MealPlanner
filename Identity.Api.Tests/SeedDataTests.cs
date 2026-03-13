using System.Security.Claims;
using Common.Data.DataContext;
using Common.Data.Entities;
using Duende.IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Api.Tests
{
    [TestFixture]
    public class SeedDataTests
    {
        private ServiceProvider _provider = null!;
        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();

            services.AddDbContext<MealPlannerDbContext>(opts => opts.UseInMemoryDatabase($"MealPlanner_{Guid.NewGuid()}"));
            services
                .AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.User.RequireUniqueEmail = false;
                })
                .AddEntityFrameworkStores<MealPlannerDbContext>()
                .AddDefaultTokenProviders();

            services.AddLogging();
            _provider = services.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

        [Test]
        public async Task EnsureSeedDataAsync_SeedsRolesAndUsers_WhenEmpty()
        {
            // Arrange
            using var scope = _provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MealPlannerDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Act
            await SeedData.EnsureSeedDataAsync(scope);

            Assert.Multiple(async () =>
            {
                // Assert - roles
                Assert.That(await roleManager.RoleExistsAsync("admin"), Is.True, "Admin role should be seeded.");
                Assert.That(await roleManager.RoleExistsAsync("member"), Is.True, "Member role should be seeded.");
            });

            // Assert - admin user
            var admin = await userManager.FindByNameAsync("admin");
            Assert.That(admin, Is.Not.Null, "Admin user should be created.");
            Assert.Multiple(async () =>
            {
                Assert.That(admin!.Email, Is.EqualTo("admin@mealplanner.com"));
                Assert.That(admin.EmailConfirmed, Is.True);
                Assert.That(admin.IsActive, Is.True);
                Assert.That(await userManager.IsInRoleAsync(admin, "admin"), Is.True);
            });

            var adminClaims = await userManager.GetClaimsAsync(admin);
            Assert.Multiple(() =>
            {
                Assert.That(adminClaims.Any(c => c.Type == JwtClaimTypes.Subject && c.Value == admin.Id), Is.True);
                Assert.That(adminClaims.Any(c => c.Type == ClaimTypes.Name && c.Value == "admin"), Is.True);
                Assert.That(adminClaims.Any(c => c.Type == JwtClaimTypes.GivenName && c.Value == "Admin"), Is.True);
                Assert.That(adminClaims.Any(c => c.Type == JwtClaimTypes.FamilyName && c.Value == "Admin"), Is.True);
                Assert.That(adminClaims.Any(c => c.Type == JwtClaimTypes.WebSite && c.Value == "http://admin.com"), Is.True);
                Assert.That(adminClaims.Any(c => c.Type == ClaimTypes.Role && c.Value == "admin"), Is.True);
            });

            // Assert - member user
            var member = await userManager.FindByNameAsync("member");
            Assert.That(member, Is.Not.Null, "Member user should be created.");
            Assert.Multiple(async () =>
            {
                Assert.That(member!.Email, Is.EqualTo("member@mealplanner.com"));
                Assert.That(member.EmailConfirmed, Is.True);
                Assert.That(member.IsActive, Is.True);
                Assert.That(await userManager.IsInRoleAsync(member, "member"), Is.True);
            });

            var memberClaims = await userManager.GetClaimsAsync(member);
            Assert.Multiple(() =>
            {
                Assert.That(memberClaims.Any(c => c.Type == JwtClaimTypes.Subject && c.Value == member.Id), Is.True);
                Assert.That(memberClaims.Any(c => c.Type == ClaimTypes.Name && c.Value == "member"), Is.True);
                Assert.That(memberClaims.Any(c => c.Type == JwtClaimTypes.GivenName && c.Value == "Member first name"), Is.True);
                Assert.That(memberClaims.Any(c => c.Type == JwtClaimTypes.FamilyName && c.Value == "Member last name"), Is.True);
                Assert.That(memberClaims.Any(c => c.Type == JwtClaimTypes.WebSite && c.Value == "http://member.com"), Is.True);
                Assert.That(memberClaims.Any(c => c.Type == ClaimTypes.Role && c.Value == "member"), Is.True);
            });
        }

        [Test]
        public async Task EnsureSeedDataAsync_DoesNotDuplicateUsersOrRoles_WhenRunTwice()
        {
            // Arrange
            using var scope1 = _provider.CreateScope();
            using var scope2 = _provider.CreateScope();

            var ctx1 = scope1.ServiceProvider.GetRequiredService<MealPlannerDbContext>();
            var ctx2 = scope2.ServiceProvider.GetRequiredService<MealPlannerDbContext>();

            // Act
            await SeedData.EnsureSeedDataAsync(scope1);
            var usersCountFirst = ctx1.Users.Count();
            var rolesCountFirst = ctx1.Roles.Count();

            await SeedData.EnsureSeedDataAsync(scope2);
            var usersCountSecond = ctx2.Users.Count();
            var rolesCountSecond = ctx2.Roles.Count();

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(usersCountSecond, Is.EqualTo(usersCountFirst), "User count should not grow on second run.");
                Assert.That(rolesCountSecond, Is.EqualTo(rolesCountFirst), "Role count should not grow on second run.");
            });
        }
    }
}