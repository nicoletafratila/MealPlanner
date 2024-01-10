using Common.Data.DataContext;
using Common.Data.Entities;

namespace MealPlanner.Api
{
    public class SeedData
    {
        public static async Task EnsureSeedData(IServiceScope scope)
        {
            var context = scope.ServiceProvider.GetService<MealPlannerDbContext>();
            context!.Database.EnsureCreated();
            await SeedShops(context);
        }

        private static async Task SeedShops(MealPlannerDbContext context)
        {
            if (context != null && !context.Shops.Any())
            {
                context.Shops.Add(new Shop
                {
                    Name = "Carrefour AFI",
                    DisplaySequence = new List<ShopDisplaySequence>
                    {
                        new() { ProductCategoryId = 1, Value = 10 },
                        new() { ProductCategoryId = 2, Value = 6 },
                    }
                });
                await context.SaveChangesAsync();
            }
        }
    }
}


//1	3	3	Legume
//1	4	2	Fructe
//1	5	20	Condimente
//1	7	5	Carne
//1	8	21	Conserve
//1	9	23	Ulei & otet
//1	10	12	Fructe uscate
//1	11	24	Paste
//1	12	9	Branzeturi
//1	13	19	Fainoase
//1	14	22	Sosuri
//1	15	8	Congelate
//1	16	14	Apa
//1	18	18	Cofetarie si patiserie
//1	20	1	Brutarie
//1	21	7	Peste
//1	22	25	Produse bucatarie
//1	23	11	Cereale
//1	24	26	Rechizite
//1	25	30	Jucarii
//1	26	28	Detergenti
//1	27	31	Haine
//1	28	4	Alcool
//1	29	29	Produse igena
//1	30	13	Sucuri
//1	31	15	Ceai/Cafea
//1	32	27	Hartie/Servetele
//1	33	16	Snaks
//1	34	17	Dulciuri
//2	1	29	Lactate
//2	2	23	Mezeluri
//2	3	28	Legume
//2	4	27	Fructe
//2	5	11	Condimente
//2	7	24	Carne
//2	8	12	Conserve
//2	9	15	Ulei & otet
//2	10	20	Fructe uscate
//2	11	16	Paste
//2	12	30	Branzeturi
//2	13	13	Fainoase
//2	14	14	Sosuri
//2	15	26	Congelate
//2	16	9	Apa
//2	18	17	Cofetarie si patiserie
//2	20	31	Brutarie
//2	21	25	Peste
//2	22	3	Produse bucatarie
//2	23	19	Cereale
//2	24	1	Rechizite
//2	25	2	Jucarii
//2	26	5	Detergenti
//2	27	4	Haine
//2	28	10	Alcool
//2	29	7	Produse igena
//2	30	8	Sucuri
//2	31	18	Ceai/Cafea
//2	32	6	Hartie/Servetele
//2	33	21	Snaks
//2	34	22	Dulciuri