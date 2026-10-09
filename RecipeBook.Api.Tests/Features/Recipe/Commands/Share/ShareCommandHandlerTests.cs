using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Api.Features.Recipe.Commands.Share;
using RecipeBook.Api.Repositories;
using RecipeBook.Api.Services;

namespace RecipeBook.Api.Tests.Features.Recipe.Commands.Share
{
    [TestFixture]
    public class ShareCommandHandlerTests
    {
        private Mock<IRecipeRepository> _repoMock = null!;
        private Mock<IProductRepository> _productRepoMock = null!;
        private Mock<IRecipeCategoryShareResolver> _recipeCategoryResolverMock = null!;
        private Mock<IProductCategoryShareResolver> _productCategoryResolverMock = null!;
        private Mock<ILogger<ShareCommandHandler>> _loggerMock = null!;
        private ShareCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IRecipeRepository>(MockBehavior.Strict);
            _productRepoMock = new Mock<IProductRepository>(MockBehavior.Strict);
            _recipeCategoryResolverMock = new Mock<IRecipeCategoryShareResolver>(MockBehavior.Strict);
            _productCategoryResolverMock = new Mock<IProductCategoryShareResolver>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<ShareCommandHandler>>(MockBehavior.Loose);

            SetUpIdentityCategoryResolvers();

            _handler = new ShareCommandHandler(
                _repoMock.Object,
                _productRepoMock.Object,
                _recipeCategoryResolverMock.Object,
                _productCategoryResolverMock.Object,
                _loggerMock.Object);
        }

        // By default, the category resolver mocks map every distinct source category to itself, so tests that don't
        // care about category remapping can assert on the pre-existing (source) category ids unchanged.
        private void SetUpIdentityCategoryResolvers()
        {
            _recipeCategoryResolverMock
                .Setup(r => r.ResolveAsync(It.IsAny<IEnumerable<Data.Entities.RecipeCategory?>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<Data.Entities.RecipeCategory?> categories, string _, CancellationToken _) =>
                    categories.Where(c => c is not null).Select(c => c!.Id).Distinct().ToDictionary(id => id, id => id));

            _productCategoryResolverMock
                .Setup(r => r.ResolveAsync(It.IsAny<IEnumerable<Data.Entities.ProductCategory?>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<Data.Entities.ProductCategory?> categories, string _, CancellationToken _) =>
                    categories.Where(c => c is not null).Select(c => c!.Id).Distinct().ToDictionary(id => id, id => id));
        }

        private void SetUpProductClone(Data.Entities.Product product, Guid clonedProductId)
        {
            _productRepoMock
                .Setup(r => r.SearchAsync(product.Name!, product.ProductCategoryId, "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Product?)null);

            _productRepoMock
                .Setup(r => r.AddAsync(
                    It.Is<Data.Entities.Product>(p =>
                        p.Name == product.Name &&
                        p.BaseUnitId == product.BaseUnitId &&
                        p.ProductCategoryId == product.ProductCategoryId &&
                        p.UserId == "user2"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Product p, CancellationToken _) =>
                {
                    p.Id = clonedProductId;
                    return p;
                });
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareCommandHandler(null!, _productRepoMock.Object, _recipeCategoryResolverMock.Object, _productCategoryResolverMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullProductRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareCommandHandler(_repoMock.Object, null!, _recipeCategoryResolverMock.Object, _productCategoryResolverMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullRecipeCategoryResolver_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareCommandHandler(_repoMock.Object, _productRepoMock.Object, null!, _productCategoryResolverMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullProductCategoryResolver_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareCommandHandler(_repoMock.Object, _productRepoMock.Object, _recipeCategoryResolverMock.Object, null!, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareCommandHandler(_repoMock.Object, _productRepoMock.Object, _recipeCategoryResolverMock.Object, _productCategoryResolverMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_RecipeNotFound_ReturnsFailedResponse_AndDoesNotAdd()
        {
            var recipeId = Guid.NewGuid();
            var command = new ShareCommand { RecipeId = recipeId, TargetUserId = "user2" };

            _repoMock
                .Setup(r => r.GetByIdIncludeIngredientsAsync(recipeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Recipe?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo($"Could not find with id {recipeId}"));
            }

            _repoMock.Verify(r => r.AddAsync(It.IsAny<Data.Entities.Recipe>(), It.IsAny<CancellationToken>()), Times.Never);
            _productRepoMock.Verify(r => r.AddAsync(It.IsAny<Data.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoNameCollision_SharesWithOriginalName_AndClonesProduct()
        {
            var recipeId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var baseUnitId = Guid.NewGuid();
            var productCategoryId = Guid.NewGuid();
            var clonedProductId = Guid.NewGuid();
            var command = new ShareCommand { RecipeId = recipeId, TargetUserId = "user2" };

            var product = new Data.Entities.Product
            {
                Id = productId,
                Name = "Flour",
                ImageContent = [7, 8],
                ImageThumbnail = [9, 10],
                BaseUnitId = baseUnitId,
                ProductCategoryId = productCategoryId,
                ProductCategory = new Data.Entities.ProductCategory { Id = productCategoryId, Name = "Baking" },
                UserId = "user1"
            };

            var source = new Data.Entities.Recipe
            {
                Id = recipeId,
                Name = "My Recipe",
                Source = "cookbook",
                ImageContent = [1, 2, 3],
                ImageThumbnail = [4, 5, 6],
                RecipeCategoryId = categoryId,
                RecipeCategory = new Data.Entities.RecipeCategory { Id = categoryId, Name = "Desserts" },
                UserId = "user1",
                RecipeIngredients =
                [
                    new Data.Entities.RecipeIngredient { ProductId = productId, Product = product, UnitId = Guid.NewGuid(), Quantity = 2 }
                ]
            };

            _repoMock
                .Setup(r => r.GetByIdIncludeIngredientsAsync(recipeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            _repoMock
                .Setup(r => r.SearchAsync("My Recipe", categoryId, "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Recipe?)null);

            SetUpProductClone(product, clonedProductId);

            Data.Entities.Recipe? added = null;
            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Recipe>(), It.IsAny<CancellationToken>()))
                .Callback<Data.Entities.Recipe, CancellationToken>((r, _) => added = r)
                .ReturnsAsync((Data.Entities.Recipe r, CancellationToken _) => r);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            Assert.That(added, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(added!.Name, Is.EqualTo("My Recipe"));
                Assert.That(added.UserId, Is.EqualTo("user2"));
                Assert.That(added.Source, Is.EqualTo("cookbook"));
                Assert.That(added.RecipeCategoryId, Is.EqualTo(categoryId));
                Assert.That(added.RecipeIngredients, Has.Count.EqualTo(1));
                Assert.That(added.RecipeIngredients![0].ProductId, Is.EqualTo(clonedProductId));
                Assert.That(added.RecipeIngredients![0].ProductId, Is.Not.EqualTo(productId));
                Assert.That(added.RecipeIngredients![0].Quantity, Is.EqualTo(2));
            }

            _repoMock.Verify(r => r.SearchAsync("My Recipe", categoryId, "user2", It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Data.Entities.Recipe>(), It.IsAny<CancellationToken>()), Times.Once);
            _productRepoMock.Verify(r => r.AddAsync(It.IsAny<Data.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_MultipleIngredientsSameProduct_ClonesProductOnlyOnce()
        {
            var recipeId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var productCategoryId = Guid.NewGuid();
            var recipeCategoryId = Guid.NewGuid();
            var clonedProductId = Guid.NewGuid();
            var command = new ShareCommand { RecipeId = recipeId, TargetUserId = "user2" };

            var product = new Data.Entities.Product
            {
                Id = productId,
                Name = "Sugar",
                BaseUnitId = Guid.NewGuid(),
                ProductCategoryId = productCategoryId,
                ProductCategory = new Data.Entities.ProductCategory { Id = productCategoryId, Name = "Baking" },
                UserId = "user1"
            };

            var source = new Data.Entities.Recipe
            {
                Id = recipeId,
                Name = "My Recipe",
                RecipeCategoryId = recipeCategoryId,
                RecipeCategory = new Data.Entities.RecipeCategory { Id = recipeCategoryId, Name = "Desserts" },
                UserId = "user1",
                RecipeIngredients =
                [
                    new Data.Entities.RecipeIngredient { ProductId = productId, Product = product, UnitId = Guid.NewGuid(), Quantity = 1 },
                    new Data.Entities.RecipeIngredient { ProductId = productId, Product = product, UnitId = Guid.NewGuid(), Quantity = 3 }
                ]
            };

            _repoMock
                .Setup(r => r.GetByIdIncludeIngredientsAsync(recipeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            _repoMock
                .Setup(r => r.SearchAsync("My Recipe", recipeCategoryId, "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Recipe?)null);

            SetUpProductClone(product, clonedProductId);

            Data.Entities.Recipe? added = null;
            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Recipe>(), It.IsAny<CancellationToken>()))
                .Callback<Data.Entities.Recipe, CancellationToken>((r, _) => added = r)
                .ReturnsAsync((Data.Entities.Recipe r, CancellationToken _) => r);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);
            Assert.That(added!.RecipeIngredients, Has.Count.EqualTo(2));
            Assert.That(added.RecipeIngredients!.Select(ri => ri.ProductId), Is.All.EqualTo(clonedProductId));

            _productRepoMock.Verify(r => r.AddAsync(It.IsAny<Data.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_NameCollision_AppendsCopySuffix()
        {
            var recipeId = Guid.NewGuid();
            var recipeCategoryId = Guid.NewGuid();
            var command = new ShareCommand { RecipeId = recipeId, TargetUserId = "user2" };

            var source = new Data.Entities.Recipe
            {
                Id = recipeId,
                Name = "My Recipe",
                RecipeCategoryId = recipeCategoryId,
                RecipeCategory = new Data.Entities.RecipeCategory { Id = recipeCategoryId, Name = "Desserts" },
                UserId = "user1",
                RecipeIngredients = []
            };

            _repoMock
                .Setup(r => r.GetByIdIncludeIngredientsAsync(recipeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            _repoMock
                .Setup(r => r.SearchAsync("My Recipe", recipeCategoryId, "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Data.Entities.Recipe { Id = Guid.NewGuid(), Name = "My Recipe" });

            _repoMock
                .Setup(r => r.SearchAsync("My Recipe (Copy)", recipeCategoryId, "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Recipe?)null);

            Data.Entities.Recipe? added = null;
            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Recipe>(), It.IsAny<CancellationToken>()))
                .Callback<Data.Entities.Recipe, CancellationToken>((r, _) => added = r)
                .ReturnsAsync((Data.Entities.Recipe r, CancellationToken _) => r);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);
            Assert.That(added!.Name, Is.EqualTo("My Recipe (Copy)"));

            _repoMock.Verify(r => r.SearchAsync("My Recipe", recipeCategoryId, "user2", It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.SearchAsync("My Recipe (Copy)", recipeCategoryId, "user2", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_RepeatedNameCollision_IncrementsCopySuffix()
        {
            var recipeId = Guid.NewGuid();
            var recipeCategoryId = Guid.NewGuid();
            var command = new ShareCommand { RecipeId = recipeId, TargetUserId = "user2" };

            var source = new Data.Entities.Recipe
            {
                Id = recipeId,
                Name = "My Recipe",
                RecipeCategoryId = recipeCategoryId,
                RecipeCategory = new Data.Entities.RecipeCategory { Id = recipeCategoryId, Name = "Desserts" },
                UserId = "user1",
                RecipeIngredients = []
            };

            _repoMock
                .Setup(r => r.GetByIdIncludeIngredientsAsync(recipeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            _repoMock
                .Setup(r => r.SearchAsync("My Recipe", recipeCategoryId, "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Data.Entities.Recipe { Id = Guid.NewGuid(), Name = "My Recipe" });

            _repoMock
                .Setup(r => r.SearchAsync("My Recipe (Copy)", recipeCategoryId, "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Data.Entities.Recipe { Id = Guid.NewGuid(), Name = "My Recipe (Copy)" });

            _repoMock
                .Setup(r => r.SearchAsync("My Recipe (Copy 2)", recipeCategoryId, "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Recipe?)null);

            Data.Entities.Recipe? added = null;
            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Recipe>(), It.IsAny<CancellationToken>()))
                .Callback<Data.Entities.Recipe, CancellationToken>((r, _) => added = r)
                .ReturnsAsync((Data.Entities.Recipe r, CancellationToken _) => r);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);
            Assert.That(added!.Name, Is.EqualTo("My Recipe (Copy 2)"));
        }

        [Test]
        public async Task Handle_ExceptionDuringAdd_LogsError_AndReturnsFailedResponse()
        {
            var recipeId = Guid.NewGuid();
            var recipeCategoryId = Guid.NewGuid();
            var command = new ShareCommand { RecipeId = recipeId, TargetUserId = "user2" };

            var source = new Data.Entities.Recipe
            {
                Id = recipeId,
                Name = "My Recipe",
                RecipeCategoryId = recipeCategoryId,
                RecipeCategory = new Data.Entities.RecipeCategory { Id = recipeCategoryId, Name = "Desserts" },
                UserId = "user1",
                RecipeIngredients = []
            };

            _repoMock
                .Setup(r => r.GetByIdIncludeIngredientsAsync(recipeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            _repoMock
                .Setup(r => r.SearchAsync("My Recipe", recipeCategoryId, "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Recipe?)null);

            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Recipe>(), It.IsAny<CancellationToken>()))
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

        [Test]
        public async Task Handle_ExceptionDuringProductClone_LogsError_AndReturnsFailedResponse()
        {
            var recipeId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var productCategoryId = Guid.NewGuid();
            var recipeCategoryId = Guid.NewGuid();
            var command = new ShareCommand { RecipeId = recipeId, TargetUserId = "user2" };

            var product = new Data.Entities.Product
            {
                Id = productId,
                Name = "Flour",
                BaseUnitId = Guid.NewGuid(),
                ProductCategoryId = productCategoryId,
                ProductCategory = new Data.Entities.ProductCategory { Id = productCategoryId, Name = "Baking" },
                UserId = "user1"
            };

            var source = new Data.Entities.Recipe
            {
                Id = recipeId,
                Name = "My Recipe",
                RecipeCategoryId = recipeCategoryId,
                RecipeCategory = new Data.Entities.RecipeCategory { Id = recipeCategoryId, Name = "Desserts" },
                UserId = "user1",
                RecipeIngredients =
                [
                    new Data.Entities.RecipeIngredient { ProductId = productId, Product = product, UnitId = Guid.NewGuid(), Quantity = 1 }
                ]
            };

            _repoMock
                .Setup(r => r.GetByIdIncludeIngredientsAsync(recipeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            _repoMock
                .Setup(r => r.SearchAsync("My Recipe", recipeCategoryId, "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Recipe?)null);

            _productRepoMock
                .Setup(r => r.SearchAsync("Flour", productCategoryId, "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Product?)null);

            _productRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Product>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);

            _repoMock.Verify(r => r.AddAsync(It.IsAny<Data.Entities.Recipe>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_ResolvesRecipeAndProductCategoriesAgainstTargetUser()
        {
            var recipeId = Guid.NewGuid();
            var recipeCategoryId = Guid.NewGuid();
            var resolvedRecipeCategoryId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var productCategoryId = Guid.NewGuid();
            var resolvedProductCategoryId = Guid.NewGuid();
            var clonedProductId = Guid.NewGuid();
            var command = new ShareCommand { RecipeId = recipeId, TargetUserId = "user2" };

            var product = new Data.Entities.Product
            {
                Id = productId,
                Name = "Flour",
                BaseUnitId = Guid.NewGuid(),
                ProductCategoryId = productCategoryId,
                ProductCategory = new Data.Entities.ProductCategory { Id = productCategoryId, Name = "Baking" },
                UserId = "user1"
            };

            var source = new Data.Entities.Recipe
            {
                Id = recipeId,
                Name = "My Recipe",
                RecipeCategoryId = recipeCategoryId,
                RecipeCategory = new Data.Entities.RecipeCategory { Id = recipeCategoryId, Name = "Desserts" },
                UserId = "user1",
                RecipeIngredients =
                [
                    new Data.Entities.RecipeIngredient { ProductId = productId, Product = product, UnitId = Guid.NewGuid(), Quantity = 1 }
                ]
            };

            _recipeCategoryResolverMock.Reset();
            _recipeCategoryResolverMock
                .Setup(r => r.ResolveAsync(It.IsAny<IEnumerable<Data.Entities.RecipeCategory?>>(), "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Guid, Guid> { [recipeCategoryId] = resolvedRecipeCategoryId });

            _productCategoryResolverMock.Reset();
            _productCategoryResolverMock
                .Setup(r => r.ResolveAsync(It.IsAny<IEnumerable<Data.Entities.ProductCategory?>>(), "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Guid, Guid> { [productCategoryId] = resolvedProductCategoryId });

            _repoMock
                .Setup(r => r.GetByIdIncludeIngredientsAsync(recipeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            _repoMock
                .Setup(r => r.SearchAsync("My Recipe", resolvedRecipeCategoryId, "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Recipe?)null);

            _productRepoMock
                .Setup(r => r.SearchAsync("Flour", resolvedProductCategoryId, "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Product?)null);

            _productRepoMock
                .Setup(r => r.AddAsync(
                    It.Is<Data.Entities.Product>(p => p.ProductCategoryId == resolvedProductCategoryId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Product p, CancellationToken _) =>
                {
                    p.Id = clonedProductId;
                    return p;
                });

            Data.Entities.Recipe? added = null;
            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Recipe>(), It.IsAny<CancellationToken>()))
                .Callback<Data.Entities.Recipe, CancellationToken>((r, _) => added = r)
                .ReturnsAsync((Data.Entities.Recipe r, CancellationToken _) => r);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);
            Assert.That(added!.RecipeCategoryId, Is.EqualTo(resolvedRecipeCategoryId));

            _productRepoMock.Verify(
                r => r.AddAsync(
                    It.Is<Data.Entities.Product>(p => p.ProductCategoryId == resolvedProductCategoryId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Handle_ExistingProductWithSameName_ReusesExistingProduct_AndDoesNotCloneIt()
        {
            var recipeId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var productCategoryId = Guid.NewGuid();
            var recipeCategoryId = Guid.NewGuid();
            var existingProductId = Guid.NewGuid();
            var command = new ShareCommand { RecipeId = recipeId, TargetUserId = "user2" };

            var product = new Data.Entities.Product
            {
                Id = productId,
                Name = "Flour",
                BaseUnitId = Guid.NewGuid(),
                ProductCategoryId = productCategoryId,
                ProductCategory = new Data.Entities.ProductCategory { Id = productCategoryId, Name = "Baking" },
                UserId = "user1"
            };

            var source = new Data.Entities.Recipe
            {
                Id = recipeId,
                Name = "My Recipe",
                RecipeCategoryId = recipeCategoryId,
                RecipeCategory = new Data.Entities.RecipeCategory { Id = recipeCategoryId, Name = "Desserts" },
                UserId = "user1",
                RecipeIngredients =
                [
                    new Data.Entities.RecipeIngredient { ProductId = productId, Product = product, UnitId = Guid.NewGuid(), Quantity = 1 }
                ]
            };

            _repoMock
                .Setup(r => r.GetByIdIncludeIngredientsAsync(recipeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            _repoMock
                .Setup(r => r.SearchAsync("My Recipe", recipeCategoryId, "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Recipe?)null);

            _productRepoMock
                .Setup(r => r.SearchAsync("Flour", productCategoryId, "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Data.Entities.Product { Id = existingProductId, Name = "Flour", UserId = "user2" });

            Data.Entities.Recipe? added = null;
            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Recipe>(), It.IsAny<CancellationToken>()))
                .Callback<Data.Entities.Recipe, CancellationToken>((r, _) => added = r)
                .ReturnsAsync((Data.Entities.Recipe r, CancellationToken _) => r);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);
            Assert.That(added!.RecipeIngredients, Has.Count.EqualTo(1));
            Assert.That(added.RecipeIngredients![0].ProductId, Is.EqualTo(existingProductId));

            _productRepoMock.Verify(r => r.SearchAsync("Flour", productCategoryId, "user2", It.IsAny<CancellationToken>()), Times.Once);
            _productRepoMock.Verify(r => r.AddAsync(It.IsAny<Data.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
