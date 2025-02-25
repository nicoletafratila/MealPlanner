using Common.Constants.Units;
using Common.Data.DataContext;
using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace RecipeBook.Api
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
    }
}
