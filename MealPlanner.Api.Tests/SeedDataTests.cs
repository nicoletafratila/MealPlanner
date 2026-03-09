using Common.Constants.Units;
using Common.Data.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MealPlanner.Api.Tests
{
    [TestFixture]
    public class SeedDataTests
    {
        private ServiceProvider _provider = null!;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();
            services.AddDbContext<MealPlannerDbContext>(opts =>  opts.UseInMemoryDatabase($"MealPlanner_{Guid.NewGuid()}"));

            _provider = services.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

        [Test]
        public async Task EnsureSeedDataAsync_SeedsUnitsAndShops_WhenEmpty()
        {
            // Arrange
            using var scope = _provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MealPlannerDbContext>();

            // Act
            await SeedData.EnsureSeedDataAsync(scope);

            // Assert
            Assert.That(context.Units.Any(), Is.True, "Units should be seeded.");
            Assert.That(context.Shops.Any(), Is.True, "Shops should be seeded.");

            // quick check for some expected values
            Assert.That(context.Units.Any(u => u.Name == MassUnit.kg.ToString()), Is.True);
            Assert.That(context.Shops.Any(s => s.Name == "Carrefour AFI"), Is.True);
        }

        [Test]
        public async Task EnsureSeedDataAsync_DoesNotDuplicateData_WhenRunTwice()
        {
            // Arrange
            using var scope1 = _provider.CreateScope();
            using var scope2 = _provider.CreateScope();

            var ctx1 = scope1.ServiceProvider.GetRequiredService<MealPlannerDbContext>();
            var ctx2 = scope2.ServiceProvider.GetRequiredService<MealPlannerDbContext>();

            // Act
            await SeedData.EnsureSeedDataAsync(scope1);
            var unitsCountFirst = ctx1.Units.Count();
            var shopsCountFirst = ctx1.Shops.Count();

            await SeedData.EnsureSeedDataAsync(scope2);
            var unitsCountSecond = ctx2.Units.Count();
            var shopsCountSecond = ctx2.Shops.Count();

            // Assert
            Assert.That(unitsCountSecond, Is.EqualTo(unitsCountFirst));
            Assert.That(shopsCountSecond, Is.EqualTo(shopsCountFirst));
        }
    }
}