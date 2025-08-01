using System.Security.Claims;
using Common.Constants.Units;
using Common.Data.DataContext;
using Common.Data.Entities;
using Duende.IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Api
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
            await SeedProductCategoriesAsync(context!);
            await SeedRecipesCategoriesAsync(context!);
            await SeedUnitsAsync(context!);
            await SeedShopsAsync(context!);
            await SeedRolesAsync(scope);
            await SeedUsersAsync(scope);
        }

        private static async Task SeedProductCategoriesAsync(MealPlannerDbContext context)
        {
            if (context == null || context.ProductCategories.Any())
            {
                return;
            }
            context.ProductCategories.Add(new ProductCategory { Name = "Lactate" });
            context.ProductCategories.Add(new ProductCategory { Name = "Mezeluri" });
            context.ProductCategories.Add(new ProductCategory { Name = "Legume" });
            context.ProductCategories.Add(new ProductCategory { Name = "Fructe" });
            context.ProductCategories.Add(new ProductCategory { Name = "Condimente" });
            context.ProductCategories.Add(new ProductCategory { Name = "Carne" });
            context.ProductCategories.Add(new ProductCategory { Name = "Conserve" });
            context.ProductCategories.Add(new ProductCategory { Name = "Ulei/Otet" });
            context.ProductCategories.Add(new ProductCategory { Name = "Fructe uscate" });
            context.ProductCategories.Add(new ProductCategory { Name = "Paste" });
            context.ProductCategories.Add(new ProductCategory { Name = "Branzeturi" });
            context.ProductCategories.Add(new ProductCategory { Name = "Fainoase" });
            context.ProductCategories.Add(new ProductCategory { Name = "Sosuri" });
            context.ProductCategories.Add(new ProductCategory { Name = "Congelate" });
            context.ProductCategories.Add(new ProductCategory { Name = "Apa" });
            context.ProductCategories.Add(new ProductCategory { Name = "Cofetarie/Patiserie" });
            context.ProductCategories.Add(new ProductCategory { Name = "Brutarie" });
            context.ProductCategories.Add(new ProductCategory { Name = "Peste" });
            context.ProductCategories.Add(new ProductCategory { Name = "Produse bucatarie" });
            context.ProductCategories.Add(new ProductCategory { Name = "Cereale" });
            context.ProductCategories.Add(new ProductCategory { Name = "Rechizite" });
            context.ProductCategories.Add(new ProductCategory { Name = "Jucarii" });
            context.ProductCategories.Add(new ProductCategory { Name = "Detergenti" });
            context.ProductCategories.Add(new ProductCategory { Name = "Haine" });
            context.ProductCategories.Add(new ProductCategory { Name = "Alcool" });
            context.ProductCategories.Add(new ProductCategory { Name = "Produse igena" });
            context.ProductCategories.Add(new ProductCategory { Name = "Sucuri" });
            context.ProductCategories.Add(new ProductCategory { Name = "Ceai/Cafea" });
            context.ProductCategories.Add(new ProductCategory { Name = "Hartie/Servetele" });
            context.ProductCategories.Add(new ProductCategory { Name = "Snaks" });
            context.ProductCategories.Add(new ProductCategory { Name = "Dulciuri" });
            await context.SaveChangesAsync();
        }

        private static async Task SeedRecipesCategoriesAsync(MealPlannerDbContext context)
        {
            if (context == null || context.RecipeCategories.Any())
            {
                return;
            }
            context.RecipeCategories.Add(new RecipeCategory { Name = "Aperitive/Gustari", DisplaySequence = 1 });
            context.RecipeCategories.Add(new RecipeCategory { Name = "Supe/Ciorbe", DisplaySequence = 2 });
            context.RecipeCategories.Add(new RecipeCategory { Name = "Fel principal", DisplaySequence = 3 });
            context.RecipeCategories.Add(new RecipeCategory { Name = "Garnituri", DisplaySequence = 4 });
            context.RecipeCategories.Add(new RecipeCategory { Name = "Paste", DisplaySequence = 5 });
            context.RecipeCategories.Add(new RecipeCategory { Name = "Salate", DisplaySequence = 6 });
            context.RecipeCategories.Add(new RecipeCategory { Name = "Desert", DisplaySequence = 7 });
            await context.SaveChangesAsync();
        }

        private static async Task SeedUnitsAsync(MealPlannerDbContext context)
        {
            if (context == null || context.Units.Any())
            {
                return;
            }

            context.Units.Add(new Unit { Name = MassUnit.kg.ToString(), UnitType = UnitType.Mass });
            context.Units.Add(new Unit { Name = MassUnit.gr.ToString(), UnitType = UnitType.Mass });

            context.Units.Add(new Unit { Name = LiquidUnit.l.ToString(), UnitType = UnitType.Liquid });
            context.Units.Add(new Unit { Name = LiquidUnit.ml.ToString(), UnitType = UnitType.Liquid });

            context.Units.Add(new Unit { Name = VolumeUnit.cup.ToString(), UnitType = UnitType.Volume });
            context.Units.Add(new Unit { Name = VolumeUnit.tbsp.ToString(), UnitType = UnitType.Volume });
            context.Units.Add(new Unit { Name = VolumeUnit.tsp.ToString(), UnitType = UnitType.Volume });

            context.Units.Add(new Unit { Name = PieceUnit.buc.ToString(), UnitType = UnitType.Piece });
            context.Units.Add(new Unit { Name = PieceUnit.cas.ToString(), UnitType = UnitType.Piece });
            context.Units.Add(new Unit { Name = PieceUnit.con.ToString(), UnitType = UnitType.Piece });
            context.Units.Add(new Unit { Name = PieceUnit.leg.ToString(), UnitType = UnitType.Piece });
            context.Units.Add(new Unit { Name = PieceUnit.pac.ToString(), UnitType = UnitType.Piece });
            await context.SaveChangesAsync();
        }

        private static async Task SeedShopsAsync(MealPlannerDbContext context)
        {
            if (context == null || context.Shops.Any())
            {
                return;
            }
            context.Shops.Add(new Shop
            {
                Name = "Carrefour AFI",
                DisplaySequence = new List<ShopDisplaySequence>
                    {
                        new() { ProductCategoryId = 1, Value = 10 },
                        new() { ProductCategoryId = 2, Value = 6 },
                        new() { ProductCategoryId = 3, Value = 3 },
                        new() { ProductCategoryId = 4, Value = 2 },
                        new() { ProductCategoryId = 5, Value = 20 },
                        new() { ProductCategoryId = 6, Value = 5 },
                        new() { ProductCategoryId = 7, Value = 21 },
                        new() { ProductCategoryId = 8, Value = 23 },
                        new() { ProductCategoryId = 9, Value = 12 },
                        new() { ProductCategoryId = 10, Value = 24 },
                        new() { ProductCategoryId = 11, Value = 9 },
                        new() { ProductCategoryId = 12, Value = 19 },
                        new() { ProductCategoryId = 13, Value = 22 },
                        new() { ProductCategoryId = 14, Value = 8 },
                        new() { ProductCategoryId = 15, Value = 14 },
                        new() { ProductCategoryId = 16, Value = 18 },
                        new() { ProductCategoryId = 17, Value = 1 },
                        new() { ProductCategoryId = 18, Value = 7 },
                        new() { ProductCategoryId = 19, Value = 25 },
                        new() { ProductCategoryId = 20, Value = 11 },
                        new() { ProductCategoryId = 21, Value = 26 },
                        new() { ProductCategoryId = 22, Value = 30 },
                        new() { ProductCategoryId = 23, Value = 28 },
                        new() { ProductCategoryId = 24, Value = 31 },
                        new() { ProductCategoryId = 25, Value = 4 },
                        new() { ProductCategoryId = 26, Value = 29 },
                        new() { ProductCategoryId = 27, Value = 13 },
                        new() { ProductCategoryId = 28, Value = 15 },
                        new() { ProductCategoryId = 29, Value = 27 },
                        new() { ProductCategoryId = 30, Value = 16 },
                        new() { ProductCategoryId = 31, Value = 17 },
                    }
            });
            context.Shops.Add(new Shop
            {
                Name = "Carrefour Calea Bucuresti",
                DisplaySequence = new List<ShopDisplaySequence>
                    {
                        new() { ProductCategoryId = 1, Value = 29 },
                        new() { ProductCategoryId = 2, Value = 23 },
                        new() { ProductCategoryId = 3, Value = 28 },
                        new() { ProductCategoryId = 4, Value = 27 },
                        new() { ProductCategoryId = 5, Value = 11 },
                        new() { ProductCategoryId = 6, Value = 24 },
                        new() { ProductCategoryId = 7, Value = 12 },
                        new() { ProductCategoryId = 8, Value = 15 },
                        new() { ProductCategoryId = 9, Value = 20 },
                        new() { ProductCategoryId = 10, Value = 16 },
                        new() { ProductCategoryId = 11, Value = 30 },
                        new() { ProductCategoryId = 12, Value = 13 },
                        new() { ProductCategoryId = 13, Value = 14 },
                        new() { ProductCategoryId = 14, Value = 26 },
                        new() { ProductCategoryId = 15, Value = 9 },
                        new() { ProductCategoryId = 16, Value = 17 },
                        new() { ProductCategoryId = 17, Value = 31 },
                        new() { ProductCategoryId = 18, Value = 25 },
                        new() { ProductCategoryId = 19, Value = 3 },
                        new() { ProductCategoryId = 20, Value = 19 },
                        new() { ProductCategoryId = 21, Value = 1 },
                        new() { ProductCategoryId = 22, Value = 2 },
                        new() { ProductCategoryId = 23, Value = 5 },
                        new() { ProductCategoryId = 24, Value = 4 },
                        new() { ProductCategoryId = 25, Value = 10 },
                        new() { ProductCategoryId = 26, Value = 7 },
                        new() { ProductCategoryId = 27, Value = 8 },
                        new() { ProductCategoryId = 28, Value = 18 },
                        new() { ProductCategoryId = 29, Value = 6 },
                        new() { ProductCategoryId = 30, Value = 21 },
                        new() { ProductCategoryId = 31, Value = 22 },
                    }
            });
            await context.SaveChangesAsync();
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
                    new Claim(JwtClaimTypes.Name, admin.UserName),
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
                    new Claim(JwtClaimTypes.Name, member.UserName),
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
