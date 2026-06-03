using Common.Data.DataContext;
using MealPlanner.Api.Repositories;
using MealPlanner.Data.Entities;
using MealPlanner.Data.TableConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeBook.Data.Entities;
using RecipeBook.Data.TableConfigurations;

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

            services.AddSingleton(new TableConfigurationAssemblies([
                typeof(RecipeTableConfiguration).Assembly,
                typeof(MealPlanTableConfiguration).Assembly
            ]));

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

        // Deterministic mapping so a given int seed always maps to the same Guid,
        // preserving the linkage between Shop.Id and ShopDisplaySequence.ShopId.
        private static Guid ShopGuid(int seed) => new(seed, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        private static Shop CreateShopGraph(
            Guid id,
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
                id: ShopGuid(1),
                name: "TestShop",
                (1, 10),
                (2, 5));

            ctx.Shops.Add(shop);
            // Ensure related ProductCategory entries are tracked
            ctx.ProductCategories.AddRange(
                shop.DisplaySequence!.Select(ds => ds.ProductCategory!));
            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdIncludeDisplaySequenceAsync(ShopGuid(1), CancellationToken.None);

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

            var shop = CreateShopGraph(ShopGuid(1), "Existing");
            ctx.Shops.Add(shop);
            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdIncludeDisplaySequenceAsync(ShopGuid(999), CancellationToken.None);

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
                await repo.GetByIdIncludeDisplaySequenceAsync(null, CancellationToken.None));
        }

        // ---------- UpdateAsync ----------
        [Test]
        public async Task UpdateAsync_AddsNewDisplaySequence()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);
            ctx.ProductCategories.AddRange(
                new ProductCategory { Id = 1, Name = "Cat1" },
                new ProductCategory { Id = 2, Name = "Cat2" });
            ctx.Shops.Add(new Shop { Id = ShopGuid(1), Name = "Shop1" });
            ctx.ShopDisplaySequences.Add(new ShopDisplaySequence { ShopId = ShopGuid(1), ProductCategoryId = 1, Value = 10 });
            await ctx.SaveChangesAsync();

            var entity = await repo.GetByIdIncludeDisplaySequenceAsync(ShopGuid(1), CancellationToken.None);
            entity!.DisplaySequence =
            [
                new ShopDisplaySequence { ShopId = ShopGuid(1), ProductCategoryId = 1, Value = 10 },
                new ShopDisplaySequence { ShopId = ShopGuid(1), ProductCategoryId = 2, Value = 20 }
            ];

            // Act
            await repo.UpdateAsync(entity, CancellationToken.None);

            // Assert
            var rows = ctx.ShopDisplaySequences.Where(s => s.ShopId == ShopGuid(1)).ToList();
            Assert.That(rows, Has.Count.EqualTo(2));
            Assert.That(rows.Select(s => s.ProductCategoryId), Is.EquivalentTo(new[] { 1, 2 }));
        }

        [Test]
        public async Task UpdateAsync_RemovesDeletedDisplaySequence()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);
            ctx.ProductCategories.AddRange(
                new ProductCategory { Id = 1, Name = "Cat1" },
                new ProductCategory { Id = 2, Name = "Cat2" });
            ctx.Shops.Add(new Shop { Id = ShopGuid(1), Name = "Shop1" });
            ctx.ShopDisplaySequences.AddRange(
                new ShopDisplaySequence { ShopId = ShopGuid(1), ProductCategoryId = 1, Value = 10 },
                new ShopDisplaySequence { ShopId = ShopGuid(1), ProductCategoryId = 2, Value = 20 });
            await ctx.SaveChangesAsync();

            var entity = await repo.GetByIdIncludeDisplaySequenceAsync(ShopGuid(1), CancellationToken.None);
            entity!.DisplaySequence = [new ShopDisplaySequence { ShopId = ShopGuid(1), ProductCategoryId = 1, Value = 10 }];

            // Act
            await repo.UpdateAsync(entity, CancellationToken.None);

            // Assert
            var rows = ctx.ShopDisplaySequences.Where(s => s.ShopId == ShopGuid(1)).ToList();
            Assert.That(rows, Has.Count.EqualTo(1));
            Assert.That(rows.Single().ProductCategoryId, Is.EqualTo(1));
        }

        [Test]
        public async Task UpdateAsync_UpdatesValue_ForExistingCategory()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);
            ctx.ProductCategories.Add(new ProductCategory { Id = 1, Name = "Cat1" });
            ctx.Shops.Add(new Shop { Id = ShopGuid(1), Name = "Shop1" });
            ctx.ShopDisplaySequences.Add(new ShopDisplaySequence { ShopId = ShopGuid(1), ProductCategoryId = 1, Value = 10 });
            await ctx.SaveChangesAsync();

            var entity = await repo.GetByIdIncludeDisplaySequenceAsync(ShopGuid(1), CancellationToken.None);
            entity!.DisplaySequence = [new ShopDisplaySequence { ShopId = ShopGuid(1), ProductCategoryId = 1, Value = 99 }];

            // Act
            await repo.UpdateAsync(entity, CancellationToken.None);

            // Assert
            var row = ctx.ShopDisplaySequences.Single(s => s.ShopId == ShopGuid(1) && s.ProductCategoryId == 1);
            Assert.That(row.Value, Is.EqualTo(99));
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