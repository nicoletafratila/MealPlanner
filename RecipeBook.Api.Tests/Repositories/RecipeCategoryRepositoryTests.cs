using Common.Data.DataContext;
using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Tests.Repositories
{
    [TestFixture]
    public class RecipeCategoryRepositoryTests
    {
        private ServiceProvider _provider = null!;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();

            services.AddDbContext<MealPlannerDbContext>(options =>
                options.UseInMemoryDatabase("RecipeCategoryRepositoryTests_" + TestContext.CurrentContext.Test.ID));

            _provider = services.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

        private RecipeCategoryRepository CreateRepository(out MealPlannerDbContext context)
        {
            context = _provider.GetRequiredService<MealPlannerDbContext>();
            return new RecipeCategoryRepository(context);
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllCategories()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.RecipeCategories.AddRange(
                new RecipeCategory { Name = "Cat1", DisplaySequence = 1 },
                new RecipeCategory { Name = "Cat2", DisplaySequence = 2 });

            await ctx.SaveChangesAsync();

            // Act
            var all = await repo.GetAllAsync();

            // Assert
            Assert.That(all.Count, Is.EqualTo(2));
            Assert.That(all.Select(c => c.Name), Is.EquivalentTo(new[] { "Cat1", "Cat2" }));
        }

        [Test]
        public async Task UpdateAllAsync_UpdatesDisplaySequences()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.RecipeCategories.AddRange(
                new RecipeCategory { Name = "Cat1", DisplaySequence = 1 },
                new RecipeCategory { Name = "Cat2", DisplaySequence = 2 });

            await ctx.SaveChangesAsync();

            // Load tracked entities and modify them
            var categories = await ctx.RecipeCategories.OrderBy(c => c.Name).ToListAsync();
            categories[0].DisplaySequence = 10;
            categories[1].DisplaySequence = 20;

            // Act
            await repo.UpdateAllAsync(categories);

            // Assert
            var fromDb = await ctx.RecipeCategories.OrderBy(c => c.Name).ToListAsync();

            Assert.Multiple(() =>
            {
                Assert.That(fromDb[0].DisplaySequence, Is.EqualTo(10));
                Assert.That(fromDb[1].DisplaySequence, Is.EqualTo(20));
            });
        }

        [Test]
        public async Task UpdateAllAsync_WithEmptyList_DoesNothing()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.RecipeCategories.Add(
                new RecipeCategory { Name = "Cat1", DisplaySequence = 1 });
            await ctx.SaveChangesAsync();

            var before = await ctx.RecipeCategories.AsNoTracking().FirstAsync();

            // Act
            await repo.UpdateAllAsync(new List<RecipeCategory>());

            // Assert
            var after = await ctx.RecipeCategories.AsNoTracking().FirstAsync();
            Assert.That(after.DisplaySequence, Is.EqualTo(before.DisplaySequence));
        }

        [Test]
        public void UpdateAllAsync_NullEntities_ThrowsArgumentNullException()
        {
            var repo = CreateRepository(out _);

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await repo.UpdateAllAsync(null!);
            });
        }
    }
}