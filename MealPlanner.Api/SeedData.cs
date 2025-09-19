using Common.Constants.Units;
using Common.Data.DataContext;
using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Api
{
    public class SeedData
    {
        public static async Task EnsureSeedDataAsync(IServiceScope scope)
        {
            var context = scope.ServiceProvider.GetService<MealPlannerDbContext>();
            context?.Database.EnsureCreated();
            //if (context!.Database.IsRelational())
            //{
            //    context?.Database.Migrate();
            //}
            await SeedUnitsAsync(context!);
            await SeedShopsAsync(context!);
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
    }
}
