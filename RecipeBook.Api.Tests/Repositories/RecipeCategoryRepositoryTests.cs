using Common.Data.DataContext;
using Common.Pagination;
using MealPlanner.Data.TableConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeBook.Api.Repositories;
using RecipeBook.Data.Entities;
using RecipeBook.Data.TableConfigurations;

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

        private static Guid RecipeCategoryGuid(int seed) => new(seed, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

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

        // ---------- GetAllByUserAsync ----------
        [Test]
        public async Task GetAllByUserAsync_ReturnsOnlyCategoriesForThatUser_OrderedByDisplaySequence()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.RecipeCategories.AddRange(
                new RecipeCategory { Name = "Cat2", DisplaySequence = 2, UserId = "user1" },
                new RecipeCategory { Name = "Cat1", DisplaySequence = 1, UserId = "user1" },
                new RecipeCategory { Name = "Cat3", DisplaySequence = 1, UserId = "user2" });
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.GetAllByUserAsync("user1", CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(c => c.Name), Is.EqualTo(["Cat1", "Cat2"]));
        }

        [Test]
        public async Task GetAllByUserAsync_NoMatches_ReturnsEmptyList()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.RecipeCategories.Add(new RecipeCategory { Name = "Cat1", DisplaySequence = 1, UserId = "user2" });
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.GetAllByUserAsync("user1", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Empty);
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
                new RecipeCategory { Id = RecipeCategoryGuid(1), Name = "Cat1", DisplaySequence = 1 },
                new RecipeCategory { Id = RecipeCategoryGuid(2), Name = "Cat2", DisplaySequence = 2 },
                new RecipeCategory { Id = RecipeCategoryGuid(3), Name = "Cat3", DisplaySequence = 3 });

            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.GetByIdsAsync([RecipeCategoryGuid(1), RecipeCategoryGuid(3)], CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(c => c.Id), Is.EquivalentTo(new[] { RecipeCategoryGuid(1), RecipeCategoryGuid(3) }));
        }

        [Test]
        public async Task GetByIdsAsync_EmptyIds_ReturnsEmptyList()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.RecipeCategories.Add(new RecipeCategory { Id = RecipeCategoryGuid(1), Name = "Cat1", DisplaySequence = 1 });
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

            ctx.RecipeCategories.Add(new RecipeCategory { Id = RecipeCategoryGuid(1), Name = "Cat1", DisplaySequence = 1 });
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.GetByIdsAsync([RecipeCategoryGuid(1), RecipeCategoryGuid(99)], CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Single().Id, Is.EqualTo(RecipeCategoryGuid(1)));
        }

        [Test]
        public void GetByIdsAsync_NullIds_ThrowsArgumentNullException()
        {
            var repo = CreateRepository(out _);

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await repo.GetByIdsAsync(null!, CancellationToken.None));
        }

        // ---------- SearchByUserAsync ----------
        [Test]
        public async Task SearchByUserAsync_ScopesToUser()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.RecipeCategories.AddRange(
                new RecipeCategory { Name = "Cat1", DisplaySequence = 1, UserId = "user1" },
                new RecipeCategory { Name = "Cat2", DisplaySequence = 1, UserId = "user2" });
            await ctx.SaveChangesAsync();

            // Act
            var (items, totalCount, skip) = await repo.SearchByUserAsync("user1", null, null, 1, 10, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(totalCount, Is.EqualTo(1));
                Assert.That(skip, Is.Zero);
                Assert.That(items.Single().Name, Is.EqualTo("Cat1"));
            }
        }

        [Test]
        public async Task SearchByUserAsync_NoExplicitSorting_DefaultsToDisplaySequence()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.RecipeCategories.AddRange(
                new RecipeCategory { Name = "Cat2", DisplaySequence = 2, UserId = "user1" },
                new RecipeCategory { Name = "Cat1", DisplaySequence = 1, UserId = "user1" });
            await ctx.SaveChangesAsync();

            // Act
            var (items, _, _) = await repo.SearchByUserAsync("user1", null, null, 1, 10, CancellationToken.None);

            // Assert
            Assert.That(items.Select(x => x.Name), Is.EqualTo(["Cat1", "Cat2"]));
        }

        [Test]
        public async Task SearchByUserAsync_ExplicitSorting_OverridesDisplaySequenceOrder()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.RecipeCategories.AddRange(
                new RecipeCategory { Name = "Cat2", DisplaySequence = 1, UserId = "user1" },
                new RecipeCategory { Name = "Cat1", DisplaySequence = 2, UserId = "user1" });
            await ctx.SaveChangesAsync();

            var sorting = new[] { new SortingModel { PropertyName = nameof(RecipeCategory.Name), Direction = SortDirection.Ascending } };

            // Act
            var (items, _, _) = await repo.SearchByUserAsync("user1", null, sorting, 1, 10, CancellationToken.None);

            // Assert
            Assert.That(items.Select(x => x.Name), Is.EqualTo(["Cat1", "Cat2"]));
        }

        [Test]
        public async Task SearchByUserAsync_NameFilter_ReturnsOnlyMatchingCategories()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.RecipeCategories.AddRange(
                new RecipeCategory { Name = "Main", DisplaySequence = 1, UserId = "user1" },
                new RecipeCategory { Name = "Dessert", DisplaySequence = 2, UserId = "user1" });
            await ctx.SaveChangesAsync();

            var filters = new[] { new FilterItem(nameof(RecipeCategory.Name), "Main", FilterOperator.Contains) };

            // Act
            var (items, totalCount, _) = await repo.SearchByUserAsync("user1", filters, null, 1, 10, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(totalCount, Is.EqualTo(1));
                Assert.That(items.Single().Name, Is.EqualTo("Main"));
            }
        }

        [Test]
        public async Task SearchByUserAsync_Paging_ReturnsRequestedPageAndSkip()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            for (var i = 1; i <= 5; i++)
                ctx.RecipeCategories.Add(new RecipeCategory { Name = $"Cat{i}", DisplaySequence = i, UserId = "user1" });
            await ctx.SaveChangesAsync();

            // Act
            var (items, totalCount, skip) = await repo.SearchByUserAsync("user1", null, null, 2, 2, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(totalCount, Is.EqualTo(5));
                Assert.That(skip, Is.EqualTo(2));
                Assert.That(items.Select(x => x.Name), Is.EqualTo(["Cat3", "Cat4"]));
            }
        }

        [Test]
        public async Task SearchByUserAsync_NoMatches_ReturnsEmptyWithZeroTotalCount()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.RecipeCategories.Add(new RecipeCategory { Name = "Cat1", DisplaySequence = 1, UserId = "user2" });
            await ctx.SaveChangesAsync();

            // Act
            var (items, totalCount, _) = await repo.SearchByUserAsync("user1", null, null, 1, 10, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(items, Is.Empty);
                Assert.That(totalCount, Is.Zero);
            }
        }
    }
}