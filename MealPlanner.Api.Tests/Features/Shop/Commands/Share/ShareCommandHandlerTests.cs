using MealPlanner.Api.Abstractions;
using MealPlanner.Api.Features.Shop.Commands.Share;
using MealPlanner.Api.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace MealPlanner.Api.Tests.Features.Shop.Commands.Share
{
    [TestFixture]
    public class ShareCommandHandlerTests
    {
        private Mock<IShopRepository> _repoMock = null!;
        private Mock<IRecipeBookClient> _recipeBookClientMock = null!;
        private Mock<ILogger<ShareCommandHandler>> _loggerMock = null!;
        private ShareCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IShopRepository>(MockBehavior.Strict);
            _recipeBookClientMock = new Mock<IRecipeBookClient>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<ShareCommandHandler>>(MockBehavior.Loose);

            _handler = new ShareCommandHandler(_repoMock.Object, _recipeBookClientMock.Object, _loggerMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareCommandHandler(null!, _recipeBookClientMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullRecipeBookClient_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareCommandHandler(_repoMock.Object, null!, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareCommandHandler(_repoMock.Object, _recipeBookClientMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_ShopNotFound_ReturnsFailedResponse_AndDoesNotAdd()
        {
            var shopId = Guid.NewGuid();
            var command = new ShareCommand { ShopId = shopId, TargetUserId = "user2" };

            _repoMock
                .Setup(r => r.GetByIdIncludeDisplaySequenceAsync(shopId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Data.Entities.Shop?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);

            _repoMock.Verify(r => r.AddAsync(It.IsAny<Data.Entities.Shop>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoDisplaySequence_DoesNotCallRecipeBookClient_AndSharesWithOriginalName()
        {
            var shopId = Guid.NewGuid();
            var command = new ShareCommand { ShopId = shopId, TargetUserId = "user2" };

            var source = new Data.Entities.Shop { Id = shopId, Name = "Kaufland", UserId = "user1", DisplaySequence = [] };

            _repoMock
                .Setup(r => r.GetByIdIncludeDisplaySequenceAsync(shopId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            Data.Entities.Shop? added = null;
            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Shop>(), It.IsAny<CancellationToken>()))
                .Callback<Data.Entities.Shop, CancellationToken>((s, _) => added = s)
                .ReturnsAsync((Data.Entities.Shop s, CancellationToken _) => s);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);
            Assert.That(added, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(added!.Name, Is.EqualTo("Kaufland"));
                Assert.That(added.UserId, Is.EqualTo("user2"));
                Assert.That(added.DisplaySequence, Is.Empty);
            }

            _recipeBookClientMock.Verify(
                c => c.ResolveShareProductCategoriesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Handle_WithDisplaySequence_ResolvesCategoriesAndRemapsThem()
        {
            var shopId = Guid.NewGuid();
            var categoryId1 = Guid.NewGuid();
            var categoryId2 = Guid.NewGuid();
            var resolvedCategoryId1 = Guid.NewGuid();
            var resolvedCategoryId2 = Guid.NewGuid();
            var command = new ShareCommand { ShopId = shopId, TargetUserId = "user2", AuthToken = "token123" };

            var source = new Data.Entities.Shop
            {
                Id = shopId,
                Name = "Kaufland",
                UserId = "user1",
                DisplaySequence =
                [
                    new Data.Entities.ShopDisplaySequence { Value = 1, ProductCategoryId = categoryId1 },
                    new Data.Entities.ShopDisplaySequence { Value = 2, ProductCategoryId = categoryId2 }
                ]
            };

            var categoryMap = new Dictionary<Guid, Guid> { [categoryId1] = resolvedCategoryId1, [categoryId2] = resolvedCategoryId2 };

            _repoMock
                .Setup(r => r.GetByIdIncludeDisplaySequenceAsync(shopId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            _recipeBookClientMock
                .Setup(c => c.ResolveShareProductCategoriesAsync(
                    It.Is<IEnumerable<Guid>>(ids => ids.OrderBy(x => x).SequenceEqual(new[] { categoryId1, categoryId2 }.OrderBy(x => x))),
                    "user2",
                    "token123",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoryMap);

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            Data.Entities.Shop? added = null;
            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Shop>(), It.IsAny<CancellationToken>()))
                .Callback<Data.Entities.Shop, CancellationToken>((s, _) => added = s)
                .ReturnsAsync((Data.Entities.Shop s, CancellationToken _) => s);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);
            Assert.That(added!.DisplaySequence, Has.Count.EqualTo(2));
            Assert.That(added.DisplaySequence!.Select(ds => ds.ProductCategoryId), Is.EquivalentTo(new[] { resolvedCategoryId1, resolvedCategoryId2 }));
        }

        [Test]
        public async Task Handle_NameCollision_AppendsCopySuffix()
        {
            var shopId = Guid.NewGuid();
            var command = new ShareCommand { ShopId = shopId, TargetUserId = "user2" };

            var source = new Data.Entities.Shop { Id = shopId, Name = "Kaufland", UserId = "user1", DisplaySequence = [] };

            _repoMock
                .Setup(r => r.GetByIdIncludeDisplaySequenceAsync(shopId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync([new Data.Entities.Shop { Id = Guid.NewGuid(), Name = "Kaufland", UserId = "user2" }]);

            Data.Entities.Shop? added = null;
            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Shop>(), It.IsAny<CancellationToken>()))
                .Callback<Data.Entities.Shop, CancellationToken>((s, _) => added = s)
                .ReturnsAsync((Data.Entities.Shop s, CancellationToken _) => s);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);
            Assert.That(added!.Name, Is.EqualTo("Kaufland (Copy)"));
        }

        [Test]
        public async Task Handle_ExceptionDuringAdd_LogsError_AndReturnsFailedResponse()
        {
            var shopId = Guid.NewGuid();
            var command = new ShareCommand { ShopId = shopId, TargetUserId = "user2" };

            var source = new Data.Entities.Shop { Id = shopId, Name = "Kaufland", UserId = "user1", DisplaySequence = [] };

            _repoMock
                .Setup(r => r.GetByIdIncludeDisplaySequenceAsync(shopId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Shop>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);

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
