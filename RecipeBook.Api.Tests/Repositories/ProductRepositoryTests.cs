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
    public class ProductRepositoryTests
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
                options.UseInMemoryDatabase("ProductRepositoryTests_" + TestContext.CurrentContext.Test.ID));

            services.AddScoped<ProductRepository>();

            _provider = services.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

        private ProductRepository CreateRepository(out MealPlannerDbContext context)
        {
            context = _provider.GetRequiredService<MealPlannerDbContext>();
            return new ProductRepository(context);
        }

        private static Guid ProductCategoryGuid(int seed) => new(seed, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        private static Product CreateProductGraph(
            string name,
            Guid categoryId,
            string categoryName,
            string baseUnitName)
        {
            var category = new ProductCategory
            {
                Id = categoryId,
                Name = categoryName
            };

            return new Product
            {
                Name = name,
                ProductCategoryId = categoryId,
                ProductCategory = category,
                BaseUnit = new Unit { Name = baseUnitName, UnitType = 0 }
            };
        }

        // ---------- GetAllAsync / GetByIdAsync ----------
        [Test]
        public async Task GetAllAsync_ReturnsAllProducts_WithCategoryAndBaseUnit()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var p1 = CreateProductGraph("P1", ProductCategoryGuid(10), "Cat1", "kg");
            var p2 = CreateProductGraph("P2", ProductCategoryGuid(20), "Cat2", "l");
            ctx.Products.AddRange(p1, p2);
            await ctx.SaveChangesAsync();

            // Act
            var all = await repo.GetAllAsync(CancellationToken.None);

            // Assert
            Assert.That(all, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(all.All(p => p.ProductCategory != null), Is.True);
                Assert.That(all.All(p => p.BaseUnit != null), Is.True);
            }
        }

        [Test]
        public async Task GetByIdAsync_ReturnsProduct_WithCategoryAndBaseUnit()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var p1 = CreateProductGraph("P1", ProductCategoryGuid(10), "Cat1", "kg");
            ctx.Products.Add(p1);
            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdAsync(p1.Id, CancellationToken.None);

            // Assert
            Assert.That(found, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(found!.Name, Is.EqualTo("P1"));
                Assert.That(found.ProductCategory, Is.Not.Null);
                Assert.That(found.BaseUnit, Is.Not.Null);
            }
        }

        // ---------- GetAllByUserAsync ----------
        [Test]
        public async Task GetAllByUserAsync_ReturnsOnlyProductsForThatUser_WithCategoryAndBaseUnit()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var p1 = CreateProductGraph("P1", ProductCategoryGuid(10), "Cat1", "kg");
            var p2 = CreateProductGraph("P2", ProductCategoryGuid(20), "Cat2", "l");
            p1.UserId = "user1";
            p2.UserId = "user2";
            ctx.Products.AddRange(p1, p2);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.GetAllByUserAsync("user1", CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            var product = result.Single();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(product.Name, Is.EqualTo("P1"));
                Assert.That(product.ProductCategory, Is.Not.Null);
                Assert.That(product.BaseUnit, Is.Not.Null);
            }
        }

        [Test]
        public async Task GetAllByUserAsync_NoMatches_ReturnsEmptyList()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var p1 = CreateProductGraph("P1", ProductCategoryGuid(10), "Cat1", "kg");
            p1.UserId = "user2";
            ctx.Products.Add(p1);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.GetAllByUserAsync("user1", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Empty);
        }

        // ---------- SearchAsync by category ----------
        [Test]
        public async Task SearchAsync_ByCategoryId_ReturnsProductsInThatCategory()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var p1 = CreateProductGraph("P1", ProductCategoryGuid(10), "Cat1", "kg");
            var p2 = CreateProductGraph("P2", ProductCategoryGuid(20), "Cat2", "l");
            ctx.Products.AddRange(p1, p2);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchAsync(ProductCategoryGuid(10), CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Single().Name, Is.EqualTo("P1"));
        }

        [Test]
        public async Task SearchAsync_ByCategoryId_NoMatches_ReturnsEmptyList()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var p1 = CreateProductGraph("P1", ProductCategoryGuid(10), "Cat1", "kg");
            ctx.Products.Add(p1);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchAsync(ProductCategoryGuid(999), CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        // ---------- SearchAsync by name ----------
        [Test]
        public async Task SearchAsync_ByName_ReturnsMatchingProduct()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var p1 = CreateProductGraph("Milk", ProductCategoryGuid(10), "Cat1", "l");
            var p2 = CreateProductGraph("Bread", ProductCategoryGuid(20), "Cat2", "pcs");
            p1.UserId = "user1";
            p2.UserId = "user1";
            ctx.Products.AddRange(p1, p2);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchAsync("milk", "user1", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Milk"));
        }

        [Test]
        public async Task SearchAsync_ByName_NullOrWhitespace_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(out _);

            // Act
            var r1 = await repo.SearchAsync((string)null!, "user1", CancellationToken.None);
            var r2 = await repo.SearchAsync("   ", "user1", CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(r1, Is.Null);
                Assert.That(r2, Is.Null);
            }
        }

        [Test]
        public async Task SearchAsync_ByName_NoMatch_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var p1 = CreateProductGraph("Milk", ProductCategoryGuid(10), "Cat1", "l");
            p1.UserId = "user1";
            ctx.Products.Add(p1);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchAsync("Juice", "user1", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Null);
        }

        // ---------- SearchByUserAsync ----------
        [Test]
        public async Task SearchByUserAsync_ScopesToUser_WithCategoryAndBaseUnit()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var p1 = CreateProductGraph("P1", ProductCategoryGuid(10), "Cat1", "kg");
            var p2 = CreateProductGraph("P2", ProductCategoryGuid(20), "Cat2", "l");
            p1.UserId = "user1";
            p2.UserId = "user2";
            ctx.Products.AddRange(p1, p2);
            await ctx.SaveChangesAsync();

            // Act
            var (items, totalCount, skip) = await repo.SearchByUserAsync(
                "user1", null, null, null, 1, 10, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(items, Has.Count.EqualTo(1));
                Assert.That(totalCount, Is.EqualTo(1));
                Assert.That(skip, Is.Zero);
                Assert.That(items.Single().Name, Is.EqualTo("P1"));
                Assert.That(items.Single().ProductCategory, Is.Not.Null);
                Assert.That(items.Single().BaseUnit, Is.Not.Null);
            }
        }

        [Test]
        public async Task SearchByUserAsync_ThumbnailOnlyTrue_ProjectsAwayImageContent_ButIncludesThumbnail()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var p1 = CreateProductGraph("P1", ProductCategoryGuid(10), "Cat1", "kg");
            p1.UserId = "user1";
            p1.ImageContent = [1, 2, 3];
            p1.ImageThumbnail = [4, 5, 6];
            ctx.Products.Add(p1);
            await ctx.SaveChangesAsync();

            // Act
            var (items, _, _) = await repo.SearchByUserAsync(
                "user1", null, null, null, 1, 10, CancellationToken.None, thumbnailOnly: true);

            // Assert
            var item = items.Single();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(item.ImageContent, Is.Null);
                Assert.That(item.ImageThumbnail, Is.EqualTo(new byte[] { 4, 5, 6 }));
            }
        }

        [Test]
        public async Task SearchByUserAsync_ThumbnailOnlyFalse_IncludesFullImageAndThumbnail()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var p1 = CreateProductGraph("P1", ProductCategoryGuid(10), "Cat1", "kg");
            p1.UserId = "user1";
            p1.ImageContent = [1, 2, 3];
            p1.ImageThumbnail = [4, 5, 6];
            ctx.Products.Add(p1);
            await ctx.SaveChangesAsync();

            // Act
            var (items, _, _) = await repo.SearchByUserAsync(
                "user1", null, null, null, 1, 10, CancellationToken.None);

            // Assert
            var item = items.Single();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(item.ImageContent, Is.EqualTo(new byte[] { 1, 2, 3 }));
                Assert.That(item.ImageThumbnail, Is.EqualTo(new byte[] { 4, 5, 6 }));
            }
        }

        [Test]
        public async Task SearchByUserAsync_CategoryIdFilter_ReturnsOnlyMatchingCategory()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var p1 = CreateProductGraph("P1", ProductCategoryGuid(10), "Cat1", "kg");
            var p2 = CreateProductGraph("P2", ProductCategoryGuid(20), "Cat2", "l");
            var p3 = new Product { Name = "P3", ProductCategoryId = p1.ProductCategoryId, ProductCategory = p1.ProductCategory, BaseUnit = new Unit { Name = "kg", UnitType = 0 } };
            p1.UserId = p2.UserId = p3.UserId = "user1";
            ctx.Products.AddRange(p1, p2, p3);
            await ctx.SaveChangesAsync();

            // Act
            var (items, totalCount, _) = await repo.SearchByUserAsync(
                "user1", ProductCategoryGuid(10), null, null, 1, 10, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(totalCount, Is.EqualTo(2));
                Assert.That(items.Select(x => x.Name), Is.EquivalentTo(["P1", "P3"]));
            }
        }

        [Test]
        public async Task SearchByUserAsync_NameFilter_ReturnsOnlyMatchingProducts()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var p1 = CreateProductGraph("Milk", ProductCategoryGuid(10), "Cat1", "l");
            var p2 = CreateProductGraph("Bread", ProductCategoryGuid(20), "Cat2", "pcs");
            p1.UserId = p2.UserId = "user1";
            ctx.Products.AddRange(p1, p2);
            await ctx.SaveChangesAsync();

            var filters = new[] { new FilterItem(nameof(Product.Name), "Milk", FilterOperator.Contains) };

            // Act
            var (items, totalCount, _) = await repo.SearchByUserAsync(
                "user1", null, filters, null, 1, 10, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(totalCount, Is.EqualTo(1));
                Assert.That(items.Single().Name, Is.EqualTo("Milk"));
            }
        }

        [Test]
        public async Task SearchByUserAsync_Sorting_ReturnsSortedByRequestedProperty()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var p1 = CreateProductGraph("Bread", ProductCategoryGuid(10), "Cat1", "pcs");
            var p2 = CreateProductGraph("Milk", ProductCategoryGuid(20), "Cat2", "l");
            p1.UserId = p2.UserId = "user1";
            ctx.Products.AddRange(p2, p1);
            await ctx.SaveChangesAsync();

            var sorting = new[] { new SortingModel { PropertyName = nameof(Product.Name), Direction = SortDirection.Ascending } };

            // Act
            var (items, _, _) = await repo.SearchByUserAsync(
                "user1", null, null, sorting, 1, 10, CancellationToken.None);

            // Assert
            Assert.That(items.Select(x => x.Name), Is.EqualTo(["Bread", "Milk"]));
        }

        [Test]
        public async Task SearchByUserAsync_Paging_ReturnsRequestedPageAndSkip()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            for (var i = 1; i <= 5; i++)
            {
                var p = CreateProductGraph($"P{i}", ProductCategoryGuid(i), $"Cat{i}", "kg");
                p.UserId = "user1";
                ctx.Products.Add(p);
            }
            await ctx.SaveChangesAsync();

            var sorting = new[] { new SortingModel { PropertyName = nameof(Product.Name), Direction = SortDirection.Ascending } };

            // Act
            var (items, totalCount, skip) = await repo.SearchByUserAsync(
                "user1", null, null, sorting, 2, 2, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(totalCount, Is.EqualTo(5));
                Assert.That(skip, Is.EqualTo(2));
                Assert.That(items.Select(x => x.Name), Is.EqualTo(["P3", "P4"]));
            }
        }

        [Test]
        public async Task SearchByUserAsync_NoMatches_ReturnsEmptyWithZeroTotalCount()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var p1 = CreateProductGraph("P1", ProductCategoryGuid(10), "Cat1", "kg");
            p1.UserId = "user2";
            ctx.Products.Add(p1);
            await ctx.SaveChangesAsync();

            // Act
            var (items, totalCount, _) = await repo.SearchByUserAsync(
                "user1", null, null, null, 1, 10, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(items, Is.Empty);
                Assert.That(totalCount, Is.Zero);
            }
        }
    }
}