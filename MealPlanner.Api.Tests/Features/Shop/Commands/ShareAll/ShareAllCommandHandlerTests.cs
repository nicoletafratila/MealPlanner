using Common.Services;
using MealPlanner.Api.Abstractions;
using MealPlanner.Api.Features.Shop.Commands.ShareAll;
using MealPlanner.Api.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace MealPlanner.Api.Tests.Features.Shop.Commands.ShareAll
{
    [TestFixture]
    public class ShareAllCommandHandlerTests
    {
        private Mock<IShopRepository> _repoMock = null!;
        private Mock<IRecipeBookClient> _recipeBookClientMock = null!;
        private Mock<ICurrentUserService> _currentUserMock = null!;
        private Mock<ILogger<ShareAllCommandHandler>> _loggerMock = null!;
        private ShareAllCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IShopRepository>(MockBehavior.Strict);
            _recipeBookClientMock = new Mock<IRecipeBookClient>(MockBehavior.Strict);
            _currentUserMock = new Mock<ICurrentUserService>(MockBehavior.Loose);
            _loggerMock = new Mock<ILogger<ShareAllCommandHandler>>(MockBehavior.Loose);

            _currentUserMock.Setup(s => s.UserId).Returns("user1");

            _handler = new ShareAllCommandHandler(
                _repoMock.Object,
                _recipeBookClientMock.Object,
                _currentUserMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareAllCommandHandler(null!, _recipeBookClientMock.Object, _currentUserMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullRecipeBookClient_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareAllCommandHandler(_repoMock.Object, null!, _currentUserMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullCurrentUserService_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareAllCommandHandler(_repoMock.Object, _recipeBookClientMock.Object, null!, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareAllCommandHandler(_repoMock.Object, _recipeBookClientMock.Object, _currentUserMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_NoUserId_ReturnsFailedResponse()
        {
            _currentUserMock.Setup(s => s.UserId).Returns((string?)null);
            var command = new ShareAllCommand { TargetUserId = "user2" };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);
        }

        [Test]
        public async Task Handle_NoShops_ReturnsSuccess_AndDoesNotAddAnything()
        {
            var command = new ShareAllCommand { TargetUserId = "user2" };

            _repoMock
                .Setup(r => r.GetAllByUserIncludeDisplaySequenceAsync("user1", command.Filters, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);
            _repoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Data.Entities.Shop>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_MultipleShops_ResolvesCategoriesOnceAndDedupsNames()
        {
            var command = new ShareAllCommand { TargetUserId = "user2", AuthToken = "token123" };

            var categoryId1 = Guid.NewGuid();
            var categoryId2 = Guid.NewGuid();
            var resolvedCategoryId1 = Guid.NewGuid();
            var resolvedCategoryId2 = Guid.NewGuid();

            var shop1 = new Data.Entities.Shop
            {
                Id = Guid.NewGuid(),
                Name = "Kaufland",
                UserId = "user1",
                DisplaySequence = [new Data.Entities.ShopDisplaySequence { Value = 1, ProductCategoryId = categoryId1 }]
            };
            var shop2 = new Data.Entities.Shop
            {
                Id = Guid.NewGuid(),
                Name = "Lidl",
                UserId = "user1",
                DisplaySequence = [new Data.Entities.ShopDisplaySequence { Value = 1, ProductCategoryId = categoryId2 }]
            };

            _repoMock
                .Setup(r => r.GetAllByUserIncludeDisplaySequenceAsync("user1", command.Filters, It.IsAny<CancellationToken>()))
                .ReturnsAsync([shop1, shop2]);

            _recipeBookClientMock
                .Setup(c => c.ResolveShareProductCategoriesAsync(
                    It.Is<IEnumerable<Guid>>(ids => ids.OrderBy(x => x).SequenceEqual(new[] { categoryId1, categoryId2 }.OrderBy(x => x))),
                    "user2",
                    "token123",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Guid, Guid> { [categoryId1] = resolvedCategoryId1, [categoryId2] = resolvedCategoryId2 });

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync([new Data.Entities.Shop { Id = Guid.NewGuid(), Name = "Kaufland", UserId = "user2" }]);

            IEnumerable<Data.Entities.Shop>? added = null;
            _repoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Data.Entities.Shop>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<Data.Entities.Shop>, CancellationToken>((s, _) => added = s)
                .ReturnsAsync((IEnumerable<Data.Entities.Shop> s, CancellationToken _) => s.ToList());

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);
            Assert.That(added, Is.Not.Null);

            var addedList = added!.ToList();
            Assert.That(addedList, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(addedList.Single(s => s.Name == "Kaufland (Copy)").DisplaySequence!.Single().ProductCategoryId, Is.EqualTo(resolvedCategoryId1));
                Assert.That(addedList.Single(s => s.Name == "Lidl").DisplaySequence!.Single().ProductCategoryId, Is.EqualTo(resolvedCategoryId2));
            }

            _recipeBookClientMock.Verify(
                c => c.ResolveShareProductCategoriesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Handle_ExceptionDuringAdd_LogsError_AndReturnsFailedResponse()
        {
            var command = new ShareAllCommand { TargetUserId = "user2" };

            var shop = new Data.Entities.Shop { Id = Guid.NewGuid(), Name = "Kaufland", UserId = "user1", DisplaySequence = [] };

            _repoMock
                .Setup(r => r.GetAllByUserIncludeDisplaySequenceAsync("user1", command.Filters, It.IsAny<CancellationToken>()))
                .ReturnsAsync([shop]);

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            _repoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Data.Entities.Shop>>(), It.IsAny<CancellationToken>()))
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
