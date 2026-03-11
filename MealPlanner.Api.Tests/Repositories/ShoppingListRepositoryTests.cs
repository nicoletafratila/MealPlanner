using Common.Data.DataContext;
using Common.Data.Entities;
using MealPlanner.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MealPlanner.Api.Tests.Repositories
{
    [TestFixture]
    public class ShoppingListRepositoryTests
    {
        private ServiceProvider _provider = null!;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();

            services.AddDbContext<MealPlannerDbContext>(options =>
                options.UseInMemoryDatabase("ShoppingListRepositoryTests_" + TestContext.CurrentContext.Test.ID));

            services.AddScoped<ShoppingListRepository>();

            _provider = services.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

        private ShoppingListRepository CreateRepository(out MealPlannerDbContext context)
        {
            context = _provider.GetRequiredService<MealPlannerDbContext>();
            return new ShoppingListRepository(context);
        }

        private static ShoppingList CreateShoppingListGraph(
            int id,
            string name,
            string shopName)
        {
            var category = new ProductCategory
            {
                Id = 1,
                Name = "Category1"
            };

            var baseUnit = new Unit
            {
                Id = 1,
                Name = "kg",
                UnitType = 0
            };

            var product = new Product
            {
                Id = 10,
                Name = "Flour",
                ProductCategoryId = category.Id,
                ProductCategory = category,
                BaseUnitId = baseUnit.Id,
                BaseUnit = baseUnit
            };

            var unit = new Unit
            {
                Id = 2,
                Name = "g",
                UnitType = 0
            };

            var shop = new Shop
            {
                Id = 5,
                Name = shopName
            };

            var slProduct = new ShoppingListProduct
            {
                ProductId = product.Id,
                Product = product,
                UnitId = unit.Id,
                Unit = unit,
                Quantity = 2m,
                DisplaySequence = 1
            };

            return new ShoppingList
            {
                Id = id,
                Name = name,
                ShopId = shop.Id,
                Shop = shop,
                Products = [slProduct]
            };
        }

        // ---------- GetByIdIncludeProductsAsync ----------
        [Test]
        public async Task GetByIdIncludeProductsAsync_ReturnsShoppingList_WithShopProductsAndNavigationProps()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var list = CreateShoppingListGraph(1, "Weekly", "MyShop");

            // Add graph
            ctx.ProductCategories.Add(list.Products![0].Product!.ProductCategory!);
            ctx.Units.Add(list.Products[0].Product!.BaseUnit!);
            ctx.Units.Add(list.Products[0].Unit!);
            ctx.Products.Add(list.Products[0].Product!);
            ctx.Shops.Add(list.Shop!);
            ctx.ShoppingLists.Add(list);

            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdIncludeProductsAsync(1, CancellationToken.None);

            // Assert
            Assert.That(found, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(found!.Shop, Is.Not.Null);
                Assert.That(found.Products, Is.Not.Null);
                Assert.That(found.Products!, Has.Count.EqualTo(1));
            }

            var item = found!.Products!.Single();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(item.Product, Is.Not.Null);
                Assert.That(item.Product!.ProductCategory, Is.Not.Null);
                Assert.That(item.Product!.BaseUnit, Is.Not.Null);
                Assert.That(item.Unit, Is.Not.Null);
            }
        }

        [Test]
        public async Task GetByIdIncludeProductsAsync_UnknownId_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var list = CreateShoppingListGraph(1, "Existing", "Shop");
            ctx.ShoppingLists.Add(list);
            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdIncludeProductsAsync(999, CancellationToken.None);

            // Assert
            Assert.That(found, Is.Null);
        }

        // ---------- SearchAsync by name ----------
        [Test]
        public async Task SearchAsync_ByName_ReturnsMatchingList_CaseInsensitive()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var l1 = new ShoppingList { Id = 1, Name = "Weekly" };
            var l2 = new ShoppingList { Id = 2, Name = "Other" };
            ctx.ShoppingLists.AddRange(l1, l2);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchAsync("weekly", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task SearchAsync_UnknownName_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.ShoppingLists.Add(new ShoppingList { Id = 1, Name = "Weekly" });
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchAsync("does-not-exist", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void SearchAsync_EmptyOrWhitespaceName_Throws()
        {
            // Arrange
            var repo = CreateRepository(out _);

            // Act / Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.ThrowsAsync<ArgumentException>(async () => await repo.SearchAsync("", CancellationToken.None));
                Assert.ThrowsAsync<ArgumentException>(async () => await repo.SearchAsync("   ", CancellationToken.None));
            }
        }
    }
}