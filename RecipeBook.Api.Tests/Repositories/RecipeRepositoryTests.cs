using Common.Data.DataContext;
using Common.Pagination;
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
        private static Guid UnitGuid(int seed) => new(seed * 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        private static Guid ProductGuid(int seed) => new(seed * 1000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        private static Guid RecipeGuid(int seed) => new(seed * 10000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        private static Recipe CreateRecipeGraph(
            Guid id,
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
                Id = id, // Guid
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

            var r1 = CreateRecipeGraph(RecipeGuid(1), "R1", RecipeCategoryGuid(10), "Main");
            var r2 = CreateRecipeGraph(RecipeGuid(2), "R2", RecipeCategoryGuid(20), "Dessert");
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

            var r1 = CreateRecipeGraph(RecipeGuid(1), "R1", RecipeCategoryGuid(10), "Main");
            ctx.Recipes.Add(r1);
            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdAsync(RecipeGuid(1), CancellationToken.None);

            // Assert
            Assert.That(found, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(found!.Name, Is.EqualTo("R1"));
                Assert.That(found.RecipeCategory, Is.Not.Null);
            }
            Assert.That(found.RecipeCategory!.Name, Is.EqualTo("Main"));
        }

        // ---------- GetAllByUserAsync ----------
        [Test]
        public async Task GetAllByUserAsync_ReturnsOnlyRecipesForThatUser_WithCategoryIncluded()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var r1 = CreateRecipeGraph(RecipeGuid(1), "R1", RecipeCategoryGuid(10), "Main");
            var r2 = CreateRecipeGraph(RecipeGuid(2), "R2", RecipeCategoryGuid(20), "Dessert");
            r1.UserId = "user1";
            r2.UserId = "user2";
            ctx.Recipes.AddRange(r1, r2);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.GetAllByUserAsync("user1", CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            var recipe = result.Single();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(recipe.Name, Is.EqualTo("R1"));
                Assert.That(recipe.RecipeCategory, Is.Not.Null);
            }
        }

        [Test]
        public async Task GetAllByUserAsync_NoMatches_ReturnsEmptyList()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var r1 = CreateRecipeGraph(RecipeGuid(1), "R1", RecipeCategoryGuid(10), "Main");
            r1.UserId = "user2";
            ctx.Recipes.Add(r1);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.GetAllByUserAsync("user1", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Empty);
        }

        // ---------- GetByIdIncludeIngredientsAsync ----------
        [Test]
        public async Task GetByIdIncludeIngredientsAsync_ReturnsRecipe_WithIngredientsAndIncludes()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var recipe = CreateRecipeGraph(RecipeGuid(1), "R1", RecipeCategoryGuid(10), "Main");
            ctx.Recipes.Add(recipe);
            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdIncludeIngredientsAsync(RecipeGuid(1), CancellationToken.None);

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

            var r1 = CreateRecipeGraph(RecipeGuid(1), "R1", RecipeCategoryGuid(10), "Main");
            var r2 = CreateRecipeGraph(RecipeGuid(2), "R2", RecipeCategoryGuid(20), "Dessert");
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

            var r1 = CreateRecipeGraph(RecipeGuid(1), "My Recipe", RecipeCategoryGuid(10), "Main");
            var r2 = CreateRecipeGraph(RecipeGuid(2), "Other", RecipeCategoryGuid(20), "Dessert");
            r1.UserId = "user1";
            r2.UserId = "user1";
            ctx.Recipes.AddRange(r1, r2);
            await ctx.SaveChangesAsync();

            // Act
            var result = await repo.SearchAsync("my recipe", "user1", CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(RecipeGuid(1)));
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
            ctx.Units.Add(new Unit { Id = UnitGuid(1), Name = "kg", UnitType = 0 });
            ctx.Recipes.Add(new Recipe { Id = RecipeGuid(1), Name = "R1", RecipeCategoryId = RecipeCategoryGuid(10) });
            ctx.RecipeIngredients.Add(new RecipeIngredient { RecipeId = RecipeGuid(1), ProductId = ProductGuid(100), UnitId = UnitGuid(1), Quantity = 1m});
            await ctx.SaveChangesAsync();

            var entity = await repo.GetByIdIncludeIngredientsAsync(RecipeGuid(1), CancellationToken.None);
            entity!.RecipeIngredients =
            [
                new RecipeIngredient { RecipeId = RecipeGuid(1), ProductId = ProductGuid(100), UnitId = UnitGuid(1), Quantity = 1m },
                new RecipeIngredient { RecipeId = RecipeGuid(1), ProductId = ProductGuid(200), UnitId = UnitGuid(1), Quantity = 2m }
            ];

            // Act
            await repo.UpdateAsync(entity, CancellationToken.None);

            // Assert
            var rows = ctx.RecipeIngredients.Where(ri => ri.RecipeId == RecipeGuid(1)).ToList();
            Assert.That(rows, Has.Count.EqualTo(2));
            Assert.That(rows.Select(ri => ri.ProductId), Is.EquivalentTo(new[] { ProductGuid(100), ProductGuid(200) }));
        }

        [Test]
        public async Task UpdateAsync_RemovesDeletedIngredient_FromRecipe()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);
            ctx.RecipeCategories.Add(new RecipeCategory { Id = RecipeCategoryGuid(10), Name = "Main", DisplaySequence = 1 });
            ctx.Units.Add(new Unit { Id = UnitGuid(1), Name = "kg", UnitType = 0 });
            ctx.Recipes.Add(new Recipe { Id = RecipeGuid(1), Name = "R1", RecipeCategoryId = RecipeCategoryGuid(10) });
            ctx.RecipeIngredients.AddRange(
                new RecipeIngredient { RecipeId = RecipeGuid(1), ProductId = ProductGuid(100), UnitId = UnitGuid(1), Quantity = 1m },
                new RecipeIngredient { RecipeId = RecipeGuid(1), ProductId = ProductGuid(200), UnitId = UnitGuid(1), Quantity = 2m });
            await ctx.SaveChangesAsync();

            var entity = await repo.GetByIdIncludeIngredientsAsync(RecipeGuid(1), CancellationToken.None);
            entity!.RecipeIngredients = [new RecipeIngredient { RecipeId = RecipeGuid(1), ProductId = ProductGuid(100), UnitId = UnitGuid(1), Quantity = 1m}];

            // Act
            await repo.UpdateAsync(entity, CancellationToken.None);

            // Assert
            var rows = ctx.RecipeIngredients.Where(ri => ri.RecipeId == RecipeGuid(1)).ToList();
            Assert.That(rows, Has.Count.EqualTo(1));
            Assert.That(rows.Single().ProductId, Is.EqualTo(ProductGuid(100)));
        }

        [Test]
        public async Task UpdateAsync_UpdatesQuantityAndUnit_ForExistingIngredient()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);
            ctx.RecipeCategories.Add(new RecipeCategory { Id = RecipeCategoryGuid(10), Name = "Main", DisplaySequence = 1 });
            ctx.Units.AddRange(
                new Unit { Id = UnitGuid(1), Name = "kg", UnitType = 0 },
                new Unit { Id = UnitGuid(2), Name = "g", UnitType = 0 });
            ctx.Recipes.Add(new Recipe { Id = RecipeGuid(1), Name = "R1", RecipeCategoryId = RecipeCategoryGuid(10) });
            ctx.RecipeIngredients.Add(new RecipeIngredient { RecipeId = RecipeGuid(1), ProductId = ProductGuid(100), UnitId = UnitGuid(1), Quantity = 1m});
            await ctx.SaveChangesAsync();

            var entity = await repo.GetByIdIncludeIngredientsAsync(RecipeGuid(1), CancellationToken.None);
            entity!.RecipeIngredients = [new RecipeIngredient { RecipeId = RecipeGuid(1), ProductId = ProductGuid(100), UnitId = UnitGuid(2), Quantity = 500m}];

            // Act
            await repo.UpdateAsync(entity, CancellationToken.None);

            // Assert
            var row = ctx.RecipeIngredients.Single(ri => ri.RecipeId == RecipeGuid(1) && ri.ProductId == ProductGuid(100));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(row.Quantity, Is.EqualTo(500m));
                Assert.That(row.UnitId, Is.EqualTo(UnitGuid(2)));
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

        // ---------- SearchByUserAsync ----------
        [Test]
        public async Task SearchByUserAsync_ScopesToUser_WithCategoryIncluded()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var r1 = CreateRecipeGraph(RecipeGuid(1), "R1", RecipeCategoryGuid(10), "Main");
            var r2 = CreateRecipeGraph(RecipeGuid(2), "R2", RecipeCategoryGuid(20), "Dessert");
            r1.UserId = "user1";
            r2.UserId = "user2";
            ctx.Recipes.AddRange(r1, r2);
            await ctx.SaveChangesAsync();

            // Act
            var (items, totalCount, skip) = await repo.SearchByUserAsync(
                "user1", null, null, null, 1, 10, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(items, Has.Count.EqualTo(1));
                Assert.That(totalCount, Is.EqualTo(1));
                Assert.That(skip, Is.Zero);
                Assert.That(items.Single().Name, Is.EqualTo("R1"));
                Assert.That(items.Single().RecipeCategory, Is.Not.Null);
            }
        }

        [Test]
        public async Task SearchByUserAsync_ThumbnailOnlyTrue_ProjectsAwayImageContent_ButIncludesThumbnail()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var r1 = CreateRecipeGraph(RecipeGuid(1), "R1", RecipeCategoryGuid(10), "Main");
            r1.UserId = "user1";
            r1.ImageContent = [1, 2, 3];
            r1.ImageThumbnail = [4, 5, 6];
            ctx.Recipes.Add(r1);
            await ctx.SaveChangesAsync();

            // Act
            var (items, _, _) = await repo.SearchByUserAsync(
                "user1", null, null, null, 1, 10, CancellationToken.None, thumbnailOnly: true);

            // Assert
            var item = items.Single();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(item.ImageContent, Is.Null);
                Assert.That(item.ImageThumbnail, Is.EqualTo(new byte[] { 4, 5, 6 }));
            }
        }

        [Test]
        public async Task SearchByUserAsync_ThumbnailOnlyFalse_IncludesFullImageAndThumbnail()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var r1 = CreateRecipeGraph(RecipeGuid(1), "R1", RecipeCategoryGuid(10), "Main");
            r1.UserId = "user1";
            r1.ImageContent = [1, 2, 3];
            r1.ImageThumbnail = [4, 5, 6];
            ctx.Recipes.Add(r1);
            await ctx.SaveChangesAsync();

            // Act
            var (items, _, _) = await repo.SearchByUserAsync(
                "user1", null, null, null, 1, 10, CancellationToken.None);

            // Assert
            var item = items.Single();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(item.ImageContent, Is.EqualTo(new byte[] { 1, 2, 3 }));
                Assert.That(item.ImageThumbnail, Is.EqualTo(new byte[] { 4, 5, 6 }));
            }
        }

        [Test]
        public async Task SearchByUserAsync_CategoryIdFilter_ReturnsOnlyMatchingCategory()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var r1 = CreateRecipeGraph(RecipeGuid(1), "R1", RecipeCategoryGuid(10), "Main");
            var r2 = CreateRecipeGraph(RecipeGuid(2), "R2", RecipeCategoryGuid(20), "Dessert");
            var r3 = new Recipe { Id = RecipeGuid(3), Name = "R3", RecipeCategoryId = r1.RecipeCategoryId, RecipeCategory = r1.RecipeCategory };
            r1.UserId = r2.UserId = r3.UserId = "user1";
            ctx.Recipes.AddRange(r1, r2, r3);
            await ctx.SaveChangesAsync();

            // Act
            var (items, totalCount, _) = await repo.SearchByUserAsync(
                "user1", RecipeCategoryGuid(10), null, null, 1, 10, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(totalCount, Is.EqualTo(2));
                Assert.That(items.Select(x => x.Name), Is.EquivalentTo(["R1", "R3"]));
            }
        }

        [Test]
        public async Task SearchByUserAsync_NameFilter_ReturnsOnlyMatchingRecipes()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var r1 = CreateRecipeGraph(RecipeGuid(1), "My Recipe", RecipeCategoryGuid(10), "Main");
            var r2 = CreateRecipeGraph(RecipeGuid(2), "Other", RecipeCategoryGuid(20), "Dessert");
            r1.UserId = r2.UserId = "user1";
            ctx.Recipes.AddRange(r1, r2);
            await ctx.SaveChangesAsync();

            var filters = new[] { new FilterItem(nameof(Recipe.Name), "My Recipe", FilterOperator.Contains) };

            // Act
            var (items, totalCount, _) = await repo.SearchByUserAsync(
                "user1", null, filters, null, 1, 10, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(totalCount, Is.EqualTo(1));
                Assert.That(items.Single().Name, Is.EqualTo("My Recipe"));
            }
        }

        [Test]
        public async Task SearchByUserAsync_Sorting_ReturnsSortedByRequestedProperty()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var r1 = CreateRecipeGraph(RecipeGuid(1), "Bravo", RecipeCategoryGuid(10), "Main");
            var r2 = CreateRecipeGraph(RecipeGuid(2), "Alpha", RecipeCategoryGuid(20), "Dessert");
            r1.UserId = r2.UserId = "user1";
            ctx.Recipes.AddRange(r1, r2);
            await ctx.SaveChangesAsync();

            var sorting = new[] { new SortingModel { PropertyName = nameof(Recipe.Name), Direction = SortDirection.Ascending } };

            // Act
            var (items, _, _) = await repo.SearchByUserAsync(
                "user1", null, null, sorting, 1, 10, CancellationToken.None);

            // Assert
            Assert.That(items.Select(x => x.Name), Is.EqualTo(["Alpha", "Bravo"]));
        }

        [Test]
        public async Task SearchByUserAsync_RecipeCategoryNameFilter_ReturnsOnlyMatchingCategory()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var r1 = CreateRecipeGraph(RecipeGuid(1), "R1", RecipeCategoryGuid(10), "Main");
            var r2 = CreateRecipeGraph(RecipeGuid(2), "R2", RecipeCategoryGuid(20), "Dessert");
            r1.UserId = r2.UserId = "user1";
            ctx.Recipes.AddRange(r1, r2);
            await ctx.SaveChangesAsync();

            var filters = new[] { new FilterItem("RecipeCategoryName", "Main", FilterOperator.Contains) };

            // Act
            var (items, totalCount, _) = await repo.SearchByUserAsync(
                "user1", null, filters, null, 1, 10, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(totalCount, Is.EqualTo(1));
                Assert.That(items.Single().Name, Is.EqualTo("R1"));
            }
        }

        [Test]
        public async Task SearchByUserAsync_RecipeCategoryNameSorting_ReturnsSortedByCategoryName()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var r1 = CreateRecipeGraph(RecipeGuid(1), "R1", RecipeCategoryGuid(10), "Main");
            var r2 = CreateRecipeGraph(RecipeGuid(2), "R2", RecipeCategoryGuid(20), "Dessert");
            r1.UserId = r2.UserId = "user1";
            ctx.Recipes.AddRange(r1, r2);
            await ctx.SaveChangesAsync();

            var sorting = new[] { new SortingModel { PropertyName = "RecipeCategoryName", Direction = SortDirection.Ascending } };

            // Act
            var (items, _, _) = await repo.SearchByUserAsync(
                "user1", null, null, sorting, 1, 10, CancellationToken.None);

            // Assert
            Assert.That(items.Select(x => x.Name), Is.EqualTo(["R2", "R1"]));
        }

        [Test]
        public async Task SearchByUserAsync_Paging_ReturnsRequestedPageAndSkip()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            for (var i = 1; i <= 5; i++)
            {
                var r = CreateRecipeGraph(RecipeGuid(i), $"R{i}", RecipeCategoryGuid(i), $"Cat{i}");
                r.UserId = "user1";
                ctx.Recipes.Add(r);
            }
            await ctx.SaveChangesAsync();

            var sorting = new[] { new SortingModel { PropertyName = nameof(Recipe.Name), Direction = SortDirection.Ascending } };

            // Act
            var (items, totalCount, skip) = await repo.SearchByUserAsync(
                "user1", null, null, sorting, 2, 2, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(totalCount, Is.EqualTo(5));
                Assert.That(skip, Is.EqualTo(2));
                Assert.That(items.Select(x => x.Name), Is.EqualTo(["R3", "R4"]));
            }
        }

        [Test]
        public async Task SearchByUserAsync_NoMatches_ReturnsEmptyWithZeroTotalCount()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var r1 = CreateRecipeGraph(RecipeGuid(1), "R1", RecipeCategoryGuid(10), "Main");
            r1.UserId = "user2";
            ctx.Recipes.Add(r1);
            await ctx.SaveChangesAsync();

            // Act
            var (items, totalCount, _) = await repo.SearchByUserAsync(
                "user1", null, null, null, 1, 10, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(items, Is.Empty);
                Assert.That(totalCount, Is.Zero);
            }
        }
    }
}