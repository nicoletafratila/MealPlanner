using Common.Data.DataContext;
using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Tests.Repositories
{
    [TestFixture]
    public class UnitRepositoryTests
    {
        private ServiceProvider _provider = null!;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();

            services.AddDbContext<MealPlannerDbContext>(options =>
                options.UseInMemoryDatabase("UnitRepositoryTests_" + TestContext.CurrentContext.Test.ID));

            services.AddScoped<IUnitRepository, UnitRepository>();

            _provider = services.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

        private UnitRepository CreateRepository(out MealPlannerDbContext context)
        {
            context = _provider.GetRequiredService<MealPlannerDbContext>();
            return new UnitRepository(context);
        }

        [Test]
        public async Task AddAsync_PersistsUnit()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var unit = new Unit
            {
                Name = "Kilogram",
                UnitType = Common.Constants.Units.UnitType.Weight
            };

            // Act
            var added = await repo.AddAsync(unit, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(added.Id, Is.Not.Zero);
                Assert.That(ctx.Set<Unit>().Count(), Is.EqualTo(1));
                Assert.That(ctx.Set<Unit>().Single().Name, Is.EqualTo("Kilogram"));
            }
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllUnits()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.Set<Unit>().Add(new Unit { Name = "Liter", UnitType = Common.Constants.Units.UnitType.Liquid });
            ctx.Set<Unit>().Add(new Unit { Name = "Gram", UnitType = Common.Constants.Units.UnitType.Weight });
            await ctx.SaveChangesAsync();

            // Act
            var all = await repo.GetAllAsync(CancellationToken.None);

            // Assert
            Assert.That(all, Has.Count.EqualTo(2));
            Assert.That(all.Select(u => u.Name), Is.EquivalentTo(["Liter", "Gram"]));
        }

        [Test]
        public async Task GetByIdAsync_ReturnsUnit_WhenExists()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var unit = new Unit { Name = "Piece", UnitType = Common.Constants.Units.UnitType.Piece };
            ctx.Set<Unit>().Add(unit);
            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdAsync(unit.Id, CancellationToken.None);

            // Assert
            Assert.That(found, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(found!.Id, Is.EqualTo(unit.Id));
                Assert.That(found.Name, Is.EqualTo("Piece"));
            }
        }

        [Test]
        public async Task DeleteAsync_RemovesUnit()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var unit = new Unit { Name = "Box", UnitType =  Common.Constants.Units.UnitType.Piece };
            ctx.Set<Unit>().Add(unit);
            await ctx.SaveChangesAsync();

            // Act
            await repo.DeleteAsync(unit, CancellationToken.None);

            // Assert
            Assert.That(ctx.Set<Unit>().Any(), Is.False);
        }
    }
}