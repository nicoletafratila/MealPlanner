using Common.Data.DataContext;
using Common.Data.Entities;
using MealPlanner.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MealPlanner.Api.Tests.Repositories
{
    [TestFixture]
    public class ShopRepositoryTests
    {
        private ServiceProvider _provider = null!;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();

            services.AddDbContext<MealPlannerDbContext>(options =>
                options.UseInMemoryDatabase("ShopRepositoryTests_" + TestContext.CurrentContext.Test.ID));

            services.AddScoped<ShopRepository>();

            _provider = services.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

        private ShopRepository CreateRepository(out MealPlannerDbContext context)
        {
            context = _provider.GetRequiredService<MealPlannerDbContext>();
            return new ShopRepository(context);
        }

        private static Shop CreateShopGraph(
            int id,
            string name,
            params (int categoryId, int value)[] displaySeq)
        {
            var display = new List<ShopDisplaySequence>();
            foreach (var (categoryId, value) in displaySeq)
            {
                var category = new ProductCategory
                {
                    Id = categoryId,
                    Name = $"Cat{categoryId}"
                };

                display.Add(new ShopDisplaySequence
                {
                    ProductCategoryId = categoryId,
                    ProductCategory = category,
                    Value = value
                });
            }

            return new Shop
            {
                Id = id,
                Name = name,
                DisplaySequence = display
            };
        }

        // ---------- GetByIdIncludeDisplaySequenceAsync ----------
        [Test]
        public async Task GetByIdIncludeDisplaySequenceAsync_ReturnsShop_WithDisplaySequenceAndCategories()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var shop = CreateShopGraph(
                id: 1,
                name: "TestShop",
                (1, 10),
                (2, 5));

            ctx.Shops.Add(shop);
            // Ensure related ProductCategory entries are tracked
            ctx.ProductCategories.AddRange(
                shop.DisplaySequence!.Select(ds => ds.ProductCategory!));
            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdIncludeDisplaySequenceAsync(1);

            // Assert
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("TestShop"));
            Assert.That(found.DisplaySequence, Is.Not.Null);
            Assert.That(found.DisplaySequence!, Has.Count.EqualTo(2));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(found.DisplaySequence![0].ProductCategory, Is.Not.Null);
                Assert.That(found.DisplaySequence![1].ProductCategory, Is.Not.Null);
            }
        }

        [Test]
        public async Task GetByIdIncludeDisplaySequenceAsync_UnknownId_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var shop = CreateShopGraph(1, "Existing");
            ctx.Shops.Add(shop);
            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdIncludeDisplaySequenceAsync(999);

            // Assert
            Assert.That(found, Is.Null);
        }

        [Test]
        public void GetByIdIncludeDisplaySequenceAsync_NullId_Throws()
        {
            // Arrange
            var repo = CreateRepository(out _);

            // Act / Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await repo.GetByIdIncludeDisplaySequenceAsync(null));
        }
    }
}