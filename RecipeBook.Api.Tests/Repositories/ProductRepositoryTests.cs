using Common.Data.DataContext;
using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeBook.Api.Repositories;

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

        private static Product CreateProductGraph(
            string name,
            int categoryId,
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

            var p1 = CreateProductGraph("P1", 10, "Cat1", "kg");
            var p2 = CreateProductGraph("P2", 20, "Cat2", "l");
            ctx.Products.AddRange(p1, p2);
            await ctx.SaveChangesAsync();

            // Act
            var all = await repo.GetAllAsync();

            // Assert
            Assert.That(all.Count, Is.EqualTo(2));
            Assert.That(all.All(p => p.ProductCategory != null), Is.True);
            Assert.That(all.All(p => p.BaseUnit != null), Is.True);
        }

        [Test]
        public async Task GetByIdAsync_ReturnsProduct_WithCategoryAndBaseUnit()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var p1 = CreateProductGraph("P1", 10, "Cat1", "kg");
            ctx.Products.Add(p1);
            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdAsync(p1.Id);

            // Assert
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("P1"));
            Assert.That(found.ProductCategory, Is.Not.Null);
            Assert.That(found.BaseUnit, Is.Not.Null);
        }

        // ---------- SearchAsync by category ----------
        [Test]
        public async Task SearchAsync_ByCategoryId_ReturnsProductsInThatCategory()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var p1 = CreateProductGraph("P1", 10, "Cat1", "kg");
            var p2 = CreateProductGraph("P2", 20, "Cat2", "l");
            ctx.Products.AddRange(p1, p2);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchAsync(10);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.Single().Name, Is.EqualTo("P1"));
        }

        [Test]
        public async Task SearchAsync_ByCategoryId_NoMatches_ReturnsEmptyList()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var p1 = CreateProductGraph("P1", 10, "Cat1", "kg");
            ctx.Products.Add(p1);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchAsync(999);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        // ---------- SearchAsync by name ----------
        [Test]
        public async Task SearchAsync_ByName_ReturnsMatchingProduct_CaseInsensitive()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var p1 = CreateProductGraph("Milk", 10, "Cat1", "l");
            var p2 = CreateProductGraph("Bread", 20, "Cat2", "pcs");
            ctx.Products.AddRange(p1, p2);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchAsync("milk");

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
            var r1 = await repo.SearchAsync((string)null!);
            var r2 = await repo.SearchAsync("   ");

            // Assert
            Assert.That(r1, Is.Null);
            Assert.That(r2, Is.Null);
        }

        [Test]
        public async Task SearchAsync_ByName_NoMatch_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var p1 = CreateProductGraph("Milk", 10, "Cat1", "l");
            ctx.Products.Add(p1);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchAsync("Juice");

            // Assert
            Assert.That(result, Is.Null);
        }
    }
}