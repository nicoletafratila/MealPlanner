using Common.Data.DataContext;
using MealPlanner.Data.TableConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeBook.Data.Entities;
using RecipeBook.Data.TableConfigurations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace RecipeBook.Api.Tests
{
    [TestFixture]
    public class ImageThumbnailBackfillTests
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
                options.UseInMemoryDatabase("ImageThumbnailBackfillTests_" + TestContext.CurrentContext.Test.ID));

            _provider = services.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

        private MealPlannerDbContext CreateContext() => _provider.GetRequiredService<MealPlannerDbContext>();

        private static byte[] CreateValidImageBytes()
        {
            using var image = new Image<Rgba32>(4, 4);
            using var ms = new MemoryStream();
            image.SaveAsPng(ms);
            return ms.ToArray();
        }

        [Test]
        public async Task EnsureBackfilledAsync_RecipeWithImageButNoThumbnail_GeneratesThumbnail()
        {
            var ctx = CreateContext();
            var recipe = new Recipe { Id = Guid.NewGuid(), Name = "R1", RecipeCategoryId = Guid.NewGuid(), ImageContent = CreateValidImageBytes() };
            ctx.Recipes.Add(recipe);
            await ctx.SaveChangesAsync();

            await ImageThumbnailBackfill.EnsureBackfilledAsync(ctx);

            var updated = await ctx.Recipes.AsNoTracking().SingleAsync(x => x.Id == recipe.Id);
            Assert.That(updated.ImageThumbnail, Is.Not.Null);
        }

        [Test]
        public async Task EnsureBackfilledAsync_ProductWithImageButNoThumbnail_GeneratesThumbnail()
        {
            var ctx = CreateContext();
            var product = new Product { Id = Guid.NewGuid(), Name = "P1", ImageContent = CreateValidImageBytes() };
            ctx.Products.Add(product);
            await ctx.SaveChangesAsync();

            await ImageThumbnailBackfill.EnsureBackfilledAsync(ctx);

            var updated = await ctx.Products.AsNoTracking().SingleAsync(x => x.Id == product.Id);
            Assert.That(updated.ImageThumbnail, Is.Not.Null);
        }

        [Test]
        public async Task EnsureBackfilledAsync_RecipeWithoutImageContent_LeavesThumbnailNull()
        {
            var ctx = CreateContext();
            var recipe = new Recipe { Id = Guid.NewGuid(), Name = "R1", RecipeCategoryId = Guid.NewGuid(), ImageContent = null };
            ctx.Recipes.Add(recipe);
            await ctx.SaveChangesAsync();

            await ImageThumbnailBackfill.EnsureBackfilledAsync(ctx);

            var updated = await ctx.Recipes.AsNoTracking().SingleAsync(x => x.Id == recipe.Id);
            Assert.That(updated.ImageThumbnail, Is.Null);
        }

        [Test]
        public async Task EnsureBackfilledAsync_RecipeAlreadyHasThumbnail_LeavesItUnchanged()
        {
            var ctx = CreateContext();
            var existingThumbnail = new byte[] { 9, 9, 9 };
            var recipe = new Recipe
            {
                Id = Guid.NewGuid(),
                Name = "R1",
                RecipeCategoryId = Guid.NewGuid(),
                ImageContent = CreateValidImageBytes(),
                ImageThumbnail = existingThumbnail
            };
            ctx.Recipes.Add(recipe);
            await ctx.SaveChangesAsync();

            await ImageThumbnailBackfill.EnsureBackfilledAsync(ctx);

            var updated = await ctx.Recipes.AsNoTracking().SingleAsync(x => x.Id == recipe.Id);
            Assert.That(updated.ImageThumbnail, Is.EqualTo(existingThumbnail));
        }

        [Test]
        public async Task EnsureBackfilledAsync_MoreRowsThanOneBatch_BackfillsAllRows()
        {
            var ctx = CreateContext();
            var recipeIds = new List<Guid>();
            for (var i = 0; i < 60; i++)
            {
                var id = Guid.NewGuid();
                recipeIds.Add(id);
                ctx.Recipes.Add(new Recipe { Id = id, Name = $"R{i}", RecipeCategoryId = Guid.NewGuid(), ImageContent = CreateValidImageBytes() });
            }
            await ctx.SaveChangesAsync();

            await ImageThumbnailBackfill.EnsureBackfilledAsync(ctx);

            var updated = await ctx.Recipes.AsNoTracking().Where(x => recipeIds.Contains(x.Id)).ToListAsync();
            Assert.That(updated.All(x => x.ImageThumbnail != null), Is.True);
        }

        [Test]
        public async Task EnsureBackfilledAsync_RunTwice_IsIdempotent()
        {
            var ctx = CreateContext();
            var recipe = new Recipe { Id = Guid.NewGuid(), Name = "R1", RecipeCategoryId = Guid.NewGuid(), ImageContent = CreateValidImageBytes() };
            ctx.Recipes.Add(recipe);
            await ctx.SaveChangesAsync();

            await ImageThumbnailBackfill.EnsureBackfilledAsync(ctx);
            var firstThumbnail = (await ctx.Recipes.AsNoTracking().SingleAsync(x => x.Id == recipe.Id)).ImageThumbnail;

            await ImageThumbnailBackfill.EnsureBackfilledAsync(ctx);
            var secondThumbnail = (await ctx.Recipes.AsNoTracking().SingleAsync(x => x.Id == recipe.Id)).ImageThumbnail;

            Assert.That(secondThumbnail, Is.EqualTo(firstThumbnail));
        }
    }
}
