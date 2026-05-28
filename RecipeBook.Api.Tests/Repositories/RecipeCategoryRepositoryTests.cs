using Common.Data.DataContext;
using RecipeBook.Data.TableConfigurations;
using MealPlanner.Data.TableConfigurations;
using RecipeBook.Data.Entities;
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

            services.AddSingleton(new TableConfigurationAssemblies([
                typeof(RecipeTableConfiguration).Assembly,
                typeof(MealPlanTableConfiguration).Assembly
            ]));

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
            var all = await repo.GetAllAsync(CancellationToken.None);

            // Assert
            Assert.That(all, Has.Count.EqualTo(2));
            Assert.That(all.Select(c => c.Name), Is.EquivalentTo(["Cat1", "Cat2"]));
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
            await repo.UpdateAllAsync(categories, CancellationToken.None);

            // Assert
            var fromDb = await ctx.RecipeCategories.OrderBy(c => c.Name).ToListAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(fromDb[0].DisplaySequence, Is.EqualTo(10));
                Assert.That(fromDb[1].DisplaySequence, Is.EqualTo(20));
            }
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
            await repo.UpdateAllAsync([], CancellationToken.None);

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
                await repo.UpdateAllAsync(null!, CancellationToken.None);
            });
        }

        // ---------- GetByIdsAsync ----------
        [Test]
        public async Task GetByIdsAsync_ReturnsOnlyMatchingCategories()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.RecipeCategories.AddRange(
                new RecipeCategory { Id = 1, Name = "Cat1", DisplaySequence = 1 },
                new RecipeCategory { Id = 2, Name = "Cat2", DisplaySequence = 2 },
                new RecipeCategory { Id = 3, Name = "Cat3", DisplaySequence = 3 });

            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.GetByIdsAsync([1, 3], CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(c => c.Id), Is.EquivalentTo(new[] { 1, 3 }));
        }

        [Test]
        public async Task GetByIdsAsync_EmptyIds_ReturnsEmptyList()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.RecipeCategories.Add(new RecipeCategory { Id = 1, Name = "Cat1", DisplaySequence = 1 });
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.GetByIdsAsync([], CancellationToken.None);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetByIdsAsync_SomeIdsNotFound_ReturnsOnlyExisting()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.RecipeCategories.Add(new RecipeCategory { Id = 1, Name = "Cat1", DisplaySequence = 1 });
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.GetByIdsAsync([1, 99], CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Single().Id, Is.EqualTo(1));
        }

        [Test]
        public void GetByIdsAsync_NullIds_ThrowsArgumentNullException()
        {
            var repo = CreateRepository(out _);

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await repo.GetByIdsAsync(null!, CancellationToken.None));
        }
    }
}