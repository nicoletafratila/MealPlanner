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
    public class ProductCategoryRepositoryTests
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
                options.UseInMemoryDatabase("ProductCategoryRepositoryTests_" + TestContext.CurrentContext.Test.ID));

            services.AddScoped<ProductCategoryRepository>();

            _provider = services.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

        private ProductCategoryRepository CreateRepository(out MealPlannerDbContext context)
        {
            context = _provider.GetRequiredService<MealPlannerDbContext>();
            return new ProductCategoryRepository(context);
        }

        [Test]
        public async Task AddAsync_PersistsCategory()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var category = new ProductCategory { Name = "Dairy" };

            // Act
            var added = await repo.AddAsync(category, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(added.Id, Is.Not.Zero);
                Assert.That(ctx.ProductCategories.Count(), Is.EqualTo(1));
                Assert.That(ctx.ProductCategories.Single().Name, Is.EqualTo("Dairy"));
            }
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllCategories()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.ProductCategories.AddRange(
                new ProductCategory { Name = "Dairy" },
                new ProductCategory { Name = "Snacks" });
            await ctx.SaveChangesAsync();

            // Act
            var all = await repo.GetAllAsync(CancellationToken.None);

            // Assert
            Assert.That(all, Has.Count.EqualTo(2));
            Assert.That(all.Select(c => c.Name), Is.EquivalentTo(["Dairy", "Snacks"]));
        }

        [Test]
        public async Task GetAllByUserAsync_ReturnsOnlyCategoriesForThatUser()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.ProductCategories.AddRange(
                new ProductCategory { Name = "Dairy", UserId = "user1" },
                new ProductCategory { Name = "Snacks", UserId = "user1" },
                new ProductCategory { Name = "Frozen", UserId = "user2" });
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.GetAllByUserAsync("user1", CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(c => c.Name), Is.EquivalentTo(["Dairy", "Snacks"]));
        }

        [Test]
        public async Task GetAllByUserAsync_NoMatches_ReturnsEmptyList()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.ProductCategories.Add(new ProductCategory { Name = "Frozen", UserId = "user2" });
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.GetAllByUserAsync("user1", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetByIdAsync_ReturnsCategory_WhenExists()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var cat = new ProductCategory { Name = "Frozen" };
            ctx.ProductCategories.Add(cat);
            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdAsync(cat.Id, CancellationToken.None);

            // Assert
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Frozen"));
        }

        [Test]
        public async Task DeleteAsync_RemovesCategory()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var cat = new ProductCategory { Name = "DeleteMe" };
            ctx.ProductCategories.Add(cat);
            await ctx.SaveChangesAsync();

            // Act
            await repo.DeleteAsync(cat, CancellationToken.None);

            // Assert
            Assert.That(ctx.ProductCategories.Any(), Is.False);
        }

        // ---------- SearchByUserAsync ----------
        [Test]
        public async Task SearchByUserAsync_ScopesToUser()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.ProductCategories.AddRange(
                new ProductCategory { Name = "Dairy", UserId = "user1" },
                new ProductCategory { Name = "Frozen", UserId = "user2" });
            await ctx.SaveChangesAsync();

            // Act
            var (items, totalCount, skip) = await repo.SearchByUserAsync("user1", null, null, 1, 10, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(totalCount, Is.EqualTo(1));
                Assert.That(skip, Is.Zero);
                Assert.That(items.Single().Name, Is.EqualTo("Dairy"));
            }
        }

        [Test]
        public async Task SearchByUserAsync_NameFilter_ReturnsOnlyMatchingCategories()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.ProductCategories.AddRange(
                new ProductCategory { Name = "Dairy", UserId = "user1" },
                new ProductCategory { Name = "Snacks", UserId = "user1" });
            await ctx.SaveChangesAsync();

            var filters = new[] { new FilterItem(nameof(ProductCategory.Name), "Dairy", FilterOperator.Contains) };

            // Act
            var (items, totalCount, _) = await repo.SearchByUserAsync("user1", filters, null, 1, 10, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(totalCount, Is.EqualTo(1));
                Assert.That(items.Single().Name, Is.EqualTo("Dairy"));
            }
        }

        [Test]
        public async Task SearchByUserAsync_Sorting_ReturnsSortedByRequestedProperty()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.ProductCategories.AddRange(
                new ProductCategory { Name = "Snacks", UserId = "user1" },
                new ProductCategory { Name = "Dairy", UserId = "user1" });
            await ctx.SaveChangesAsync();

            var sorting = new[] { new SortingModel { PropertyName = nameof(ProductCategory.Name), Direction = SortDirection.Ascending } };

            // Act
            var (items, _, _) = await repo.SearchByUserAsync("user1", null, sorting, 1, 10, CancellationToken.None);

            // Assert
            Assert.That(items.Select(x => x.Name), Is.EqualTo(["Dairy", "Snacks"]));
        }

        [Test]
        public async Task SearchByUserAsync_Paging_ReturnsRequestedPageAndSkip()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            for (var i = 1; i <= 5; i++)
                ctx.ProductCategories.Add(new ProductCategory { Name = $"Cat{i}", UserId = "user1" });
            await ctx.SaveChangesAsync();

            var sorting = new[] { new SortingModel { PropertyName = nameof(ProductCategory.Name), Direction = SortDirection.Ascending } };

            // Act
            var (items, totalCount, skip) = await repo.SearchByUserAsync("user1", null, sorting, 2, 2, CancellationToken.None);

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

            ctx.ProductCategories.Add(new ProductCategory { Name = "Frozen", UserId = "user2" });
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