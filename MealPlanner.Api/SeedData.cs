using Common.Constants.Units;
using Common.Data.DataContext;
using MealPlanner.Data.Entities;
using Microsoft.EntityFrameworkCore;
using RecipeBook.Data.Entities;

namespace MealPlanner.Api
{
    public static class SeedData
    {
        public static async Task EnsureSeedDataAsync(IServiceScope scope)
        {
            var context = scope.ServiceProvider.GetService<MealPlannerDbContext>() ?? throw new InvalidOperationException("MealPlannerDbContext is not registered in the service provider.");
            await context.EnsureSqlServerDatabaseCreatedAsync();
            context.Database.Migrate();

            await SeedUnitsAsync(context);
            await SeedShopsAsync(context);
        }

        private static async Task SeedUnitsAsync(MealPlannerDbContext context)
        {
            if (context.Units.Any())
                return;

            context.Units.AddRange(
                new Unit { Name = MassUnit.kg.ToString(), UnitType = UnitType.Weight },
                new Unit { Name = MassUnit.gr.ToString(), UnitType = UnitType.Weight },

                new Unit { Name = LiquidUnit.l.ToString(), UnitType = UnitType.Liquid },
                new Unit { Name = LiquidUnit.ml.ToString(), UnitType = UnitType.Liquid },

                new Unit { Name = VolumeUnit.cup.ToString(), UnitType = UnitType.Volume },
                new Unit { Name = VolumeUnit.tbsp.ToString(), UnitType = UnitType.Volume },
                new Unit { Name = VolumeUnit.tsp.ToString(), UnitType = UnitType.Volume },

                new Unit { Name = PieceUnit.buc.ToString(), UnitType = UnitType.Piece },
                new Unit { Name = PieceUnit.cas.ToString(), UnitType = UnitType.Piece },
                new Unit { Name = PieceUnit.con.ToString(), UnitType = UnitType.Piece },
                new Unit { Name = PieceUnit.leg.ToString(), UnitType = UnitType.Piece },
                new Unit { Name = PieceUnit.pac.ToString(), UnitType = UnitType.Piece }
            );

            await context.SaveChangesAsync();
        }

        private static readonly string[] ProductCategoryOrder =
        [
            "Lactate", "Mezeluri", "Legume", "Fructe", "Condimente", "Carne", "Conserve", "Ulei/Otet",
            "Fructe uscate", "Paste", "Branzeturi", "Fainoase", "Sosuri", "Congelate", "Apa",
            "Cofetarie/Patiserie", "Brutarie", "Peste", "Produse bucatarie", "Cereale", "Rechizite",
            "Jucarii", "Detergenti", "Haine", "Alcool", "Produse igena", "Sucuri", "Ceai/Cafea",
            "Hartie/Servetele", "Snaks", "Dulciuri"
        ];

        private static async Task SeedShopsAsync(MealPlannerDbContext context)
        {
            if (context.Shops.Any())
                return;

            var categoryIdsByName = await context.ProductCategories
                .Where(c => c.Name != null)
                .ToDictionaryAsync(c => c.Name!, c => c.Id);

            if (categoryIdsByName.Count == 0)
                return;

            context.Shops.Add(new Shop
            {
                Name = "Carrefour AFI",
                DisplaySequence = BuildDisplaySequence(
                    categoryIdsByName,
                    [10, 6, 3, 2, 20, 5, 21, 23, 12, 24, 9, 19, 22, 8, 14, 18, 1, 7, 25, 11, 26, 30, 28, 31, 4, 29, 13, 15, 27, 16, 17])
            });

            context.Shops.Add(new Shop
            {
                Name = "Carrefour Calea Bucuresti",
                DisplaySequence = BuildDisplaySequence(
                    categoryIdsByName,
                    [29, 23, 28, 27, 11, 24, 12, 15, 20, 16, 30, 13, 14, 26, 9, 17, 31, 25, 3, 19, 1, 2, 5, 4, 10, 7, 8, 18, 6, 21, 22])
            });

            await context.SaveChangesAsync();
        }

        private static List<ShopDisplaySequence> BuildDisplaySequence(
            IReadOnlyDictionary<string, Guid> categoryIdsByName,
            int[] values)
        {
            var sequence = new List<ShopDisplaySequence>();

            for (var i = 0; i < ProductCategoryOrder.Length && i < values.Length; i++)
            {
                if (categoryIdsByName.TryGetValue(ProductCategoryOrder[i], out var categoryId))
                {
                    sequence.Add(new ShopDisplaySequence { ProductCategoryId = categoryId, Value = values[i] });
                }
            }

            return sequence;
        }
    }
}