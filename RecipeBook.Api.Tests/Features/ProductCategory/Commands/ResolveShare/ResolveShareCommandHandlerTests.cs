using Moq;
using RecipeBook.Api.Features.ProductCategory.Commands.ResolveShare;
using RecipeBook.Api.Repositories;
using RecipeBook.Api.Services;

namespace RecipeBook.Api.Tests.Features.ProductCategory.Commands.ResolveShare
{
    [TestFixture]
    public class ResolveShareCommandHandlerTests
    {
        private Mock<IProductCategoryRepository> _repoMock = null!;
        private Mock<IProductCategoryShareResolver> _resolverMock = null!;
        private ResolveShareCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IProductCategoryRepository>(MockBehavior.Strict);
            _resolverMock = new Mock<IProductCategoryShareResolver>(MockBehavior.Strict);
            _handler = new ResolveShareCommandHandler(_repoMock.Object, _resolverMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ResolveShareCommandHandler(null!, _resolverMock.Object));
        }

        [Test]
        public void Ctor_NullResolver_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ResolveShareCommandHandler(_repoMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_FiltersCategoriesByIdAndDelegatesToResolver()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();

            var category1 = new Data.Entities.ProductCategory { Id = id1, Name = "Cat1" };
            var category2 = new Data.Entities.ProductCategory { Id = id2, Name = "Cat2" };
            var category3 = new Data.Entities.ProductCategory { Id = id3, Name = "Cat3" };

            _repoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync([category1, category2, category3]);

            var expectedMap = new Dictionary<Guid, Guid> { [id1] = Guid.NewGuid(), [id3] = Guid.NewGuid() };

            _resolverMock
                .Setup(r => r.ResolveAsync(
                    It.Is<IEnumerable<Data.Entities.ProductCategory?>>(list => list.Select(c => c!.Id).OrderBy(x => x).SequenceEqual(new[] { id1, id3 }.OrderBy(x => x))),
                    "user2",
                    CancellationToken.None))
                .ReturnsAsync(expectedMap);

            var command = new ResolveShareCommand { CategoryIds = [id1, id3], TargetUserId = "user2" };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.EqualTo(expectedMap));
            _repoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Once);
            _resolverMock.VerifyAll();
        }

        [Test]
        public async Task Handle_RepositoryReturnsNull_TreatedAsEmpty()
        {
            _repoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync([]);

            _resolverMock
                .Setup(r => r.ResolveAsync(It.Is<IEnumerable<Data.Entities.ProductCategory?>>(list => !list.Any()), "user2", CancellationToken.None))
                .ReturnsAsync([]);

            var command = new ResolveShareCommand { CategoryIds = [Guid.NewGuid()], TargetUserId = "user2" };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Empty);
        }
    }
}
