using Common.Data.DataContext;
using MealPlanner.Api.Repositories;
using MealPlanner.Data.Entities;
using MealPlanner.Data.TableConfigurations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeBook.Data.Entities;
using RecipeBook.Data.TableConfigurations;

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

        // SQLite is required for tests that use ExecuteDeleteAsync (unsupported by InMemory).
        private static (ShoppingListRepository repo, MealPlannerDbContext ctx, SqliteConnection connection) CreateSqliteRepository()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            var assemblies = new TableConfigurationAssemblies([
                typeof(RecipeTableConfiguration).Assembly,
                typeof(MealPlanTableConfiguration).Assembly
            ]);
            var options = new DbContextOptionsBuilder<MealPlannerDbContext>()
                .UseSqlite(connection)
                .Options;

            var ctx = new MealPlannerDbContext(options, assemblies);
            return (new ShoppingListRepository(ctx), ctx, connection);
        }

        // Deterministic mapping so a given int seed always maps to the same Guid,
        // preserving the linkage between ShoppingList.Id and ShoppingListProduct.ShoppingListId.
        private static Guid ShoppingListGuid(int seed) => new(seed, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        private static Guid UnitGuid(int seed) => new(seed * 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        private static Guid ProductGuid(int seed) => new(seed * 1000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        private static ShoppingList CreateShoppingListGraph(
            Guid id,
            string name,
            string shopName)
        {
            var category = new ProductCategory
            {
                Id = Guid.NewGuid(),
                Name = "Category1"
            };

            var baseUnit = new Unit
            {
                Id = UnitGuid(1),
                Name = "kg",
                UnitType = 0
            };

            var product = new Product
            {
                Id = ProductGuid(10),
                Name = "Flour",
                ProductCategoryId = category.Id,
                ProductCategory = category,
                BaseUnitId = baseUnit.Id,
                BaseUnit = baseUnit
            };

            var unit = new Unit
            {
                Id = UnitGuid(2),
                Name = "g",
                UnitType = 0
            };

            var shop = new Shop
            {
                Id = Guid.NewGuid(),
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

        // ---------- GetAllByUserAsync ----------
        [Test]
        public async Task GetAllByUserAsync_ReturnsOnlyShoppingListsForThatUser()
        {
            var repo = CreateRepository(out var ctx);

            ctx.ShoppingLists.AddRange(
                new ShoppingList { Id = ShoppingListGuid(1), Name = "List1", UserId = "user1" },
                new ShoppingList { Id = ShoppingListGuid(2), Name = "List2", UserId = "user1" },
                new ShoppingList { Id = ShoppingListGuid(3), Name = "List3", UserId = "user2" });
            await ctx.SaveChangesAsync();

            var result = await repo.GetAllByUserAsync("user1", CancellationToken.None);

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(l => l.Name), Is.EquivalentTo(["List1", "List2"]));
        }

        [Test]
        public async Task GetAllByUserAsync_NoMatches_ReturnsEmptyList()
        {
            var repo = CreateRepository(out var ctx);

            ctx.ShoppingLists.Add(new ShoppingList { Id = ShoppingListGuid(1), Name = "List1", UserId = "user2" });
            await ctx.SaveChangesAsync();

            var result = await repo.GetAllByUserAsync("user1", CancellationToken.None);

            Assert.That(result, Is.Empty);
        }

        // ---------- DeleteAsync ----------
        [Test]
        public async Task DeleteAsync_RemovesProductRowsAndShoppingList()
        {
            var (repo, ctx, connection) = CreateSqliteRepository();
            await using var _ = ctx;
            using var __ = connection;

            await ctx.Database.EnsureCreatedAsync();
            var shopId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            ctx.Shops.Add(new Shop { Id = shopId, Name = "Shop" });
            ctx.ProductCategories.Add(new ProductCategory { Id = categoryId, Name = "Cat1" });
            ctx.Units.Add(new Unit { Id = UnitGuid(1), Name = "kg", UnitType = 0 });
            ctx.Products.Add(new Product { Id = ProductGuid(10), Name = "Flour", ProductCategoryId = categoryId, BaseUnitId = UnitGuid(1) });
            ctx.ShoppingLists.Add(new ShoppingList { Id = ShoppingListGuid(1), Name = "List1", ShopId = shopId, UserId = "user1" });
            ctx.ShoppingListProducts.Add(
                new ShoppingListProduct { ShoppingListId = ShoppingListGuid(1), ProductId = ProductGuid(10), UnitId = UnitGuid(1), Quantity = 1m });
            await ctx.SaveChangesAsync();

            var entity = await repo.GetByIdIncludeProductsAsync(ShoppingListGuid(1), CancellationToken.None);
            Assert.That(entity, Is.Not.Null);

            // Act
            await repo.DeleteAsync(entity!, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(await ctx.ShoppingLists.AnyAsync(sl => sl.Id == ShoppingListGuid(1)), Is.False);
                Assert.That(await ctx.ShoppingListProducts.AnyAsync(p => p.ShoppingListId == ShoppingListGuid(1)), Is.False);
            }
        }

        // ---------- GetByIdIncludeProductsAsync ----------
        [Test]
        public async Task GetByIdIncludeProductsAsync_ReturnsShoppingList_WithShopProductsAndNavigationProps()
        {
            var repo = CreateRepository(out var ctx);

            var list = CreateShoppingListGraph(ShoppingListGuid(1), "Weekly", "MyShop");

            ctx.ProductCategories.Add(list.Products![0].Product!.ProductCategory!);
            ctx.Units.Add(list.Products[0].Product!.BaseUnit!);
            ctx.Units.Add(list.Products[0].Unit!);
            ctx.Products.Add(list.Products[0].Product!);
            ctx.Shops.Add(list.Shop!);
            ctx.ShoppingLists.Add(list);

            await ctx.SaveChangesAsync();

            var found = await repo.GetByIdIncludeProductsAsync(ShoppingListGuid(1), CancellationToken.None);

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

            var list = CreateShoppingListGraph(ShoppingListGuid(1), "Existing", "Shop");
            ctx.ShoppingLists.Add(list);
            await ctx.SaveChangesAsync();

            var found = await repo.GetByIdIncludeProductsAsync(ShoppingListGuid(999), CancellationToken.None);

            Assert.That(found, Is.Null);
        }

        // ---------- SearchAsync by name ----------
        [Test]
        public async Task SearchAsync_ByName_ReturnsMatchingList()
        {
            var repo = CreateRepository(out var ctx);

            var l1 = new ShoppingList { Id = ShoppingListGuid(1), Name = "Weekly", UserId = "user1" };
            var l2 = new ShoppingList { Id = ShoppingListGuid(2), Name = "Other", UserId = "user1" };
            ctx.ShoppingLists.AddRange(l1, l2);
            await ctx.SaveChangesAsync();

            var result = await repo.SearchAsync("weekly", "user1", CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(ShoppingListGuid(1)));
        }

        [Test]
        public async Task SearchAsync_UnknownName_ReturnsNull()
        {
            var repo = CreateRepository(out var ctx);

            ctx.ShoppingLists.Add(new ShoppingList { Id = ShoppingListGuid(1), Name = "Weekly", UserId = "user1" });
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
            var shopId = Guid.NewGuid();
            ctx.Shops.Add(new Shop { Id = shopId, Name = "Shop" });
            ctx.Units.Add(new Unit { Id = UnitGuid(1), Name = "kg", UnitType = 0 });
            ctx.ShoppingLists.Add(new ShoppingList { Id = ShoppingListGuid(1), Name = "List1", ShopId = shopId, UserId = "user1" });
            ctx.ShoppingListProducts.Add(
                new ShoppingListProduct { ShoppingListId = ShoppingListGuid(1), ProductId = ProductGuid(10), UnitId = UnitGuid(1), Quantity = 1m });
            await ctx.SaveChangesAsync();

            var entity = await repo.GetByIdIncludeProductsAsync(ShoppingListGuid(1), CancellationToken.None);
            entity!.Products =
            [
                new ShoppingListProduct { ShoppingListId = ShoppingListGuid(1), ProductId = ProductGuid(10), UnitId = UnitGuid(1), Quantity = 1m },
                new ShoppingListProduct { ShoppingListId = ShoppingListGuid(1), ProductId = ProductGuid(20), UnitId = UnitGuid(1), Quantity = 3m }
            ];

            // Act
            await repo.UpdateAsync(entity, CancellationToken.None);

            // Assert
            var rows = ctx.ShoppingListProducts.Where(p => p.ShoppingListId == ShoppingListGuid(1)).ToList();
            Assert.That(rows, Has.Count.EqualTo(2));
            Assert.That(rows.Select(p => p.ProductId), Is.EquivalentTo(new[] { ProductGuid(10), ProductGuid(20) }));
        }

        [Test]
        public async Task UpdateAsync_RemovesDeletedProduct_FromShoppingList()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);
            var shopId = Guid.NewGuid();
            ctx.Shops.Add(new Shop { Id = shopId, Name = "Shop" });
            ctx.Units.Add(new Unit { Id = UnitGuid(1), Name = "kg", UnitType = 0 });
            ctx.ShoppingLists.Add(new ShoppingList { Id = ShoppingListGuid(1), Name = "List1", ShopId = shopId, UserId = "user1" });
            ctx.ShoppingListProducts.AddRange(
                new ShoppingListProduct { ShoppingListId = ShoppingListGuid(1), ProductId = ProductGuid(10), UnitId = UnitGuid(1), Quantity = 1m },
                new ShoppingListProduct { ShoppingListId = ShoppingListGuid(1), ProductId = ProductGuid(20), UnitId = UnitGuid(1), Quantity = 2m });
            await ctx.SaveChangesAsync();

            var entity = await repo.GetByIdIncludeProductsAsync(ShoppingListGuid(1), CancellationToken.None);
            entity!.Products = [new ShoppingListProduct { ShoppingListId = ShoppingListGuid(1), ProductId = ProductGuid(10), UnitId = UnitGuid(1), Quantity = 1m }];

            // Act
            await repo.UpdateAsync(entity, CancellationToken.None);

            // Assert
            var rows = ctx.ShoppingListProducts.Where(p => p.ShoppingListId == ShoppingListGuid(1)).ToList();
            Assert.That(rows, Has.Count.EqualTo(1));
            Assert.That(rows.Single().ProductId, Is.EqualTo(ProductGuid(10)));
        }

        [Test]
        public async Task UpdateAsync_UpdatesMutableColumns_ForExistingProduct()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);
            var shopId = Guid.NewGuid();
            ctx.Shops.Add(new Shop { Id = shopId, Name = "Shop" });
            ctx.Units.AddRange(
                new Unit { Id = UnitGuid(1), Name = "kg", UnitType = 0 },
                new Unit { Id = UnitGuid(2), Name = "g", UnitType = 0 });
            ctx.ShoppingLists.Add(new ShoppingList { Id = ShoppingListGuid(1), Name = "List1", ShopId = shopId, UserId = "user1" });
            ctx.ShoppingListProducts.Add(new ShoppingListProduct
            {
                ShoppingListId = ShoppingListGuid(1),
                ProductId = ProductGuid(10),
                UnitId = UnitGuid(1),
                Quantity = 1m,
                Collected = false,
                DisplaySequence = 1
            });
            await ctx.SaveChangesAsync();

            var entity = await repo.GetByIdIncludeProductsAsync(ShoppingListGuid(1), CancellationToken.None);
            entity!.Products =
            [
                new ShoppingListProduct
                {
                    ShoppingListId = ShoppingListGuid(1),
                    ProductId = ProductGuid(10),
                    UnitId = UnitGuid(2),
                    Quantity = 5m,
                    Collected = true,
                    DisplaySequence = 3
                }
            ];

            // Act
            await repo.UpdateAsync(entity, CancellationToken.None);

            // Assert
            var row = ctx.ShoppingListProducts.Single(p => p.ShoppingListId == ShoppingListGuid(1) && p.ProductId == ProductGuid(10));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(row.Quantity, Is.EqualTo(5m));
                Assert.That(row.UnitId, Is.EqualTo(UnitGuid(2)));
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
