using Common.Data.DataContext;
using MealPlanner.Data.TableConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeBook.Api.Repositories;
using RecipeBook.Data.Entities;
using RecipeBook.Data.TableConfigurations;

namespace RecipeBook.Api.Tests.Repositories
{
    [TestFixture]
    public class RecipeIngredientRepositoryTests
    {
        private ServiceProvider _provider = null!;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();

            services.AddSingleton(new TableConfigurationAssemblies([
                typeof(RecipeTableConfiguration).Assembly,
                typeof(MealPlanTableConfiguration).Assembly
            ]));

            services.AddDbContext<MealPlannerDbContext>(options =>
                options.UseInMemoryDatabase("RecipeIngredientRepositoryTests_" + TestContext.CurrentContext.Test.ID));

            services.AddScoped<RecipeIngredientRepository>();

            _provider = services.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

        private RecipeIngredientRepository CreateRepository(out MealPlannerDbContext context)
        {
            context = _provider.GetRequiredService<MealPlannerDbContext>();
            return new RecipeIngredientRepository(context);
        }

        private static Guid ProductGuid(int seed) => new(seed * 1000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        [Test]
        public async Task GetAllAsync_ReturnsAllIngredients_WithUnitsIncluded()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var unit = new Unit { Name = "kg", UnitType = 0 };
            ctx.Units.Add(unit);

            ctx.RecipeIngredients.AddRange(
                new RecipeIngredient { RecipeId = 1, ProductId = ProductGuid(1), Unit = unit, Quantity = 1 },
                new RecipeIngredient { RecipeId = 1, ProductId = ProductGuid(2), Unit = unit, Quantity = 2 });

            await ctx.SaveChangesAsync();

            // Act
            var all = await repo.GetAllAsync(CancellationToken.None);

            // Assert
            Assert.That(all, Has.Count.EqualTo(2));
            Assert.That(all.All(i => i.Unit != null), Is.True);
        }

        [Test]
        public async Task SearchAsync_ByProductId_ReturnsMatchingIngredientsOnly()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var unit = new Unit { Name = "kg", UnitType = 0 };
            ctx.Units.Add(unit);

            ctx.RecipeIngredients.AddRange(
                new RecipeIngredient { RecipeId = 1, ProductId = ProductGuid(1), Unit = unit, Quantity = 1 },
                new RecipeIngredient { RecipeId = 2, ProductId = ProductGuid(1), Unit = unit, Quantity = 2 },
                new RecipeIngredient { RecipeId = 3, ProductId = ProductGuid(2), Unit = unit, Quantity = 3 });

            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchAsync(ProductGuid(1), CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.All(i => i.ProductId == ProductGuid(1)), Is.True);
            Assert.That(result.All(i => i.Unit != null), Is.True);
        }

        [Test]
        public async Task SearchAsync_NoMatches_ReturnsEmptyList()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.RecipeIngredients.Add(
                new RecipeIngredient { RecipeId = 1, ProductId = ProductGuid(5), Quantity = 1 });
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchAsync(ProductGuid(999), CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }
    }
}
