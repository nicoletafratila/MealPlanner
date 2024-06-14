using Common.Constants.Units;
using Common.Data.DataContext;
using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Api
{
    public class SeedData
    {
        public static async Task EnsureSeedData(IServiceScope scope)
        {
            var context = scope.ServiceProvider.GetService<MealPlannerDbContext>();
            context?.Database.EnsureCreated();
            if (context!.Database.IsRelational())
            {
                context?.Database.Migrate();
            }
            await SeedProductCategories(context!);
            await SeedRecipesCategories(context!);
            await SeedUnits(context!);
            await SeedShops(context!);
        }

        private static async Task SeedProductCategories(MealPlannerDbContext context)
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

        private static async Task SeedRecipesCategories(MealPlannerDbContext context)
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

        private static async Task SeedUnits(MealPlannerDbContext context)
        {
            if (context == null || context.Units.Any())
            {
                return;
            }

            context.Units.Add(new Unit { Name = MassUnit.kg.ToString(), UnitType = UnitType.Mass });
            context.Units.Add(new Unit { Name = MassUnit.gr.ToString(), UnitType = UnitType.Mass });

            context.Units.Add(new Unit { Name = LiquidUnit.l.ToString(), UnitType = UnitType.Liquid });
            context.Units.Add(new Unit { Name = LiquidUnit.ml.ToString(), UnitType = UnitType.Liquid });

            context.Units.Add(new Unit { Name = AllUnit.cup.ToString(), UnitType = UnitType.All });
            context.Units.Add(new Unit { Name = AllUnit.tbsp.ToString(), UnitType = UnitType.All });
            context.Units.Add(new Unit { Name = AllUnit.tsp.ToString(), UnitType = UnitType.All });

            context.Units.Add(new Unit { Name = PieceUnit.buc.ToString(), UnitType = UnitType.Piece });
            context.Units.Add(new Unit { Name = PieceUnit.cas.ToString(), UnitType = UnitType.Piece });
            context.Units.Add(new Unit { Name = PieceUnit.con.ToString(), UnitType = UnitType.Piece });
            context.Units.Add(new Unit { Name = PieceUnit.leg.ToString(), UnitType = UnitType.Piece });
            context.Units.Add(new Unit { Name = PieceUnit.pac.ToString(), UnitType = UnitType.Piece });
            await context.SaveChangesAsync();
        }

        private static async Task SeedShops(MealPlannerDbContext context)
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
    }
}
