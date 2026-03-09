using Common.Data.DataContext;
using Common.Data.Entities;
using MealPlanner.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

        private static (MealPlan plan, Recipe recipe, Product product) CreateMealPlanGraph(
            int mealPlanId,
            string mealPlanName,
            int recipeId,
            string recipeName,
            int recipeCategoryId,
            string recipeCategoryName,
            int productId,
            string productName,
            int productCategoryId,
            string productCategoryName,
            int baseUnitId,
            int ingredientUnitId)
        {
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
            var recipeCategory = new RecipeCategory
            {
                Id = recipeCategoryId,
                Name = recipeCategoryName,
                DisplaySequence = 1
            };

            var productCategory = new ProductCategory
            {
                Id = productCategoryId,
                Name = productCategoryName
            };

            var product = new Product
            {
                Id = productId,
                Name = productName,
                ProductCategoryId = productCategory.Id,
                ProductCategory = productCategory,
                BaseUnitId = baseUnit.Id,
                BaseUnit = baseUnit
            };

            var recipe = new Recipe
            {
                Id = recipeId,
                Name = recipeName,
                RecipeCategoryId = recipeCategory.Id,
                RecipeCategory = recipeCategory,
                RecipeIngredients =
                [
                    new RecipeIngredient
                    {
                        RecipeId = recipeId,
                        Quantity = 1m,
                        UnitId = ingredientUnit.Id,
                        Unit = ingredientUnit,
                        ProductId = product.Id,
                        Product = product
                    }
                ]
            };

            var mealPlan = new MealPlan
            {
                Id = mealPlanId,
                Name = mealPlanName,
                MealPlanRecipes =
                [
                    new()
                    {
                        MealPlanId = mealPlanId,
                        MealPlan = null!, // will be set below
                        RecipeId = recipeId,
                        Recipe = recipe
                    }
                ]
            };

            // fix back reference
            foreach (var mpr in mealPlan.MealPlanRecipes!)
                mpr.MealPlan = mealPlan;

            return (mealPlan, recipe, product);
        }

        // ---------- GetByIdAsync override ----------
        [Test]
        public async Task GetByIdAsync_ReturnsMealPlan_WithRecipesAndCategoriesIncluded()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var (plan, recipe, _) = CreateMealPlanGraph(
                mealPlanId: 1,
                mealPlanName: "Plan1",
                recipeId: 10,
                recipeName: "R1",
                recipeCategoryId: 5,
                recipeCategoryName: "Main",
                productId: 1,
                productName: "Flower",
                productCategoryId: 20,
                productCategoryName: "Baking",
                baseUnitId: 1,
                ingredientUnitId: 2);

            ctx.RecipeCategories.Add(recipe.RecipeCategory!);
            ctx.ProductCategories.Add(recipe.RecipeIngredients!.Single().Product!.ProductCategory!);
            ctx.Units.AddRange(
                recipe.RecipeIngredients!.Single().Product!.BaseUnit!,
                recipe.RecipeIngredients!.Single().Unit!);
            ctx.Products.Add(recipe.RecipeIngredients!.Single().Product!);
            ctx.Recipes.Add(recipe);
            ctx.MealPlans.Add(plan);
            ctx.MealPlanRecipes.AddRange(plan.MealPlanRecipes!);

            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdAsync(1);

            // Assert
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Plan1"));
            Assert.That(found.MealPlanRecipes, Is.Not.Null);
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

            var (plan, recipe, _) = CreateMealPlanGraph(1, "Plan1", 10, "R1", 5, "Main", 1, "Flower", 20, "Baking", 1, 2);
            ctx.MealPlans.Add(plan);
            ctx.Recipes.Add(recipe);
            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdAsync(999);

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
                mealPlanId: 1,
                mealPlanName: "Plan1",
                recipeId: 10,
                recipeName: "R1",
                recipeCategoryId: 5,
                recipeCategoryName: "Main",
                productId: 1,
                productName: "Flower",
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
            var found = await repo.GetByIdIncludeRecipesAsync(1);

            // Assert
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.MealPlanRecipes, Is.Not.Null);
            Assert.That(found.MealPlanRecipes!, Has.Count.EqualTo(1));

            var mpr = found.MealPlanRecipes!.Single();
            var r = mpr.Recipe;
            Assert.That(r, Is.Not.Null);
            Assert.That(r!.RecipeIngredients, Is.Not.Null);
            Assert.That(r.RecipeIngredients!, Has.Count.EqualTo(1));

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

            var (plan, recipe, _) = CreateMealPlanGraph(1, "Plan1", 10, "R1", 5, "Main", 1, "Flower", 20, "Baking", 1, 2);
            ctx.MealPlans.Add(plan);
            ctx.Recipes.Add(recipe);
            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdIncludeRecipesAsync(999);

            // Assert
            Assert.That(found, Is.Null);
        }

        // ---------- SearchByRecipeCategoryIdsAsync ----------
        [Test]
        public async Task SearchByRecipeCategoryIdsAsync_WithIds_ReturnsMatchingMealPlanRecipes()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var (plan1, recipe1, _) = CreateMealPlanGraph(1, "Plan1", 10, "R1", 5, "Main", 1, "Flower", 20, "Baking", 1, 2);
            var (plan2, recipe2, _) = CreateMealPlanGraph(2, "Plan2", 11, "R2", 6, "Dessert", 2, "Chocolate", 21, "Fruit", 3, 4);

            ctx.RecipeCategories.AddRange(recipe1.RecipeCategory!, recipe2.RecipeCategory!);
            ctx.Recipes.AddRange(recipe1, recipe2);
            ctx.MealPlans.AddRange(plan1, plan2);
            ctx.MealPlanRecipes.AddRange(plan1.MealPlanRecipes!);
            ctx.MealPlanRecipes.AddRange(plan2.MealPlanRecipes!);

            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchByRecipeCategoryIdsAsync([5]);

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
            var result = await repo.SearchByRecipeCategoryIdsAsync([]);

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

            var (plan1, recipe1, product1) = CreateMealPlanGraph(1, "Plan1", 10, "R1", 5, "Main", 1, "Flower", 20, "Baking", 1, 2);
            var (plan2, recipe2, product2) = CreateMealPlanGraph(2, "Plan2", 11, "R2", 6, "Dessert", 2, "Chocolate", 21, "Fruit", 3, 4);

            ctx.RecipeCategories.AddRange(recipe1.RecipeCategory!, recipe2.RecipeCategory!);
            ctx.ProductCategories.AddRange(product1.ProductCategory!, product2.ProductCategory!);
            ctx.Units.AddRange(product1.BaseUnit!, product2.BaseUnit!, recipe1.RecipeIngredients!.Single().Unit!, recipe2.RecipeIngredients!.Single().Unit!);
            ctx.Products.AddRange(product1, product2);
            ctx.Recipes.AddRange(recipe1, recipe2);
            ctx.MealPlans.AddRange(plan1, plan2);
            ctx.MealPlanRecipes.AddRange(plan1.MealPlanRecipes!);
            ctx.MealPlanRecipes.AddRange(plan2.MealPlanRecipes!);
            ctx.RecipeIngredients.AddRange(recipe1.RecipeIngredients!);
            ctx.RecipeIngredients.AddRange(recipe2.RecipeIngredients!);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchByProductCategoryIdsAsync([20]);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            var kv = result.Single();
            Assert.That(kv.Key.ProductCategoryId, Is.EqualTo(20));
            Assert.That(kv.Value.Name, Is.EqualTo("Plan1"));
        }

        [Test]
        public async Task SearchByProductCategoryIdsAsync_EmptyList_ReturnsEmpty()
        {
            // Arrange
            var repo = CreateRepository(out _);

            // Act
            var result = await repo.SearchByProductCategoryIdsAsync([]);

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

            var (plan1, recipe1, _) = CreateMealPlanGraph(1, "Plan1", 10, "R1", 5, "Main", 1, "Flower", 20, "Baking", 1, 2);
            var (plan2, recipe2, _) = CreateMealPlanGraph(2, "Plan2", 11, "R2", 6, "Dessert", 2, "Chocolate", 21, "Fruit", 3, 4);

            ctx.Recipes.AddRange(recipe1, recipe2);
            ctx.MealPlans.AddRange(plan1, plan2);
            ctx.MealPlanRecipes.AddRange(plan1.MealPlanRecipes!);
            ctx.MealPlanRecipes.AddRange(plan2.MealPlanRecipes!);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchByRecipeAsync(10);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Single().Name, Is.EqualTo("Plan1"));
        }

        [Test]
        public async Task SearchByRecipeAsync_NoMatch_ReturnsEmpty()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var (plan1, recipe1, _) = CreateMealPlanGraph(1, "Plan1", 10, "R1", 5, "Main", 1, "Flower", 20, "Baking", 1, 2);
            ctx.Recipes.Add(recipe1);
            ctx.MealPlans.Add(plan1);
            ctx.MealPlanRecipes.AddRange(plan1.MealPlanRecipes!);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchByRecipeAsync(999);

            // Assert
            Assert.That(result, Is.Empty);
        }

        // ---------- SearchAsync by name ----------
        [Test]
        public async Task SearchAsync_ByName_ReturnsMatchingMealPlan_CaseInsensitive()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var p1 = new MealPlan { Id = 1, Name = "Weekly" };
            var p2 = new MealPlan { Id = 2, Name = "Other" };
            ctx.MealPlans.AddRange(p1, p2);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchAsync("weekly");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task SearchAsync_UnknownName_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.MealPlans.Add(new MealPlan { Id = 1, Name = "Weekly" });
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchAsync("does-not-exist");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task SearchAsync_NullOrWhitespace_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(out _);

            var r1 = await repo.SearchAsync(null!);
            var r2 = await repo.SearchAsync("   ");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(r1, Is.Null);
                Assert.That(r2, Is.Null);
            }
        }
    }
}