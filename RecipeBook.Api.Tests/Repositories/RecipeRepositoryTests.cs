using Common.Data.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeBook.Api.Repositories;
using RecipeBook.Data.Entities;

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

        private static Guid RecipeCategoryGuid(int seed) => new(seed, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        private static Recipe CreateRecipeGraph(
            int id,
            string name,
            Guid categoryId,
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

            var r1 = CreateRecipeGraph(1, "R1", RecipeCategoryGuid(10), "Main");
            var r2 = CreateRecipeGraph(2, "R2", RecipeCategoryGuid(20), "Dessert");
            ctx.Recipes.AddRange(r1, r2);
            await ctx.SaveChangesAsync();

            // Act
            var all = await repo.GetAllAsync(CancellationToken.None);

            // Assert
            Assert.That(all, Has.Count.EqualTo(2));
            Assert.That(all.All(r => r.RecipeCategory != null), Is.True);
        }

        [Test]
        public async Task GetByIdAsync_ReturnsRecipe_WithCategoryIncluded()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var r1 = CreateRecipeGraph(1, "R1", RecipeCategoryGuid(10), "Main");
            ctx.Recipes.Add(r1);
            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdAsync(1, CancellationToken.None);

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

            var recipe = CreateRecipeGraph(1, "R1", RecipeCategoryGuid(10), "Main");
            ctx.Recipes.Add(recipe);
            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdIncludeIngredientsAsync(1, CancellationToken.None);

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
            var found = await repo.GetByIdIncludeIngredientsAsync(null, CancellationToken.None);

            // Assert
            Assert.That(found, Is.Null);
        }

        // ---------- SearchAsync by category ----------
        [Test]
        public async Task SearchAsync_ByCategoryId_ReturnsRecipesInThatCategory()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var r1 = CreateRecipeGraph(1, "R1", RecipeCategoryGuid(10), "Main");
            var r2 = CreateRecipeGraph(2, "R2", RecipeCategoryGuid(20), "Dessert");
            ctx.Recipes.AddRange(r1, r2);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchAsync(RecipeCategoryGuid(10), CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Single().Name, Is.EqualTo("R1"));
        }

        // ---------- SearchAsync by name ----------
        [Test]
        public async Task SearchAsync_ByName_ReturnsMatchingRecipe()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var r1 = CreateRecipeGraph(1, "My Recipe", RecipeCategoryGuid(10), "Main");
            var r2 = CreateRecipeGraph(2, "Other", RecipeCategoryGuid(20), "Dessert");
            r1.UserId = "user1";
            r2.UserId = "user1";
            ctx.Recipes.AddRange(r1, r2);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchAsync("my recipe", "user1", CancellationToken.None);

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
            var result1 = await repo.SearchAsync((string)null!, "user1", CancellationToken.None);
            var result2 = await repo.SearchAsync("   ", "user1", CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result1, Is.Null);
                Assert.That(result2, Is.Null);
            }
        }

        // ---------- UpdateAsync ----------
        [Test]
        public async Task UpdateAsync_AddsNewIngredient_ToRecipe()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);
            ctx.RecipeCategories.Add(new RecipeCategory { Id = RecipeCategoryGuid(10), Name = "Main", DisplaySequence = 1 });
            ctx.Units.Add(new Unit { Id = 1, Name = "kg", UnitType = 0 });
            ctx.Recipes.Add(new Recipe { Id = 1, Name = "R1", RecipeCategoryId = RecipeCategoryGuid(10) });
            ctx.RecipeIngredients.Add(new RecipeIngredient { RecipeId = 1, ProductId = 100, UnitId = 1, Quantity = 1m });
            await ctx.SaveChangesAsync();

            var entity = await repo.GetByIdIncludeIngredientsAsync(1, CancellationToken.None);
            entity!.RecipeIngredients =
            [
                new RecipeIngredient { RecipeId = 1, ProductId = 100, UnitId = 1, Quantity = 1m },
                new RecipeIngredient { RecipeId = 1, ProductId = 200, UnitId = 1, Quantity = 2m }
            ];

            // Act
            await repo.UpdateAsync(entity, CancellationToken.None);

            // Assert
            var rows = ctx.RecipeIngredients.Where(ri => ri.RecipeId == 1).ToList();
            Assert.That(rows, Has.Count.EqualTo(2));
            Assert.That(rows.Select(ri => ri.ProductId), Is.EquivalentTo(new[] { 100, 200 }));
        }

        [Test]
        public async Task UpdateAsync_RemovesDeletedIngredient_FromRecipe()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);
            ctx.RecipeCategories.Add(new RecipeCategory { Id = RecipeCategoryGuid(10), Name = "Main", DisplaySequence = 1 });
            ctx.Units.Add(new Unit { Id = 1, Name = "kg", UnitType = 0 });
            ctx.Recipes.Add(new Recipe { Id = 1, Name = "R1", RecipeCategoryId = RecipeCategoryGuid(10) });
            ctx.RecipeIngredients.AddRange(
                new RecipeIngredient { RecipeId = 1, ProductId = 100, UnitId = 1, Quantity = 1m },
                new RecipeIngredient { RecipeId = 1, ProductId = 200, UnitId = 1, Quantity = 2m });
            await ctx.SaveChangesAsync();

            var entity = await repo.GetByIdIncludeIngredientsAsync(1, CancellationToken.None);
            entity!.RecipeIngredients = [new RecipeIngredient { RecipeId = 1, ProductId = 100, UnitId = 1, Quantity = 1m }];

            // Act
            await repo.UpdateAsync(entity, CancellationToken.None);

            // Assert
            var rows = ctx.RecipeIngredients.Where(ri => ri.RecipeId == 1).ToList();
            Assert.That(rows, Has.Count.EqualTo(1));
            Assert.That(rows.Single().ProductId, Is.EqualTo(100));
        }

        [Test]
        public async Task UpdateAsync_UpdatesQuantityAndUnit_ForExistingIngredient()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);
            ctx.RecipeCategories.Add(new RecipeCategory { Id = RecipeCategoryGuid(10), Name = "Main", DisplaySequence = 1 });
            ctx.Units.AddRange(
                new Unit { Id = 1, Name = "kg", UnitType = 0 },
                new Unit { Id = 2, Name = "g", UnitType = 0 });
            ctx.Recipes.Add(new Recipe { Id = 1, Name = "R1", RecipeCategoryId = RecipeCategoryGuid(10) });
            ctx.RecipeIngredients.Add(new RecipeIngredient { RecipeId = 1, ProductId = 100, UnitId = 1, Quantity = 1m });
            await ctx.SaveChangesAsync();

            var entity = await repo.GetByIdIncludeIngredientsAsync(1, CancellationToken.None);
            entity!.RecipeIngredients = [new RecipeIngredient { RecipeId = 1, ProductId = 100, UnitId = 2, Quantity = 500m }];

            // Act
            await repo.UpdateAsync(entity, CancellationToken.None);

            // Assert
            var row = ctx.RecipeIngredients.Single(ri => ri.RecipeId == 1 && ri.ProductId == 100);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(row.Quantity, Is.EqualTo(500m));
                Assert.That(row.UnitId, Is.EqualTo(2));
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