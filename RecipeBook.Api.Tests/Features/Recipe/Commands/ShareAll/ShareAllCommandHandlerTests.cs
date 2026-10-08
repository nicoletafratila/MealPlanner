using Common.Pagination;
using Common.Services;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Api.Features.Recipe.Commands.ShareAll;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Tests.Features.Recipe.Commands.ShareAll
{
    [TestFixture]
    public class ShareAllCommandHandlerTests
    {
        private Mock<IRecipeRepository> _repoMock = null!;
        private Mock<IProductRepository> _productRepoMock = null!;
        private Mock<ICurrentUserService> _currentUserMock = null!;
        private Mock<ILogger<ShareAllCommandHandler>> _loggerMock = null!;
        private ShareAllCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IRecipeRepository>(MockBehavior.Strict);
            _productRepoMock = new Mock<IProductRepository>(MockBehavior.Strict);
            _currentUserMock = new Mock<ICurrentUserService>(MockBehavior.Loose);
            _loggerMock = new Mock<ILogger<ShareAllCommandHandler>>(MockBehavior.Loose);

            _currentUserMock.Setup(s => s.UserId).Returns("user1");

            _handler = new ShareAllCommandHandler(
                _repoMock.Object,
                _productRepoMock.Object,
                _currentUserMock.Object,
                _loggerMock.Object);
        }

        private static Data.Entities.Product CreateProduct(Guid id, string name, string userId = "user1") =>
            new()
            {
                Id = id,
                Name = name,
                BaseUnitId = Guid.NewGuid(),
                ProductCategoryId = Guid.NewGuid(),
                UserId = userId
            };

        private static Data.Entities.Recipe CreateRecipe(
            Guid id,
            string name,
            IList<Data.Entities.RecipeIngredient>? ingredients = null) =>
            new()
            {
                Id = id,
                Name = name,
                RecipeCategoryId = Guid.NewGuid(),
                UserId = "user1",
                RecipeIngredients = ingredients ?? []
            };

        private void SetUpAddRangeProducts(Action<IReadOnlyList<Data.Entities.Product>>? assignIds = null)
        {
            _productRepoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Data.Entities.Product>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<Data.Entities.Product> products, CancellationToken _) =>
                {
                    var list = products.ToList();
                    foreach (var product in list.Where(p => p.Id == Guid.Empty))
                        product.Id = Guid.NewGuid();
                    assignIds?.Invoke(list);
                    return list;
                });
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareAllCommandHandler(null!, _productRepoMock.Object, _currentUserMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullProductRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareAllCommandHandler(_repoMock.Object, null!, _currentUserMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullCurrentUserService_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareAllCommandHandler(_repoMock.Object, _productRepoMock.Object, null!, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareAllCommandHandler(_repoMock.Object, _productRepoMock.Object, _currentUserMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_NoCurrentUser_ReturnsFailedResponse_AndDoesNotQueryRecipes()
        {
            _currentUserMock.Setup(s => s.UserId).Returns((string?)null);
            var command = new ShareAllCommand { TargetUserId = "user2" };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("User id is required."));
            }

            _repoMock.Verify(r => r.GetAllByUserIncludeIngredientsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<FilterItem>?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoRecipes_ReturnsSuccess_AndDoesNotCloneAnything()
        {
            var command = new ShareAllCommand { TargetUserId = "user2" };

            _repoMock
                .Setup(r => r.GetAllByUserIncludeIngredientsAsync("user1", It.IsAny<IEnumerable<FilterItem>?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Recipe>());

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _productRepoMock.Verify(r => r.GetAllByUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _repoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Data.Entities.Recipe>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_MultipleRecipes_ClonesRecipesAndProducts_InBatchedCalls()
        {
            var command = new ShareAllCommand { TargetUserId = "user2" };

            var flourId = Guid.NewGuid();
            var sugarId = Guid.NewGuid();
            var flour = CreateProduct(flourId, "Flour");
            var sugar = CreateProduct(sugarId, "Sugar");

            var recipe1 = CreateRecipe(Guid.NewGuid(), "R1",
                [new Data.Entities.RecipeIngredient { ProductId = flourId, Product = flour, UnitId = Guid.NewGuid(), Quantity = 1 }]);
            var recipe2 = CreateRecipe(Guid.NewGuid(), "R2",
                [new Data.Entities.RecipeIngredient { ProductId = sugarId, Product = sugar, UnitId = Guid.NewGuid(), Quantity = 2 }]);

            _repoMock
                .Setup(r => r.GetAllByUserIncludeIngredientsAsync("user1", It.IsAny<IEnumerable<FilterItem>?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Recipe> { recipe1, recipe2 });

            _productRepoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Product>());

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Recipe>());

            SetUpAddRangeProducts();

            List<Data.Entities.Recipe>? addedRecipes = null;
            _repoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Data.Entities.Recipe>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<Data.Entities.Recipe>, CancellationToken>((recipes, _) => addedRecipes = recipes.ToList())
                .ReturnsAsync((IEnumerable<Data.Entities.Recipe> recipes, CancellationToken _) => recipes.ToList());

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            Assert.That(addedRecipes, Is.Not.Null);
            Assert.That(addedRecipes, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(addedRecipes!.Select(r => r.Name), Is.EquivalentTo(["R1", "R2"]));
                Assert.That(addedRecipes!.All(r => r.UserId == "user2"), Is.True);
                Assert.That(addedRecipes![0].RecipeIngredients![0].ProductId, Is.Not.EqualTo(flourId));
                Assert.That(addedRecipes![1].RecipeIngredients![0].ProductId, Is.Not.EqualTo(sugarId));
            }

            _productRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Data.Entities.Product>>(), It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Data.Entities.Recipe>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_SameProductAcrossMultipleRecipes_ClonesProductOnlyOnce()
        {
            var command = new ShareAllCommand { TargetUserId = "user2" };

            var productId = Guid.NewGuid();
            var product = CreateProduct(productId, "Flour");

            var recipe1 = CreateRecipe(Guid.NewGuid(), "R1",
                [new Data.Entities.RecipeIngredient { ProductId = productId, Product = product, UnitId = Guid.NewGuid(), Quantity = 1 }]);
            var recipe2 = CreateRecipe(Guid.NewGuid(), "R2",
                [new Data.Entities.RecipeIngredient { ProductId = productId, Product = product, UnitId = Guid.NewGuid(), Quantity = 2 }]);

            _repoMock
                .Setup(r => r.GetAllByUserIncludeIngredientsAsync("user1", It.IsAny<IEnumerable<FilterItem>?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Recipe> { recipe1, recipe2 });

            _productRepoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Product>());

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Recipe>());

            List<Data.Entities.Product>? addedProducts = null;
            SetUpAddRangeProducts(list => addedProducts = list.ToList());

            _repoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Data.Entities.Recipe>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<Data.Entities.Recipe> recipes, CancellationToken _) => recipes.ToList());

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);
            Assert.That(addedProducts, Has.Count.EqualTo(1));

            _productRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Data.Entities.Product>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_ExistingProductWithSameName_ReusesExistingProduct_AndDoesNotCloneIt()
        {
            var command = new ShareAllCommand { TargetUserId = "user2" };

            var productId = Guid.NewGuid();
            var existingProductId = Guid.NewGuid();
            var product = CreateProduct(productId, "Flour");

            var recipe = CreateRecipe(Guid.NewGuid(), "R1",
                [new Data.Entities.RecipeIngredient { ProductId = productId, Product = product, UnitId = Guid.NewGuid(), Quantity = 1 }]);

            _repoMock
                .Setup(r => r.GetAllByUserIncludeIngredientsAsync("user1", It.IsAny<IEnumerable<FilterItem>?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Recipe> { recipe });

            _productRepoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Product> { CreateProduct(existingProductId, "Flour", "user2") });

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Recipe>());

            List<Data.Entities.Recipe>? addedRecipes = null;
            _repoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Data.Entities.Recipe>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<Data.Entities.Recipe>, CancellationToken>((recipes, _) => addedRecipes = recipes.ToList())
                .ReturnsAsync((IEnumerable<Data.Entities.Recipe> recipes, CancellationToken _) => recipes.ToList());

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);
            Assert.That(addedRecipes![0].RecipeIngredients![0].ProductId, Is.EqualTo(existingProductId));

            _productRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Data.Entities.Product>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_NameCollisionWithExistingTargetRecipe_AppendsCopySuffix()
        {
            var command = new ShareAllCommand { TargetUserId = "user2" };

            var recipe = CreateRecipe(Guid.NewGuid(), "My Recipe");

            _repoMock
                .Setup(r => r.GetAllByUserIncludeIngredientsAsync("user1", It.IsAny<IEnumerable<FilterItem>?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Recipe> { recipe });

            _productRepoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Product>());

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Recipe> { CreateRecipe(Guid.NewGuid(), "My Recipe") });

            List<Data.Entities.Recipe>? addedRecipes = null;
            _repoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Data.Entities.Recipe>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<Data.Entities.Recipe>, CancellationToken>((recipes, _) => addedRecipes = recipes.ToList())
                .ReturnsAsync((IEnumerable<Data.Entities.Recipe> recipes, CancellationToken _) => recipes.ToList());

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);
            Assert.That(addedRecipes![0].Name, Is.EqualTo("My Recipe (Copy)"));
        }

        [Test]
        public async Task Handle_TwoSourceRecipesWithSameName_SecondGetsCopySuffix_WithoutQueryingBetween()
        {
            var command = new ShareAllCommand { TargetUserId = "user2" };

            var recipe1 = CreateRecipe(Guid.NewGuid(), "My Recipe");
            var recipe2 = CreateRecipe(Guid.NewGuid(), "My Recipe");

            _repoMock
                .Setup(r => r.GetAllByUserIncludeIngredientsAsync("user1", It.IsAny<IEnumerable<FilterItem>?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Recipe> { recipe1, recipe2 });

            _productRepoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Product>());

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Recipe>());

            List<Data.Entities.Recipe>? addedRecipes = null;
            _repoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Data.Entities.Recipe>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<Data.Entities.Recipe>, CancellationToken>((recipes, _) => addedRecipes = recipes.ToList())
                .ReturnsAsync((IEnumerable<Data.Entities.Recipe> recipes, CancellationToken _) => recipes.ToList());

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);
            Assert.That(addedRecipes!.Select(r => r.Name), Is.EqualTo(["My Recipe", "My Recipe (Copy)"]));

            // Only one lookup of the target's existing recipes - name collisions within the batch are
            // resolved in memory, not via a DB round trip per recipe.
            _repoMock.Verify(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_RepeatedNameCollision_IncrementsCopySuffix()
        {
            var command = new ShareAllCommand { TargetUserId = "user2" };

            var recipe = CreateRecipe(Guid.NewGuid(), "My Recipe");

            _repoMock
                .Setup(r => r.GetAllByUserIncludeIngredientsAsync("user1", It.IsAny<IEnumerable<FilterItem>?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Recipe> { recipe });

            _productRepoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Product>());

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Recipe>
                {
                    CreateRecipe(Guid.NewGuid(), "My Recipe"),
                    CreateRecipe(Guid.NewGuid(), "My Recipe (Copy)")
                });

            List<Data.Entities.Recipe>? addedRecipes = null;
            _repoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Data.Entities.Recipe>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<Data.Entities.Recipe>, CancellationToken>((recipes, _) => addedRecipes = recipes.ToList())
                .ReturnsAsync((IEnumerable<Data.Entities.Recipe> recipes, CancellationToken _) => recipes.ToList());

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);
            Assert.That(addedRecipes![0].Name, Is.EqualTo("My Recipe (Copy 2)"));
        }

        [Test]
        public async Task Handle_WithFilters_PassesThemToTheRepositoryQuery()
        {
            var filters = new List<FilterItem> { new("Name", "Soup", FilterOperator.Contains, StringComparison.OrdinalIgnoreCase) };
            var command = new ShareAllCommand { TargetUserId = "user2", Filters = filters };

            IEnumerable<FilterItem>? capturedFilters = null;
            _repoMock
                .Setup(r => r.GetAllByUserIncludeIngredientsAsync("user1", It.IsAny<IEnumerable<FilterItem>?>(), It.IsAny<CancellationToken>()))
                .Callback<string, IEnumerable<FilterItem>?, CancellationToken>((_, f, _) => capturedFilters = f)
                .ReturnsAsync(new List<Data.Entities.Recipe>());

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);
            Assert.That(capturedFilters, Is.SameAs(filters));
        }

        [Test]
        public async Task Handle_ExceptionDuringProcessing_LogsError_AndReturnsFailedResponse()
        {
            var command = new ShareAllCommand { TargetUserId = "user2" };

            _repoMock
                .Setup(r => r.GetAllByUserIncludeIngredientsAsync("user1", It.IsAny<IEnumerable<FilterItem>?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("An error occurred when saving the recipe."));
            }

            _loggerMock.Verify(
                l => l.Log(
                    It.Is<LogLevel>(ll => ll == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
