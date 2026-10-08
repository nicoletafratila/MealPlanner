using Moq;
using RecipeBook.Api.Repositories;
using RecipeBook.Api.Services;
using RecipeBook.Data.Entities;

namespace RecipeBook.Api.Tests.Services
{
    [TestFixture]
    public class RecipeCategoryShareResolverTests
    {
        private Mock<IRecipeCategoryRepository> _repoMock = null!;
        private RecipeCategoryShareResolver _resolver = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IRecipeCategoryRepository>(MockBehavior.Strict);
            _resolver = new RecipeCategoryShareResolver(_repoMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new RecipeCategoryShareResolver(null!));
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

            var source = new RecipeCategory { Id = sourceId, Name = "Desserts", UserId = "user1", DisplaySequence = 0 };
            var target = new RecipeCategory { Id = targetId, Name = "Desserts", UserId = "user2", DisplaySequence = 0 };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync([target]);

            var result = await _resolver.ResolveAsync([source], "user2", CancellationToken.None);

            Assert.That(result[sourceId], Is.EqualTo(targetId));
            _repoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<RecipeCategory>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task ResolveAsync_CaseInsensitiveNameMatch_ReusesExistingTargetCategory()
        {
            var sourceId = Guid.NewGuid();
            var targetId = Guid.NewGuid();

            var source = new RecipeCategory { Id = sourceId, Name = "DESSERTS", UserId = "user1", DisplaySequence = 0 };
            var target = new RecipeCategory { Id = targetId, Name = "desserts", UserId = "user2", DisplaySequence = 0 };

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

            var source = new RecipeCategory { Id = sourceId, Name = "Desserts", UserId = "user1", DisplaySequence = 3 };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            RecipeCategory? added = null;
            _repoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<RecipeCategory>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<RecipeCategory>, CancellationToken>((entities, _) => added = entities.Single())
                .ReturnsAsync((IEnumerable<RecipeCategory> entities, CancellationToken _) =>
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
                Assert.That(added!.Name, Is.EqualTo("Desserts"));
                Assert.That(added.UserId, Is.EqualTo("user2"));
                Assert.That(added.DisplaySequence, Is.EqualTo(0));
            }
        }

        [Test]
        public async Task ResolveAsync_NoMatch_NewDisplaySequenceIsOnePastTargetMax()
        {
            var sourceId = Guid.NewGuid();

            var source = new RecipeCategory { Id = sourceId, Name = "Desserts", UserId = "user1", DisplaySequence = 0 };
            var existingTarget = new RecipeCategory { Id = Guid.NewGuid(), Name = "Mains", UserId = "user2", DisplaySequence = 5 };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync([existingTarget]);

            RecipeCategory? added = null;
            _repoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<RecipeCategory>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<RecipeCategory>, CancellationToken>((entities, _) => added = entities.Single())
                .ReturnsAsync((IEnumerable<RecipeCategory> entities, CancellationToken _) => entities.ToList());

            await _resolver.ResolveAsync([source], "user2", CancellationToken.None);

            Assert.That(added!.DisplaySequence, Is.EqualTo(6));
        }

        [Test]
        public async Task ResolveAsync_MultipleSourceCategoriesWithSameMissingName_CreatesOnlyOneNewCategory()
        {
            var sourceId1 = Guid.NewGuid();
            var sourceId2 = Guid.NewGuid();
            var createdId = Guid.NewGuid();

            var source1 = new RecipeCategory { Id = sourceId1, Name = "Desserts", UserId = "user1", DisplaySequence = 0 };
            var source2 = new RecipeCategory { Id = sourceId2, Name = "desserts", UserId = "user3", DisplaySequence = 1 };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            _repoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<RecipeCategory>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<RecipeCategory> entities, CancellationToken _) =>
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
                r => r.AddRangeAsync(It.Is<IEnumerable<RecipeCategory>>(e => e.Count() == 1), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task ResolveAsync_DuplicateSourceCategoryId_IsResolvedOnlyOnce()
        {
            var sourceId = Guid.NewGuid();
            var targetId = Guid.NewGuid();

            var source = new RecipeCategory { Id = sourceId, Name = "Desserts", UserId = "user1", DisplaySequence = 0 };
            var target = new RecipeCategory { Id = targetId, Name = "Desserts", UserId = "user2", DisplaySequence = 0 };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user2", It.IsAny<CancellationToken>()))
                .ReturnsAsync([target]);

            var result = await _resolver.ResolveAsync([source, source], "user2", CancellationToken.None);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[sourceId], Is.EqualTo(targetId));
        }
    }
}
