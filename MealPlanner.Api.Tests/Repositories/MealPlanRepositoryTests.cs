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
    public class MealPlanRepositoryTests
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
                options.UseInMemoryDatabase("MealPlanRepositoryTests_" + TestContext.CurrentContext.Test.ID));

            services.AddScoped<MealPlanRepository>();

            _provider = services.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

        private MealPlanRepository CreateRepository(out MealPlannerDbContext context)
        {
            context = _provider.GetRequiredService<MealPlannerDbContext>();
            return new MealPlanRepository(context);
        }

        // SQLite is required for tests that use ExecuteDeleteAsync (unsupported by InMemory).
        private static (MealPlanRepository repo, MealPlannerDbContext ctx, SqliteConnection connection) CreateSqliteRepository()
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
            return (new MealPlanRepository(ctx), ctx, connection);
        }

        // Deterministic mapping so a given int seed always maps to the same Guid,
        // preserving the linkage between MealPlan.Id and MealPlanRecipe.MealPlanId.
        private static Guid MealPlanId(int seed) => new(seed, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        // Maps the int seed used throughout these tests to a deterministic ProductCategory Guid, so
        // call sites can keep passing simple seeds while the entity uses a uniqueidentifier key.
        private static Guid ProductCategoryGuid(int seed) => new(seed, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        private static (MealPlan plan, Recipe recipe, Product product) CreateMealPlanGraph(
            Guid mealPlanId,
            string mealPlanName,
            int recipeId,
            string recipeName,
            int recipeCategoryId,
            string recipeCategoryName,
            int productCategoryId,
            string productCategoryName,
            int baseUnitId,
            int ingredientUnitId)
        {
            var recipeCategory = new RecipeCategory
            {
                Id = recipeCategoryId,
                Name = recipeCategoryName,
                DisplaySequence = 1
            };

            var productCategory = new ProductCategory
            {
                Id = ProductCategoryGuid(productCategoryId),
                Name = productCategoryName
            };

            var baseUnit = new Unit
            {
                Id = baseUnitId,
                Name = "kg",
                UnitType = 0
            };

            var ingredientUnit = new Unit
            {
                Id = ingredientUnitId,
                Name = "g",
                UnitType = 0
            };

            var product = new Product
            {
                Id = productCategoryId * 10,
                Name = "Flour",
                ProductCategoryId = productCategory.Id,
                ProductCategory = productCategory,
                BaseUnitId = baseUnit.Id,
                BaseUnit = baseUnit
            };

            var ingredient = new RecipeIngredient
            {
                RecipeId = recipeId,
                Quantity = 1m,
                UnitId = ingredientUnit.Id,
                Unit = ingredientUnit,
                ProductId = product.Id,
                Product = product
            };

            var recipe = new Recipe
            {
                Id = recipeId,
                Name = recipeName,
                RecipeCategoryId = recipeCategory.Id,
                RecipeCategory = recipeCategory,
                RecipeIngredients = [ingredient]
            };

            var mealPlan = new MealPlan
            {
                Id = mealPlanId,
                Name = mealPlanName,
                UserId = "user1",
                MealPlanRecipes =
                [
                    new()
                    {
                        MealPlanId = mealPlanId,
                        RecipeId = recipeId,
                        Recipe = recipe
                    }
                ]
            };

            foreach (var mpr in mealPlan.MealPlanRecipes!)
            {
                mpr.MealPlan = mealPlan;
            }

            return (mealPlan, recipe, product);
        }

        // ---------- GetByIdAsync override ----------
        [Test]
        public async Task GetByIdAsync_ReturnsMealPlan_WithRecipesAndCategoriesIncluded()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var (plan, recipe, product) = CreateMealPlanGraph(
                mealPlanId: MealPlanId(1),
                mealPlanName: "Plan1",
                recipeId: 10,
                recipeName: "R1",
                recipeCategoryId: 5,
                recipeCategoryName: "Main",
                productCategoryId: 20,
                productCategoryName: "Baking",
                baseUnitId: 1,
                ingredientUnitId: 2);

            ctx.RecipeCategories.Add(recipe.RecipeCategory!);
            ctx.ProductCategories.Add(product.ProductCategory!);
            ctx.Units.AddRange(product.BaseUnit!, recipe.RecipeIngredients!.Single().Unit!);
            ctx.Products.Add(product);
            ctx.Recipes.Add(recipe);
            ctx.MealPlans.Add(plan);
            ctx.MealPlanRecipes.AddRange(plan.MealPlanRecipes!);
            ctx.RecipeIngredients.AddRange(recipe.RecipeIngredients!);

            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdAsync(MealPlanId(1), CancellationToken.None);

            // Assert
            Assert.That(found, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(found!.Name, Is.EqualTo("Plan1"));
                Assert.That(found.MealPlanRecipes, Is.Not.Null);
            }
            Assert.That(found.MealPlanRecipes!, Has.Count.EqualTo(1));
            var mpr = found.MealPlanRecipes!.Single();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(mpr.Recipe, Is.Not.Null);
                Assert.That(mpr.Recipe!.RecipeCategory, Is.Not.Null);
                Assert.That(mpr.Recipe!.RecipeCategory!.Name, Is.EqualTo("Main"));
            }
        }

        [Test]
        public async Task GetByIdAsync_UnknownId_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var (plan, recipe, product) = CreateMealPlanGraph(
                MealPlanId(1), "Plan1", 10, "R1", 5, "Main", 20, "Baking", 1, 2);

            ctx.RecipeCategories.Add(recipe.RecipeCategory!);
            ctx.ProductCategories.Add(product.ProductCategory!);
            ctx.Units.AddRange(product.BaseUnit!, recipe.RecipeIngredients!.Single().Unit!);
            ctx.Products.Add(product);
            ctx.Recipes.Add(recipe);
            ctx.MealPlans.Add(plan);
            ctx.MealPlanRecipes.AddRange(plan.MealPlanRecipes!);
            ctx.RecipeIngredients.AddRange(recipe.RecipeIngredients!);
            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdAsync(MealPlanId(999), CancellationToken.None);

            // Assert
            Assert.That(found, Is.Null);
        }

        // ---------- GetByIdIncludeRecipesAsync ----------
        [Test]
        public async Task GetByIdIncludeRecipesAsync_ReturnsMealPlan_WithRecipesIngredientsAndNavProps()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var (plan, recipe, product) = CreateMealPlanGraph(
                mealPlanId: MealPlanId(1),
                mealPlanName: "Plan1",
                recipeId: 10,
                recipeName: "R1",
                recipeCategoryId: 5,
                recipeCategoryName: "Main",
                productCategoryId: 20,
                productCategoryName: "Baking",
                baseUnitId: 1,
                ingredientUnitId: 2);

            ctx.RecipeCategories.Add(recipe.RecipeCategory!);
            ctx.ProductCategories.Add(product.ProductCategory!);
            ctx.Units.AddRange(product.BaseUnit!, recipe.RecipeIngredients!.Single().Unit!);
            ctx.Products.Add(product);
            ctx.Recipes.Add(recipe);
            ctx.MealPlans.Add(plan);
            ctx.MealPlanRecipes.AddRange(plan.MealPlanRecipes!);
            ctx.RecipeIngredients.AddRange(recipe.RecipeIngredients!);

            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdIncludeRecipesAsync(MealPlanId(1), CancellationToken.None);

            // Assert
            Assert.That(found, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(found!.MealPlanRecipes, Is.Not.Null);
                Assert.That(found.MealPlanRecipes!, Has.Count.EqualTo(1));
            }

            var mpr = found.MealPlanRecipes!.Single();
            var r = mpr.Recipe;
            Assert.That(r, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(r!.RecipeIngredients, Is.Not.Null);
                Assert.That(r.RecipeIngredients!, Has.Count.EqualTo(1));
            }

            var ing = r.RecipeIngredients!.Single();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(ing.Unit, Is.Not.Null);
                Assert.That(ing.Product, Is.Not.Null);
                Assert.That(ing.Product!.ProductCategory, Is.Not.Null);
                Assert.That(ing.Product!.BaseUnit, Is.Not.Null);
            }
        }

        [Test]
        public async Task GetByIdIncludeRecipesAsync_UnknownId_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var (plan, recipe, product) = CreateMealPlanGraph(
                MealPlanId(1), "Plan1", 10, "R1", 5, "Main", 20, "Baking", 1, 2);

            ctx.RecipeCategories.Add(recipe.RecipeCategory!);
            ctx.ProductCategories.Add(product.ProductCategory!);
            ctx.Units.AddRange(product.BaseUnit!, recipe.RecipeIngredients!.Single().Unit!);
            ctx.Products.Add(product);
            ctx.Recipes.Add(recipe);
            ctx.MealPlans.Add(plan);
            ctx.MealPlanRecipes.AddRange(plan.MealPlanRecipes!);
            ctx.RecipeIngredients.AddRange(recipe.RecipeIngredients!);
            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdIncludeRecipesAsync(MealPlanId(999), CancellationToken.None);

            // Assert
            Assert.That(found, Is.Null);
        }

        // ---------- SearchByRecipeCategoryIdsAsync ----------
        [Test]
        public async Task SearchByRecipeCategoryIdsAsync_WithIds_ReturnsMatchingMealPlanRecipes()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var (plan1, recipe1, product1) = CreateMealPlanGraph(
                MealPlanId(1), "Plan1", 10, "R1", 5, "Main", 20, "Baking", 1, 2);
            var (plan2, recipe2, product2) = CreateMealPlanGraph(
                MealPlanId(2), "Plan2", 11, "R2", 6, "Dessert", 21, "Fruit", 3, 4);

            ctx.RecipeCategories.AddRange(recipe1.RecipeCategory!, recipe2.RecipeCategory!);
            ctx.ProductCategories.AddRange(product1.ProductCategory!, product2.ProductCategory!);
            ctx.Units.AddRange(
                product1.BaseUnit!, recipe1.RecipeIngredients!.Single().Unit!,
                product2.BaseUnit!, recipe2.RecipeIngredients!.Single().Unit!);
            ctx.Products.AddRange(product1, product2);
            ctx.Recipes.AddRange(recipe1, recipe2);
            ctx.MealPlans.AddRange(plan1, plan2);
            ctx.MealPlanRecipes.AddRange(plan1.MealPlanRecipes!);
            ctx.MealPlanRecipes.AddRange(plan2.MealPlanRecipes!);
            ctx.RecipeIngredients.AddRange(recipe1.RecipeIngredients!);
            ctx.RecipeIngredients.AddRange(recipe2.RecipeIngredients!);

            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchByRecipeCategoryIdsAsync([5], "user1", CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            var mpr = result.Single();
            Assert.That(mpr.Recipe, Is.Not.Null);
            Assert.That(mpr.Recipe!.RecipeCategoryId, Is.EqualTo(5));
        }

        [Test]
        public async Task SearchByRecipeCategoryIdsAsync_EmptyList_ReturnsEmpty()
        {
            // Arrange
            var repo = CreateRepository(out _);

            // Act
            var result = await repo.SearchByRecipeCategoryIdsAsync([], "user1", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        // ---------- SearchByProductCategoryIdsAsync ----------
        [Test]
        public async Task SearchByProductCategoryIdsAsync_ReturnsProductMealPlanPairs()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var (plan1, recipe1, product1) = CreateMealPlanGraph(
                MealPlanId(1), "Plan1", 10, "R1", 5, "Main", 20, "Baking", 1, 2);
            var (plan2, recipe2, product2) = CreateMealPlanGraph(
                MealPlanId(2), "Plan2", 11, "R2", 6, "Dessert", 21, "Fruit", 3, 4);

            ctx.RecipeCategories.AddRange(recipe1.RecipeCategory!, recipe2.RecipeCategory!);
            ctx.ProductCategories.AddRange(product1.ProductCategory!, product2.ProductCategory!);
            ctx.Units.AddRange(
                product1.BaseUnit!, recipe1.RecipeIngredients!.Single().Unit!,
                product2.BaseUnit!, recipe2.RecipeIngredients!.Single().Unit!);
            ctx.Products.AddRange(product1, product2);
            ctx.Recipes.AddRange(recipe1, recipe2);
            ctx.MealPlans.AddRange(plan1, plan2);
            ctx.MealPlanRecipes.AddRange(plan1.MealPlanRecipes!);
            ctx.MealPlanRecipes.AddRange(plan2.MealPlanRecipes!);
            ctx.RecipeIngredients.AddRange(recipe1.RecipeIngredients!);
            ctx.RecipeIngredients.AddRange(recipe2.RecipeIngredients!);

            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchByProductCategoryIdsAsync([ProductCategoryGuid(20)], "user1", CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            var kv = result.Single();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(kv.Key.ProductCategoryId, Is.EqualTo(ProductCategoryGuid(20)));
                Assert.That(kv.Value.Name, Is.EqualTo("Plan1"));
            }
        }

        [Test]
        public async Task SearchByProductCategoryIdsAsync_EmptyList_ReturnsEmpty()
        {
            // Arrange
            var repo = CreateRepository(out _);

            // Act
            var result = await repo.SearchByProductCategoryIdsAsync([], "user1", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        // ---------- SearchByRecipeAsync ----------
        [Test]
        public async Task SearchByRecipeAsync_ReturnsMealPlansForRecipe()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var (plan1, recipe1, product1) = CreateMealPlanGraph(
                MealPlanId(1), "Plan1", 10, "R1", 5, "Main", 20, "Baking", 1, 2);
            var (plan2, recipe2, product2) = CreateMealPlanGraph(
                MealPlanId(2), "Plan2", 11, "R2", 6, "Dessert", 21, "Fruit", 3, 4);

            ctx.RecipeCategories.AddRange(recipe1.RecipeCategory!, recipe2.RecipeCategory!);
            ctx.ProductCategories.AddRange(product1.ProductCategory!, product2.ProductCategory!);
            ctx.Units.AddRange(
                product1.BaseUnit!, recipe1.RecipeIngredients!.Single().Unit!,
                product2.BaseUnit!, recipe2.RecipeIngredients!.Single().Unit!);
            ctx.Products.AddRange(product1, product2);
            ctx.Recipes.AddRange(recipe1, recipe2);
            ctx.MealPlans.AddRange(plan1, plan2);
            ctx.MealPlanRecipes.AddRange(plan1.MealPlanRecipes!);
            ctx.MealPlanRecipes.AddRange(plan2.MealPlanRecipes!);
            ctx.RecipeIngredients.AddRange(recipe1.RecipeIngredients!);
            ctx.RecipeIngredients.AddRange(recipe2.RecipeIngredients!);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchByRecipeAsync(10, "user1", CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Single().Name, Is.EqualTo("Plan1"));
        }

        [Test]
        public async Task SearchByRecipeAsync_NoMatch_ReturnsEmpty()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var (plan1, recipe1, product1) = CreateMealPlanGraph(
                MealPlanId(1), "Plan1", 10, "R1", 5, "Main", 20, "Baking", 1, 2);
            ctx.RecipeCategories.Add(recipe1.RecipeCategory!);
            ctx.ProductCategories.Add(product1.ProductCategory!);
            ctx.Units.AddRange(product1.BaseUnit!, recipe1.RecipeIngredients!.Single().Unit!);
            ctx.Products.Add(product1);
            ctx.Recipes.Add(recipe1);
            ctx.MealPlans.Add(plan1);
            ctx.MealPlanRecipes.AddRange(plan1.MealPlanRecipes!);
            ctx.RecipeIngredients.AddRange(recipe1.RecipeIngredients!);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchByRecipeAsync(999, "user1", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Empty);
        }

        // ---------- SearchAsync by name ----------
        [Test]
        public async Task SearchAsync_ByName_ReturnsMatchingMealPlan()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var p1 = new MealPlan { Id = MealPlanId(1), Name = "Weekly", UserId = "user1" };
            var p2 = new MealPlan { Id = MealPlanId(2), Name = "Other", UserId = "user1" };
            ctx.MealPlans.AddRange(p1, p2);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchAsync("weekly", "user1", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(MealPlanId(1)));
        }

        [Test]
        public async Task SearchAsync_UnknownName_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.MealPlans.Add(new MealPlan { Id = MealPlanId(1), Name = "Weekly", UserId = "user1" });
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchAsync("does-not-exist", "user1", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task SearchAsync_NullOrWhitespace_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(out _);

            var r1 = await repo.SearchAsync(null!, "user1", CancellationToken.None);
            var r2 = await repo.SearchAsync("   ", "user1", CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(r1, Is.Null);
                Assert.That(r2, Is.Null);
            }
        }

        // ---------- DeleteAsync ----------

        [Test]
        public async Task DeleteAsync_RemovesJunctionRowsAndMealPlan()
        {
            var (repo, ctx, connection) = CreateSqliteRepository();
            await using var _ = ctx;
            using var __ = connection;

            await ctx.Database.EnsureCreatedAsync();
            ctx.RecipeCategories.Add(new RecipeCategory { Id = 1, Name = "Cat", DisplaySequence = 1 });
            ctx.Recipes.Add(new Recipe { Id = 10, Name = "R1", RecipeCategoryId = 1 });
            ctx.MealPlans.Add(new MealPlan { Id = MealPlanId(1), Name = "Plan", UserId = "u1" });
            ctx.MealPlanRecipes.Add(new MealPlanRecipe { MealPlanId = MealPlanId(1), RecipeId = 10 });
            await ctx.SaveChangesAsync();

            var entity = await repo.GetByIdAsync(MealPlanId(1), CancellationToken.None);
            Assert.That(entity, Is.Not.Null);

            // Act
            await repo.DeleteAsync(entity!, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(await ctx.MealPlans.AnyAsync(mp => mp.Id == MealPlanId(1)), Is.False);
                Assert.That(await ctx.MealPlanRecipes.AnyAsync(mpr => mpr.MealPlanId == MealPlanId(1)), Is.False);
            }
        }

        [Test]
        public async Task DeleteAsync_WhenRecipesShareCategory_DoesNotThrow()
        {
            // Regression: deleting a plan whose recipes share a RecipeCategory used to throw
            // InvalidOperationException due to EF identity-map conflicts on DbContext.Remove.
            var (repo, ctx, connection) = CreateSqliteRepository();
            await using var _ = ctx;
            using var __ = connection;

            await ctx.Database.EnsureCreatedAsync();
            ctx.RecipeCategories.Add(new RecipeCategory { Id = 1, Name = "Main", DisplaySequence = 1 });
            ctx.Recipes.AddRange(
                new Recipe { Id = 10, Name = "R1", RecipeCategoryId = 1 },
                new Recipe { Id = 11, Name = "R2", RecipeCategoryId = 1 });
            ctx.MealPlans.Add(new MealPlan { Id = MealPlanId(1), Name = "Plan", UserId = "u1" });
            ctx.MealPlanRecipes.AddRange(
                new MealPlanRecipe { MealPlanId = MealPlanId(1), RecipeId = 10 },
                new MealPlanRecipe { MealPlanId = MealPlanId(1), RecipeId = 11 });
            await ctx.SaveChangesAsync();

            var entity = await repo.GetByIdAsync(MealPlanId(1), CancellationToken.None);
            Assert.That(entity, Is.Not.Null);

            // Act & Assert — must not throw
            Assert.DoesNotThrowAsync(async () =>
                await repo.DeleteAsync(entity!, CancellationToken.None));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(await ctx.MealPlans.AnyAsync(mp => mp.Id == MealPlanId(1)), Is.False);
                Assert.That(await ctx.MealPlanRecipes.AnyAsync(mpr => mpr.MealPlanId == MealPlanId(1)), Is.False);
            }
        }

        // ---------- UpdateAsync ----------
        [Test]
        public async Task UpdateAsync_AddsNewRecipe_ToExistingMealPlan()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);
            ctx.MealPlans.Add(new MealPlan { Id = MealPlanId(1), Name = "Plan1", UserId = "user1" });
            ctx.MealPlanRecipes.Add(new MealPlanRecipe { MealPlanId = MealPlanId(1), RecipeId = 10 });
            await ctx.SaveChangesAsync();

            var entity = await repo.GetByIdAsync(MealPlanId(1), CancellationToken.None);
            entity!.MealPlanRecipes =
            [
                new MealPlanRecipe { MealPlanId = MealPlanId(1), RecipeId = 10 },
                new MealPlanRecipe { MealPlanId = MealPlanId(1), RecipeId = 11 }
            ];

            // Act
            await repo.UpdateAsync(entity, CancellationToken.None);

            // Assert
            var rows = ctx.MealPlanRecipes.Where(mpr => mpr.MealPlanId == MealPlanId(1)).ToList();
            Assert.That(rows, Has.Count.EqualTo(2));
            Assert.That(rows.Select(r => r.RecipeId), Is.EquivalentTo([10, 11]));
        }

        [Test]
        public async Task UpdateAsync_RemovesDeletedRecipe_FromMealPlan()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);
            ctx.MealPlans.Add(new MealPlan { Id = MealPlanId(1), Name = "Plan1", UserId = "user1" });
            ctx.MealPlanRecipes.AddRange(
                new MealPlanRecipe { MealPlanId = MealPlanId(1), RecipeId = 10 },
                new MealPlanRecipe { MealPlanId = MealPlanId(1), RecipeId = 11 });
            await ctx.SaveChangesAsync();

            var entity = await repo.GetByIdAsync(MealPlanId(1), CancellationToken.None);
            entity!.MealPlanRecipes = [new MealPlanRecipe { MealPlanId = MealPlanId(1), RecipeId = 10 }];

            // Act
            await repo.UpdateAsync(entity, CancellationToken.None);

            // Assert
            var rows = ctx.MealPlanRecipes.Where(mpr => mpr.MealPlanId == MealPlanId(1)).ToList();
            Assert.That(rows, Has.Count.EqualTo(1));
            Assert.That(rows.Single().RecipeId, Is.EqualTo(10));
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