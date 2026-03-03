using Common.Data.DataContext;
using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeBook.Api.Repositories;

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
            var added = await repo.AddAsync(category);

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
            var all = await repo.GetAllAsync();

            // Assert
            Assert.That(all, Has.Count.EqualTo(2));
            Assert.That(all.Select(c => c.Name), Is.EquivalentTo(["Dairy", "Snacks"]));
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
            var found = await repo.GetByIdAsync(cat.Id);

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
            await repo.DeleteAsync(cat);

            // Assert
            Assert.That(ctx.ProductCategories.Any(), Is.False);
        }
    }
}