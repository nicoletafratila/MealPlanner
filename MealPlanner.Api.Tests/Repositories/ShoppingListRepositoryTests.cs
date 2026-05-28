using Common.Data.DataContext;
using RecipeBook.Data.TableConfigurations;
using MealPlanner.Data.TableConfigurations;
using MealPlanner.Data.Entities;
using RecipeBook.Data.Entities;
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

            services.AddSingleton(new TableConfigurationAssemblies([
                typeof(RecipeTableConfiguration).Assembly,
                typeof(MealPlanTableConfiguration).Assembly
            ]));

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
            var repo = CreateRepository(out var ctx);

            var list = CreateShoppingListGraph(1, "Weekly", "MyShop");

            ctx.ProductCategories.Add(list.Products![0].Product!.ProductCategory!);
            ctx.Units.Add(list.Products[0].Product!.BaseUnit!);
            ctx.Units.Add(list.Products[0].Unit!);
            ctx.Products.Add(list.Products[0].Product!);
            ctx.Shops.Add(list.Shop!);
            ctx.ShoppingLists.Add(list);

            await ctx.SaveChangesAsync();

            var found = await repo.GetByIdIncludeProductsAsync(1, CancellationToken.None);

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
            var repo = CreateRepository(out var ctx);

            var list = CreateShoppingListGraph(1, "Existing", "Shop");
            ctx.ShoppingLists.Add(list);
            await ctx.SaveChangesAsync();

            var found = await repo.GetByIdIncludeProductsAsync(999, CancellationToken.None);

            Assert.That(found, Is.Null);
        }

        // ---------- SearchAsync by name ----------
        [Test]
        public async Task SearchAsync_ByName_ReturnsMatchingList()
        {
            var repo = CreateRepository(out var ctx);

            var l1 = new ShoppingList { Id = 1, Name = "Weekly", UserId = "user1" };
            var l2 = new ShoppingList { Id = 2, Name = "Other", UserId = "user1" };
            ctx.ShoppingLists.AddRange(l1, l2);
            await ctx.SaveChangesAsync();

            var result = await repo.SearchAsync("weekly", "user1", CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task SearchAsync_UnknownName_ReturnsNull()
        {
            var repo = CreateRepository(out var ctx);

            ctx.ShoppingLists.Add(new ShoppingList { Id = 1, Name = "Weekly", UserId = "user1" });
            await ctx.SaveChangesAsync();

            var result = await repo.SearchAsync("does-not-exist", "user1", CancellationToken.None);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void SearchAsync_EmptyOrWhitespaceName_Throws()
        {
            var repo = CreateRepository(out _);

            using (Assert.EnterMultipleScope())
            {
                Assert.ThrowsAsync<ArgumentException>(async () => await repo.SearchAsync("", "user1", CancellationToken.None));
                Assert.ThrowsAsync<ArgumentException>(async () => await repo.SearchAsync("   ", "user1", CancellationToken.None));
            }
        }

        // ---------- UpdateAsync ----------
        [Test]
        public async Task UpdateAsync_AddsNewProduct_ToShoppingList()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);
            ctx.Shops.Add(new Shop { Id = 5, Name = "Shop" });
            ctx.Units.Add(new Unit { Id = 1, Name = "kg", UnitType = 0 });
            ctx.ShoppingLists.Add(new ShoppingList { Id = 1, Name = "List1", ShopId = 5, UserId = "user1" });
            ctx.ShoppingListProducts.Add(
                new ShoppingListProduct { ShoppingListId = 1, ProductId = 10, UnitId = 1, Quantity = 1m });
            await ctx.SaveChangesAsync();

            var entity = await repo.GetByIdIncludeProductsAsync(1, CancellationToken.None);
            entity!.Products =
            [
                new ShoppingListProduct { ShoppingListId = 1, ProductId = 10, UnitId = 1, Quantity = 1m },
                new ShoppingListProduct { ShoppingListId = 1, ProductId = 20, UnitId = 1, Quantity = 3m }
            ];

            // Act
            await repo.UpdateAsync(entity, CancellationToken.None);

            // Assert
            var rows = ctx.ShoppingListProducts.Where(p => p.ShoppingListId == 1).ToList();
            Assert.That(rows, Has.Count.EqualTo(2));
            Assert.That(rows.Select(p => p.ProductId), Is.EquivalentTo(new[] { 10, 20 }));
        }

        [Test]
        public async Task UpdateAsync_RemovesDeletedProduct_FromShoppingList()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);
            ctx.Shops.Add(new Shop { Id = 5, Name = "Shop" });
            ctx.Units.Add(new Unit { Id = 1, Name = "kg", UnitType = 0 });
            ctx.ShoppingLists.Add(new ShoppingList { Id = 1, Name = "List1", ShopId = 5, UserId = "user1" });
            ctx.ShoppingListProducts.AddRange(
                new ShoppingListProduct { ShoppingListId = 1, ProductId = 10, UnitId = 1, Quantity = 1m },
                new ShoppingListProduct { ShoppingListId = 1, ProductId = 20, UnitId = 1, Quantity = 2m });
            await ctx.SaveChangesAsync();

            var entity = await repo.GetByIdIncludeProductsAsync(1, CancellationToken.None);
            entity!.Products = [new ShoppingListProduct { ShoppingListId = 1, ProductId = 10, UnitId = 1, Quantity = 1m }];

            // Act
            await repo.UpdateAsync(entity, CancellationToken.None);

            // Assert
            var rows = ctx.ShoppingListProducts.Where(p => p.ShoppingListId == 1).ToList();
            Assert.That(rows, Has.Count.EqualTo(1));
            Assert.That(rows.Single().ProductId, Is.EqualTo(10));
        }

        [Test]
        public async Task UpdateAsync_UpdatesMutableColumns_ForExistingProduct()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);
            ctx.Shops.Add(new Shop { Id = 5, Name = "Shop" });
            ctx.Units.AddRange(
                new Unit { Id = 1, Name = "kg", UnitType = 0 },
                new Unit { Id = 2, Name = "g", UnitType = 0 });
            ctx.ShoppingLists.Add(new ShoppingList { Id = 1, Name = "List1", ShopId = 5, UserId = "user1" });
            ctx.ShoppingListProducts.Add(new ShoppingListProduct
            {
                ShoppingListId = 1,
                ProductId = 10,
                UnitId = 1,
                Quantity = 1m,
                Collected = false,
                DisplaySequence = 1
            });
            await ctx.SaveChangesAsync();

            var entity = await repo.GetByIdIncludeProductsAsync(1, CancellationToken.None);
            entity!.Products =
            [
                new ShoppingListProduct
                {
                    ShoppingListId = 1,
                    ProductId = 10,
                    UnitId = 2,
                    Quantity = 5m,
                    Collected = true,
                    DisplaySequence = 3
                }
            ];

            // Act
            await repo.UpdateAsync(entity, CancellationToken.None);

            // Assert
            var row = ctx.ShoppingListProducts.Single(p => p.ShoppingListId == 1 && p.ProductId == 10);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(row.Quantity, Is.EqualTo(5m));
                Assert.That(row.UnitId, Is.EqualTo(2));
                Assert.That(row.Collected, Is.True);
                Assert.That(row.DisplaySequence, Is.EqualTo(3));
            }
        }

        [Test]
        public async Task UpdateAsync_NullEntity_Throws()
        {
            // Arrange
            var repo = CreateRepository(out _);

            // Act / Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await repo.UpdateAsync(null!, CancellationToken.None));
        }
    }
}
