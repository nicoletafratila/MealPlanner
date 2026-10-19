using Common.Pagination;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Api.Repositories;
using RecipeBook.Api.Services;
using RecipeBook.Data.Entities;
using RecipeBook.Data.Entities.Enums;

namespace RecipeBook.Api.Tests.Services
{
    [TestFixture]
    public class RecipeJobProcessorTests
    {
        private Mock<IJobRepository> _jobRepoMock = null!;
        private Mock<IRecipeRepository> _recipeRepoMock = null!;
        private Mock<IProductRepository> _productRepoMock = null!;
        private Mock<IRecipeCategoryShareResolver> _recipeCategoryResolverMock = null!;
        private Mock<IProductCategoryShareResolver> _productCategoryResolverMock = null!;
        private Mock<IRecipeShareProgressNotifier> _notifierMock = null!;
        private Mock<ILogger<RecipeJobProcessor>> _loggerMock = null!;
        private RecipeJobProcessor _processor = null!;

        [SetUp]
        public void SetUp()
        {
            _jobRepoMock = new Mock<IJobRepository>(MockBehavior.Strict);
            _recipeRepoMock = new Mock<IRecipeRepository>(MockBehavior.Strict);
            _productRepoMock = new Mock<IProductRepository>(MockBehavior.Strict);
            _recipeCategoryResolverMock = new Mock<IRecipeCategoryShareResolver>(MockBehavior.Strict);
            _productCategoryResolverMock = new Mock<IProductCategoryShareResolver>(MockBehavior.Strict);
            _notifierMock = new Mock<IRecipeShareProgressNotifier>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<RecipeJobProcessor>>(MockBehavior.Loose);

            _jobRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _notifierMock.Setup(n => n.NotifyAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            SetUpIdentityCategoryResolvers();

            _processor = new RecipeJobProcessor(
                _jobRepoMock.Object,
                _recipeRepoMock.Object,
                _productRepoMock.Object,
                _recipeCategoryResolverMock.Object,
                _productCategoryResolverMock.Object,
                _notifierMock.Object,
                _loggerMock.Object);
        }

        private void SetUpIdentityCategoryResolvers()
        {
            _recipeCategoryResolverMock
                .Setup(r => r.ResolveAsync(It.IsAny<IEnumerable<RecipeCategory?>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<RecipeCategory?> categories, string _, CancellationToken _) =>
                    categories.Where(c => c is not null).Select(c => c!.Id).Distinct().ToDictionary(id => id, id => id));

            _productCategoryResolverMock
                .Setup(r => r.ResolveAsync(It.IsAny<IEnumerable<ProductCategory?>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<ProductCategory?> categories, string _, CancellationToken _) =>
                    categories.Where(c => c is not null).Select(c => c!.Id).Distinct().ToDictionary(id => id, id => id));
        }

        private static Product CreateProduct(Guid id, string name, string userId = "user1", Guid? categoryId = null)
        {
            var catId = categoryId ?? Guid.NewGuid();
            return new()
            {
                Id = id,
                Name = name,
                BaseUnitId = Guid.NewGuid(),
                ProductCategoryId = catId,
                ProductCategory = new ProductCategory { Id = catId, Name = "Category", UserId = userId },
                UserId = userId
            };
        }

        private static Recipe CreateRecipe(
            Guid id,
            string name,
            IList<RecipeIngredient>? ingredients = null,
            Guid? categoryId = null)
        {
            var catId = categoryId ?? Guid.NewGuid();
            return new()
            {
                Id = id,
                Name = name,
                RecipeCategoryId = catId,
                RecipeCategory = new RecipeCategory { Id = catId, Name = "Category", UserId = "user1" },
                UserId = "user1",
                RecipeIngredients = ingredients ?? []
            };
        }

        private static Job CreateJob(Guid jobId, string ownerUserId = "user1", string targetUserId = "user2", string? filtersJson = null) => new()
        {
            Id = jobId,
            OwnerUserId = ownerUserId,
            TargetUserId = targetUserId,
            FiltersJson = filtersJson,
            Status = JobStatus.Pending
        };

        private void SetUpAddRangeProducts(Action<IReadOnlyList<Product>>? assignIds = null)
        {
            _productRepoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Product>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<Product> products, CancellationToken _) =>
                {
                    var list = products.ToList();
                    foreach (var product in list.Where(p => p.Id == Guid.Empty))
                        product.Id = Guid.NewGuid();
                    assignIds?.Invoke(list);
                    return list;
                });
        }

        [Test]
        public void Ctor_NullJobRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new RecipeJobProcessor(null!, _recipeRepoMock.Object, _productRepoMock.Object, _recipeCategoryResolverMock.Object, _productCategoryResolverMock.Object, _notifierMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullRecipeRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new RecipeJobProcessor(_jobRepoMock.Object, null!, _productRepoMock.Object, _recipeCategoryResolverMock.Object, _productCategoryResolverMock.Object, _notifierMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullProductRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new RecipeJobProcessor(_jobRepoMock.Object, _recipeRepoMock.Object, null!, _recipeCategoryResolverMock.Object, _productCategoryResolverMock.Object, _notifierMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullRecipeCategoryResolver_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new RecipeJobProcessor(_jobRepoMock.Object, _recipeRepoMock.Object, _productRepoMock.Object, null!, _productCategoryResolverMock.Object, _notifierMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullProductCategoryResolver_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new RecipeJobProcessor(_jobRepoMock.Object, _recipeRepoMock.Object, _productRepoMock.Object, _recipeCategoryResolverMock.Object, null!, _notifierMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullNotifier_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new RecipeJobProcessor(_jobRepoMock.Object, _recipeRepoMock.Object, _productRepoMock.Object, _recipeCategoryResolverMock.Object, _productCategoryResolverMock.Object, null!, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new RecipeJobProcessor(_jobRepoMock.Object, _recipeRepoMock.Object, _productRepoMock.Object, _recipeCategoryResolverMock.Object, _productCategoryResolverMock.Object, _notifierMock.Object, null!));
        }

        [Test]
        public async Task ProcessAsync_JobNotFound_ReturnsWithoutThrowing()
        {
            var jobId = Guid.NewGuid();
            _jobRepoMock.Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>())).ReturnsAsync((Job?)null);

            await _processor.ProcessAsync(jobId, CancellationToken.None);

            _jobRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()), Times.Never);
            _notifierMock.Verify(n => n.NotifyAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task ProcessAsync_NoSourceRecipes_MarksCompleted_WithZeroTotalCount()
        {
            var jobId = Guid.NewGuid();
            var job = CreateJob(jobId);
            _jobRepoMock.Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>())).ReturnsAsync(job);

            _recipeRepoMock
                .Setup(r => r.GetAllByUserIncludeIngredientsAsync("user1", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Recipe>());

            await _processor.ProcessAsync(jobId, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(job.Status, Is.EqualTo(JobStatus.Completed));
                Assert.That(job.TotalCount, Is.Zero);
                Assert.That(job.CompletedAt, Is.Not.Null);
            }

            _productRepoMock.Verify(r => r.GetAllByUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _recipeRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Recipe>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task ProcessAsync_MultipleRecipes_ClonesRecipesAndProducts_MarksCompleted()
        {
            var jobId = Guid.NewGuid();
            var job = CreateJob(jobId);
            _jobRepoMock.Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>())).ReturnsAsync(job);

            var flourId = Guid.NewGuid();
            var sugarId = Guid.NewGuid();
            var flour = CreateProduct(flourId, "Flour");
            var sugar = CreateProduct(sugarId, "Sugar");

            var recipe1 = CreateRecipe(Guid.NewGuid(), "R1",
                [new RecipeIngredient { ProductId = flourId, Product = flour, UnitId = Guid.NewGuid(), Quantity = 1 }]);
            var recipe2 = CreateRecipe(Guid.NewGuid(), "R2",
                [new RecipeIngredient { ProductId = sugarId, Product = sugar, UnitId = Guid.NewGuid(), Quantity = 2 }]);

            _recipeRepoMock
                .Setup(r => r.GetAllByUserIncludeIngredientsAsync("user1", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Recipe> { recipe1, recipe2 });

            _productRepoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Product>());

            _recipeRepoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Recipe>());

            SetUpAddRangeProducts();

            List<Recipe>? addedRecipes = null;
            _recipeRepoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Recipe>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<Recipe>, CancellationToken>((recipes, _) => addedRecipes = recipes.ToList())
                .ReturnsAsync((IEnumerable<Recipe> recipes, CancellationToken _) => recipes.ToList());

            await _processor.ProcessAsync(jobId, CancellationToken.None);

            Assert.That(job.Status, Is.EqualTo(JobStatus.Completed));
            Assert.That(addedRecipes, Is.Not.Null);
            Assert.That(addedRecipes, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(addedRecipes!.Select(r => r.Name), Is.EquivalentTo(["R1", "R2"]));
                Assert.That(addedRecipes!.All(r => r.UserId == "user2"), Is.True);
                Assert.That(job.ProcessedCount, Is.EqualTo(2));
                Assert.That(job.TotalCount, Is.EqualTo(2));
            }

            _productRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Product>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ProcessAsync_SameProductAcrossMultipleRecipes_ClonesProductOnlyOnce()
        {
            var jobId = Guid.NewGuid();
            var job = CreateJob(jobId);
            _jobRepoMock.Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>())).ReturnsAsync(job);

            var productId = Guid.NewGuid();
            var product = CreateProduct(productId, "Flour");

            var recipe1 = CreateRecipe(Guid.NewGuid(), "R1",
                [new RecipeIngredient { ProductId = productId, Product = product, UnitId = Guid.NewGuid(), Quantity = 1 }]);
            var recipe2 = CreateRecipe(Guid.NewGuid(), "R2",
                [new RecipeIngredient { ProductId = productId, Product = product, UnitId = Guid.NewGuid(), Quantity = 2 }]);

            _recipeRepoMock
                .Setup(r => r.GetAllByUserIncludeIngredientsAsync("user1", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Recipe> { recipe1, recipe2 });

            _productRepoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Product>());

            _recipeRepoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Recipe>());

            List<Product>? addedProducts = null;
            SetUpAddRangeProducts(list => addedProducts = list.ToList());

            _recipeRepoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Recipe>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<Recipe> recipes, CancellationToken _) => recipes.ToList());

            await _processor.ProcessAsync(jobId, CancellationToken.None);

            Assert.That(job.Status, Is.EqualTo(JobStatus.Completed));
            Assert.That(addedProducts, Has.Count.EqualTo(1));
            _productRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Product>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ProcessAsync_ExistingProductWithSameName_ReusesExistingProduct_AndDoesNotCloneIt()
        {
            var jobId = Guid.NewGuid();
            var job = CreateJob(jobId);
            _jobRepoMock.Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>())).ReturnsAsync(job);

            var productId = Guid.NewGuid();
            var existingProductId = Guid.NewGuid();
            var product = CreateProduct(productId, "Flour");

            var recipe = CreateRecipe(Guid.NewGuid(), "R1",
                [new RecipeIngredient { ProductId = productId, Product = product, UnitId = Guid.NewGuid(), Quantity = 1 }]);

            _recipeRepoMock
                .Setup(r => r.GetAllByUserIncludeIngredientsAsync("user1", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Recipe> { recipe });

            _productRepoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Product> { CreateProduct(existingProductId, "Flour", "user2", product.ProductCategoryId) });

            _recipeRepoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Recipe>());

            List<Recipe>? addedRecipes = null;
            _recipeRepoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Recipe>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<Recipe>, CancellationToken>((recipes, _) => addedRecipes = recipes.ToList())
                .ReturnsAsync((IEnumerable<Recipe> recipes, CancellationToken _) => recipes.ToList());

            await _processor.ProcessAsync(jobId, CancellationToken.None);

            Assert.That(job.Status, Is.EqualTo(JobStatus.Completed));
            Assert.That(addedRecipes![0].RecipeIngredients![0].ProductId, Is.EqualTo(existingProductId));
            _productRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Product>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task ProcessAsync_NameCollisionWithExistingTargetRecipe_AppendsCopySuffix()
        {
            var jobId = Guid.NewGuid();
            var job = CreateJob(jobId);
            _jobRepoMock.Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>())).ReturnsAsync(job);

            var categoryId = Guid.NewGuid();
            var recipe = CreateRecipe(Guid.NewGuid(), "My Recipe", categoryId: categoryId);

            _recipeRepoMock
                .Setup(r => r.GetAllByUserIncludeIngredientsAsync("user1", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Recipe> { recipe });

            _productRepoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Product>());

            _recipeRepoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Recipe> { CreateRecipe(Guid.NewGuid(), "My Recipe", categoryId: categoryId) });

            List<Recipe>? addedRecipes = null;
            _recipeRepoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Recipe>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<Recipe>, CancellationToken>((recipes, _) => addedRecipes = recipes.ToList())
                .ReturnsAsync((IEnumerable<Recipe> recipes, CancellationToken _) => recipes.ToList());

            await _processor.ProcessAsync(jobId, CancellationToken.None);

            Assert.That(addedRecipes![0].Name, Is.EqualTo("My Recipe (Copy)"));
        }

        [Test]
        public async Task ProcessAsync_TwoSourceRecipesWithSameName_SecondGetsCopySuffix_WithoutQueryingBetween()
        {
            var jobId = Guid.NewGuid();
            var job = CreateJob(jobId);
            _jobRepoMock.Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>())).ReturnsAsync(job);

            var categoryId = Guid.NewGuid();
            var recipe1 = CreateRecipe(Guid.NewGuid(), "My Recipe", categoryId: categoryId);
            var recipe2 = CreateRecipe(Guid.NewGuid(), "My Recipe", categoryId: categoryId);

            _recipeRepoMock
                .Setup(r => r.GetAllByUserIncludeIngredientsAsync("user1", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Recipe> { recipe1, recipe2 });

            _productRepoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Product>());

            _recipeRepoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Recipe>());

            List<Recipe>? addedRecipes = null;
            _recipeRepoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Recipe>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<Recipe>, CancellationToken>((recipes, _) => addedRecipes = recipes.ToList())
                .ReturnsAsync((IEnumerable<Recipe> recipes, CancellationToken _) => recipes.ToList());

            await _processor.ProcessAsync(jobId, CancellationToken.None);

            Assert.That(addedRecipes!.Select(r => r.Name), Is.EqualTo(["My Recipe", "My Recipe (Copy)"]));

            // Only one lookup of the target's existing recipes - name collisions within the batch are
            // resolved in memory, not via a DB round trip per recipe.
            _recipeRepoMock.Verify(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ProcessAsync_WithFilters_DeserializesAndPassesThemToTheRepositoryQuery()
        {
            var jobId = Guid.NewGuid();
            var filters = new List<FilterItem> { new("Name", "Soup", FilterOperator.Contains, StringComparison.OrdinalIgnoreCase) };
            var job = CreateJob(jobId, filtersJson: Newtonsoft.Json.JsonConvert.SerializeObject(filters));
            _jobRepoMock.Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>())).ReturnsAsync(job);

            IEnumerable<FilterItem>? capturedFilters = null;
            _recipeRepoMock
                .Setup(r => r.GetAllByUserIncludeIngredientsAsync("user1", It.IsAny<IEnumerable<FilterItem>?>(), It.IsAny<CancellationToken>()))
                .Callback<string, IEnumerable<FilterItem>?, CancellationToken>((_, f, _) => capturedFilters = f)
                .ReturnsAsync(new List<Recipe>());

            await _processor.ProcessAsync(jobId, CancellationToken.None);

            Assert.That(job.Status, Is.EqualTo(JobStatus.Completed));
            Assert.That(capturedFilters, Is.Not.Null);
            Assert.That(capturedFilters!.Single().Value, Is.EqualTo("Soup"));
        }

        [Test]
        public async Task ProcessAsync_ExceptionDuringProcessing_MarksFailed_WithErrorMessage_AndNotifies()
        {
            var jobId = Guid.NewGuid();
            var job = CreateJob(jobId);
            _jobRepoMock.Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>())).ReturnsAsync(job);

            _recipeRepoMock
                .Setup(r => r.GetAllByUserIncludeIngredientsAsync("user1", null, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            await _processor.ProcessAsync(jobId, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(job.Status, Is.EqualTo(JobStatus.Failed));
                Assert.That(job.ErrorMessage, Is.EqualTo("An error occurred when saving the recipe."));
                Assert.That(job.CompletedAt, Is.Not.Null);
            }

            _notifierMock.Verify(n => n.NotifyAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }
    }
}
