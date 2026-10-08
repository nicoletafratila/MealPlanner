using Moq;
using RecipeBook.Api.Repositories;
using RecipeBook.Api.Services;
using RecipeBook.Data.Entities;

namespace RecipeBook.Api.Tests.Services
{
    [TestFixture]
    public class ProductCategoryShareResolverTests
    {
        private Mock<IProductCategoryRepository> _repoMock = null!;
        private ProductCategoryShareResolver _resolver = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IProductCategoryRepository>(MockBehavior.Strict);
            _resolver = new ProductCategoryShareResolver(_repoMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new ProductCategoryShareResolver(null!));
        }

        [Test]
        public void ResolveAsync_NullSourceCategories_Throws()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _resolver.ResolveAsync(null!, "user2", CancellationToken.None));
        }

        [Test]
        public async Task ResolveAsync_EmptySourceCategories_ReturnsEmptyMap_AndDoesNotQueryRepository()
        {
            var result = await _resolver.ResolveAsync([], "user2", CancellationToken.None);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task ResolveAsync_AllNullSourceCategories_ReturnsEmptyMap()
        {
            var result = await _resolver.ResolveAsync([null, null], "user2", CancellationToken.None);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task ResolveAsync_ExactNameMatch_ReusesExistingTargetCategory()
        {
            var sourceId = Guid.NewGuid();
            var targetId = Guid.NewGuid();

            var source = new ProductCategory { Id = sourceId, Name = "Dairy", UserId = "user1" };
            var target = new ProductCategory { Id = targetId, Name = "Dairy", UserId = "user2" };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync([target]);

            var result = await _resolver.ResolveAsync([source], "user2", CancellationToken.None);

            Assert.That(result[sourceId], Is.EqualTo(targetId));
            _repoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<ProductCategory>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task ResolveAsync_CaseInsensitiveNameMatch_ReusesExistingTargetCategory()
        {
            var sourceId = Guid.NewGuid();
            var targetId = Guid.NewGuid();

            var source = new ProductCategory { Id = sourceId, Name = "DAIRY", UserId = "user1" };
            var target = new ProductCategory { Id = targetId, Name = "dairy", UserId = "user2" };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync([target]);

            var result = await _resolver.ResolveAsync([source], "user2", CancellationToken.None);

            Assert.That(result[sourceId], Is.EqualTo(targetId));
        }

        [Test]
        public async Task ResolveAsync_NoMatch_CreatesNewCategoryForTargetUser()
        {
            var sourceId = Guid.NewGuid();
            var createdId = Guid.NewGuid();

            var source = new ProductCategory { Id = sourceId, Name = "Dairy", UserId = "user1" };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            ProductCategory? added = null;
            _repoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ProductCategory>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ProductCategory>, CancellationToken>((entities, _) => added = entities.Single())
                .ReturnsAsync((IEnumerable<ProductCategory> entities, CancellationToken _) =>
                {
                    var list = entities.ToList();
                    foreach (var entity in list)
                        entity.Id = createdId;
                    return list;
                });

            var result = await _resolver.ResolveAsync([source], "user2", CancellationToken.None);

            Assert.That(result[sourceId], Is.EqualTo(createdId));
            Assert.That(added, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(added!.Name, Is.EqualTo("Dairy"));
                Assert.That(added.UserId, Is.EqualTo("user2"));
            }
        }

        [Test]
        public async Task ResolveAsync_MultipleSourceCategoriesWithSameMissingName_CreatesOnlyOneNewCategory()
        {
            var sourceId1 = Guid.NewGuid();
            var sourceId2 = Guid.NewGuid();
            var createdId = Guid.NewGuid();

            var source1 = new ProductCategory { Id = sourceId1, Name = "Dairy", UserId = "user1" };
            var source2 = new ProductCategory { Id = sourceId2, Name = "dairy", UserId = "user3" };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            _repoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ProductCategory>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<ProductCategory> entities, CancellationToken _) =>
                {
                    var list = entities.ToList();
                    foreach (var entity in list)
                        entity.Id = createdId;
                    return list;
                });

            var result = await _resolver.ResolveAsync([source1, source2], "user2", CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result[sourceId1], Is.EqualTo(createdId));
                Assert.That(result[sourceId2], Is.EqualTo(createdId));
            }

            _repoMock.Verify(
                r => r.AddRangeAsync(It.Is<IEnumerable<ProductCategory>>(e => e.Count() == 1), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task ResolveAsync_DuplicateSourceCategoryId_IsResolvedOnlyOnce()
        {
            var sourceId = Guid.NewGuid();
            var targetId = Guid.NewGuid();

            var source = new ProductCategory { Id = sourceId, Name = "Dairy", UserId = "user1" };
            var target = new ProductCategory { Id = targetId, Name = "Dairy", UserId = "user2" };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync([target]);

            var result = await _resolver.ResolveAsync([source, source], "user2", CancellationToken.None);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[sourceId], Is.EqualTo(targetId));
        }
    }
}
