using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Api.Features.Product.Commands.Share;
using RecipeBook.Api.Repositories;
using RecipeBook.Api.Services;

namespace RecipeBook.Api.Tests.Features.Product.Commands.Share
{
    [TestFixture]
    public class ShareCommandHandlerTests
    {
        private Mock<IProductRepository> _repoMock = null!;
        private Mock<IProductCategoryShareResolver> _productCategoryResolverMock = null!;
        private Mock<ILogger<ShareCommandHandler>> _loggerMock = null!;
        private ShareCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IProductRepository>(MockBehavior.Strict);
            _productCategoryResolverMock = new Mock<IProductCategoryShareResolver>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<ShareCommandHandler>>(MockBehavior.Loose);

            SetUpIdentityCategoryResolver();

            _handler = new ShareCommandHandler(
                _repoMock.Object,
                _productCategoryResolverMock.Object,
                _loggerMock.Object);
        }

        // By default, the category resolver mock maps every distinct source category to itself, so tests that don't
        // care about category remapping can assert on the pre-existing (source) category id unchanged.
        private void SetUpIdentityCategoryResolver()
        {
            _productCategoryResolverMock
                .Setup(r => r.ResolveAsync(It.IsAny<IEnumerable<Data.Entities.ProductCategory?>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<Data.Entities.ProductCategory?> categories, string _, CancellationToken _) =>
                    categories.Where(c => c is not null).Select(c => c!.Id).Distinct().ToDictionary(id => id, id => id));
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareCommandHandler(null!, _productCategoryResolverMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullProductCategoryResolver_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareCommandHandler(_repoMock.Object, null!, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareCommandHandler(_repoMock.Object, _productCategoryResolverMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_ProductNotFound_ReturnsFailedResponse_AndDoesNotAdd()
        {
            var productId = Guid.NewGuid();
            var command = new ShareCommand { ProductId = productId, TargetUserId = "user2" };

            _repoMock
                .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Product?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo($"Could not find with id {productId}"));
            }

            _repoMock.Verify(r => r.AddAsync(It.IsAny<Data.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoNameCollision_SharesWithOriginalName()
        {
            var productId = Guid.NewGuid();
            var productCategoryId = Guid.NewGuid();
            var baseUnitId = Guid.NewGuid();
            var command = new ShareCommand { ProductId = productId, TargetUserId = "user2" };

            var source = new Data.Entities.Product
            {
                Id = productId,
                Name = "Flour",
                ImageContent = [1, 2, 3],
                ImageThumbnail = [4, 5, 6],
                BaseUnitId = baseUnitId,
                ProductCategoryId = productCategoryId,
                ProductCategory = new Data.Entities.ProductCategory { Id = productCategoryId, Name = "Baking" },
                UserId = "user1"
            };

            _repoMock
                .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            _repoMock
                .Setup(r => r.SearchAsync("Flour", "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Product?)null);

            Data.Entities.Product? added = null;
            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Product>(), It.IsAny<CancellationToken>()))
                .Callback<Data.Entities.Product, CancellationToken>((p, _) => added = p)
                .ReturnsAsync((Data.Entities.Product p, CancellationToken _) => p);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            Assert.That(added, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(added!.Name, Is.EqualTo("Flour"));
                Assert.That(added.UserId, Is.EqualTo("user2"));
                Assert.That(added.BaseUnitId, Is.EqualTo(baseUnitId));
                Assert.That(added.ProductCategoryId, Is.EqualTo(productCategoryId));
                Assert.That(added.ImageContent, Is.EqualTo(source.ImageContent));
                Assert.That(added.ImageThumbnail, Is.EqualTo(source.ImageThumbnail));
            }

            _repoMock.Verify(r => r.SearchAsync("Flour", "user2", It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Data.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_NameCollision_AppendsCopySuffix()
        {
            var productId = Guid.NewGuid();
            var productCategoryId = Guid.NewGuid();
            var command = new ShareCommand { ProductId = productId, TargetUserId = "user2" };

            var source = new Data.Entities.Product
            {
                Id = productId,
                Name = "Flour",
                BaseUnitId = Guid.NewGuid(),
                ProductCategoryId = productCategoryId,
                ProductCategory = new Data.Entities.ProductCategory { Id = productCategoryId, Name = "Baking" },
                UserId = "user1"
            };

            _repoMock
                .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            _repoMock
                .Setup(r => r.SearchAsync("Flour", "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Data.Entities.Product { Id = Guid.NewGuid(), Name = "Flour" });

            _repoMock
                .Setup(r => r.SearchAsync("Flour (Copy)", "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Product?)null);

            Data.Entities.Product? added = null;
            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Product>(), It.IsAny<CancellationToken>()))
                .Callback<Data.Entities.Product, CancellationToken>((p, _) => added = p)
                .ReturnsAsync((Data.Entities.Product p, CancellationToken _) => p);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);
            Assert.That(added!.Name, Is.EqualTo("Flour (Copy)"));

            _repoMock.Verify(r => r.SearchAsync("Flour", "user2", It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.SearchAsync("Flour (Copy)", "user2", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_RepeatedNameCollision_IncrementsCopySuffix()
        {
            var productId = Guid.NewGuid();
            var productCategoryId = Guid.NewGuid();
            var command = new ShareCommand { ProductId = productId, TargetUserId = "user2" };

            var source = new Data.Entities.Product
            {
                Id = productId,
                Name = "Flour",
                BaseUnitId = Guid.NewGuid(),
                ProductCategoryId = productCategoryId,
                ProductCategory = new Data.Entities.ProductCategory { Id = productCategoryId, Name = "Baking" },
                UserId = "user1"
            };

            _repoMock
                .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            _repoMock
                .Setup(r => r.SearchAsync("Flour", "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Data.Entities.Product { Id = Guid.NewGuid(), Name = "Flour" });

            _repoMock
                .Setup(r => r.SearchAsync("Flour (Copy)", "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Data.Entities.Product { Id = Guid.NewGuid(), Name = "Flour (Copy)" });

            _repoMock
                .Setup(r => r.SearchAsync("Flour (Copy 2)", "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Product?)null);

            Data.Entities.Product? added = null;
            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Product>(), It.IsAny<CancellationToken>()))
                .Callback<Data.Entities.Product, CancellationToken>((p, _) => added = p)
                .ReturnsAsync((Data.Entities.Product p, CancellationToken _) => p);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);
            Assert.That(added!.Name, Is.EqualTo("Flour (Copy 2)"));
        }

        [Test]
        public async Task Handle_ExceptionDuringAdd_LogsError_AndReturnsFailedResponse()
        {
            var productId = Guid.NewGuid();
            var productCategoryId = Guid.NewGuid();
            var command = new ShareCommand { ProductId = productId, TargetUserId = "user2" };

            var source = new Data.Entities.Product
            {
                Id = productId,
                Name = "Flour",
                BaseUnitId = Guid.NewGuid(),
                ProductCategoryId = productCategoryId,
                ProductCategory = new Data.Entities.ProductCategory { Id = productCategoryId, Name = "Baking" },
                UserId = "user1"
            };

            _repoMock
                .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            _repoMock
                .Setup(r => r.SearchAsync("Flour", "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Product?)null);

            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Product>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("An error occurred when saving the product."));
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
        public async Task Handle_ResolvesProductCategoryAgainstTargetUser()
        {
            var productId = Guid.NewGuid();
            var productCategoryId = Guid.NewGuid();
            var resolvedProductCategoryId = Guid.NewGuid();
            var command = new ShareCommand { ProductId = productId, TargetUserId = "user2" };

            var source = new Data.Entities.Product
            {
                Id = productId,
                Name = "Flour",
                BaseUnitId = Guid.NewGuid(),
                ProductCategoryId = productCategoryId,
                ProductCategory = new Data.Entities.ProductCategory { Id = productCategoryId, Name = "Baking" },
                UserId = "user1"
            };

            _productCategoryResolverMock.Reset();
            _productCategoryResolverMock
                .Setup(r => r.ResolveAsync(It.IsAny<IEnumerable<Data.Entities.ProductCategory?>>(), "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Guid, Guid> { [productCategoryId] = resolvedProductCategoryId });

            _repoMock
                .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            _repoMock
                .Setup(r => r.SearchAsync("Flour", "user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Product?)null);

            Data.Entities.Product? added = null;
            _repoMock
                .Setup(r => r.AddAsync(
                    It.Is<Data.Entities.Product>(p => p.ProductCategoryId == resolvedProductCategoryId),
                    It.IsAny<CancellationToken>()))
                .Callback<Data.Entities.Product, CancellationToken>((p, _) => added = p)
                .ReturnsAsync((Data.Entities.Product p, CancellationToken _) => p);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);
            Assert.That(added!.ProductCategoryId, Is.EqualTo(resolvedProductCategoryId));

            _repoMock.Verify(
                r => r.AddAsync(
                    It.Is<Data.Entities.Product>(p => p.ProductCategoryId == resolvedProductCategoryId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
