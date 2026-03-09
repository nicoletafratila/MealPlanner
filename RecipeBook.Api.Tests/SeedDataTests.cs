using Common.Data.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RecipeBook.Api.Tests
{
    [TestFixture]
    public class SeedDataTests
    {
        private ServiceProvider _provider = null!;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();
            services.AddDbContext<MealPlannerDbContext>(options => options.UseInMemoryDatabase(databaseName: "SeedDataTests_" + TestContext.CurrentContext.Test.ID));

            _provider = services.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

        [Test]
        public async Task EnsureSeedDataAsync_SeedsProductAndRecipeCategories_WhenEmpty()
        {
            // Arrange
            using var scope = _provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MealPlannerDbContext>();

            // Act
            await SeedData.EnsureSeedDataAsync(scope);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(await context.ProductCategories.CountAsync(), Is.EqualTo(31));
                Assert.That(await context.RecipeCategories.CountAsync(), Is.EqualTo(7));
            }
        }

        [Test]
        public async Task EnsureSeedDataAsync_IsIdempotent_WhenCalledMultipleTimes()
        {
            // Arrange
            using var scope1 = _provider.CreateScope();
            var context1 = scope1.ServiceProvider.GetRequiredService<MealPlannerDbContext>();

            await SeedData.EnsureSeedDataAsync(scope1);

            var initialProductCount = await context1.ProductCategories.CountAsync();
            var initialRecipeCount = await context1.RecipeCategories.CountAsync();

            // Act: call again in a new scope (simulating app restart)
            using var scope2 = _provider.CreateScope();
            await SeedData.EnsureSeedDataAsync(scope2);

            // Assert: counts unchanged
            using var scope3 = _provider.CreateScope();
            var context3 = scope3.ServiceProvider.GetRequiredService<MealPlannerDbContext>();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(await context3.ProductCategories.CountAsync(), Is.EqualTo(initialProductCount));
                Assert.That(await context3.RecipeCategories.CountAsync(), Is.EqualTo(initialRecipeCount));
            }
        }
    }
}