using Common.Data.DataContext;
using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Tests.Repositories
{
    [TestFixture]
    public class RecipeRepositoryTests
    {
        private ServiceProvider _provider = null!;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();

            services.AddDbContext<MealPlannerDbContext>(options =>
                options.UseInMemoryDatabase("RecipeRepositoryTests_" + TestContext.CurrentContext.Test.ID));

            services.AddScoped<RecipeRepository>();

            _provider = services.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

        private RecipeRepository CreateRepository(out MealPlannerDbContext context)
        {
            context = _provider.GetRequiredService<MealPlannerDbContext>();
            return new RecipeRepository(context);
        }

        private static Recipe CreateRecipeGraph(
            int id,
            string name,
            int categoryId,
            string categoryName)
        {
            var category = new RecipeCategory
            {
                Id = categoryId,          
                Name = categoryName,
                DisplaySequence = 1
            };

            return new Recipe
            {
                Id = id,
                Name = name,
                RecipeCategoryId = categoryId,
                RecipeCategory = category,
                RecipeIngredients =
                [
                    new()
                    {
                        Quantity = 1,
                        Unit = new Unit { Name = "kg", UnitType = 0 }, // no explicit Id
                        Product = new Product
                        {
                            Name = "Flour",
                            BaseUnit = new Unit { Name = "g", UnitType = 0 }, // no explicit Id
                            ProductCategory = new ProductCategory { Name = "Baking" } // no explicit Id
                        }
                    }
                ]
            };
        }

        // ---------- GetAllAsync / GetByIdAsync ----------
        [Test]
        public async Task GetAllAsync_ReturnsAllRecipes_WithCategoryIncluded()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var r1 = CreateRecipeGraph(1, "R1", 10, "Main");
            var r2 = CreateRecipeGraph(2, "R2", 20, "Dessert");
            ctx.Recipes.AddRange(r1, r2);
            await ctx.SaveChangesAsync();

            // Act
            var all = await repo.GetAllAsync();

            // Assert
            Assert.That(all, Has.Count.EqualTo(2));
            Assert.That(all.All(r => r.RecipeCategory != null), Is.True);
        }

        [Test]
        public async Task GetByIdAsync_ReturnsRecipe_WithCategoryIncluded()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var r1 = CreateRecipeGraph(1, "R1", 10, "Main");
            ctx.Recipes.Add(r1);
            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdAsync(1);

            // Assert
            Assert.That(found, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(found!.Name, Is.EqualTo("R1"));
                Assert.That(found.RecipeCategory, Is.Not.Null);
            }
            Assert.That(found.RecipeCategory!.Name, Is.EqualTo("Main"));
        }

        // ---------- GetByIdIncludeIngredientsAsync ----------
        [Test]
        public async Task GetByIdIncludeIngredientsAsync_ReturnsRecipe_WithIngredientsAndIncludes()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var recipe = CreateRecipeGraph(1, "R1", 10, "Main");
            ctx.Recipes.Add(recipe);
            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdIncludeIngredientsAsync(1);

            // Assert
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.RecipeIngredients, Is.Not.Null);
            Assert.That(found!.RecipeIngredients!, Has.Count.EqualTo(1));

            var ingredient = found.RecipeIngredients!.Single();
            Assert.That(ingredient.Product, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(ingredient.Product!.ProductCategory, Is.Not.Null);
                Assert.That(ingredient.Product!.BaseUnit, Is.Not.Null);
                Assert.That(ingredient.Unit, Is.Not.Null);
            }
        }

        [Test]
        public async Task GetByIdIncludeIngredientsAsync_NullId_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(out _);

            // Act
            var found = await repo.GetByIdIncludeIngredientsAsync(null);

            // Assert
            Assert.That(found, Is.Null);
        }

        // ---------- SearchAsync by category ----------
        [Test]
        public async Task SearchAsync_ByCategoryId_ReturnsRecipesInThatCategory()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var r1 = CreateRecipeGraph(1, "R1", 10, "Main");
            var r2 = CreateRecipeGraph(2, "R2", 20, "Dessert");
            ctx.Recipes.AddRange(r1, r2);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchAsync(10);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Single().Name, Is.EqualTo("R1"));
        }

        // ---------- SearchAsync by name ----------
        [Test]
        public async Task SearchAsync_ByName_ReturnsMatchingRecipe_CaseInsensitive()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var r1 = CreateRecipeGraph(1, "My Recipe", 10, "Main");
            var r2 = CreateRecipeGraph(2, "Other", 20, "Dessert");
            ctx.Recipes.AddRange(r1, r2);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchAsync("my recipe");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task SearchAsync_ByName_NullOrWhitespace_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(out _);

            // Act
            var result1 = await repo.SearchAsync((string)null!);
            var result2 = await repo.SearchAsync("   ");

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result1, Is.Null);
                Assert.That(result2, Is.Null);
            }
        }
    }
}