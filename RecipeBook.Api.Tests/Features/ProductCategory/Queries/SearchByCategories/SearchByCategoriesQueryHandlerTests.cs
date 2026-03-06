using AutoMapper;
using Moq;
using RecipeBook.Api.Features.ProductCategory.Queries.SearchByCategories;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.ProductCategory.Queries.SearchByCategories
{
    [TestFixture]
    public class SearchByCategoriesQueryHandlerTests
    {
        private Mock<IProductCategoryRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private SearchByCategoriesQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IProductCategoryRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _handler = new SearchByCategoriesQueryHandler(_repoMock.Object, _mapperMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new SearchByCategoriesQueryHandler(null!, _mapperMock.Object));
        }

        [Test]
        public void Ctor_NullMapper_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new SearchByCategoriesQueryHandler(_repoMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_EmptyCategoryIds_ReturnsAllCategories()
        {
            // Arrange
            var entities = new List<Common.Data.Entities.ProductCategory>
            {
                new() { Id = 1, Name = "Cat1" },
                new() { Id = 2, Name = "Cat2" }
            };

            var models = new List<ProductCategoryModel>
            {
                new() { Id = 1, Name = "Cat1" },
                new() { Id = 2, Name = "Cat2" }
            };

            _repoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<ProductCategoryModel>>(entities))
                .Returns(models);

            var query = new SearchByCategoriesQuery
            {
                CategoryIds = null
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(c => c.Id), Is.EquivalentTo([1, 2]));

            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<ProductCategoryModel>>(entities), Times.Once);
        }

        [Test]
        public async Task Handle_WithCategoryIds_FiltersCategoriesById()
        {
            // Arrange
            var entities = new List<Common.Data.Entities.ProductCategory>
            {
                new() { Id = 1, Name = "Cat1" },
                new() { Id = 2, Name = "Cat2" },
                new() { Id = 3, Name = "Cat3" }
            };

            var filteredEntities = entities.Where(e => e.Id == 1 || e.Id == 3).ToList();

            var models = new List<ProductCategoryModel>
            {
                new() { Id = 1, Name = "Cat1" },
                new() { Id = 3, Name = "Cat3" }
            };

            _repoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<ProductCategoryModel>>(
                    It.Is<IList<Common.Data.Entities.ProductCategory>>(list =>
                        list.Count == 2 &&
                        list.Any(e => e.Id == 1) &&
                        list.Any(e => e.Id == 3))))
                .Returns(models);

            var query = new SearchByCategoriesQuery
            {
                CategoryIds = "1, 3"
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(c => c.Id), Is.EquivalentTo([1, 3]));

            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _mapperMock.VerifyAll();
        }

        [Test]
        public async Task Handle_InvalidCategoryIds_AreIgnored()
        {
            // Arrange
            var entities = new List<Common.Data.Entities.ProductCategory>
            {
                new() { Id = 1, Name = "Cat1" },
                new() { Id = 2, Name = "Cat2" }
            };

            var filtered = entities.Where(e => e.Id == 1).ToList();

            var models = new List<ProductCategoryModel>
            {
                new() { Id = 1, Name = "Cat1" }
            };

            _repoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<ProductCategoryModel>>(
                    It.Is<IList<Common.Data.Entities.ProductCategory>>(list =>
                        list.Count == 1 && list[0].Id == 1)))
                .Returns(models);

            var query = new SearchByCategoriesQuery
            {
                CategoryIds = "1,xyz"
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(1));

            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _mapperMock.VerifyAll();
        }

        [Test]
        public async Task Handle_RepositoryReturnsNull_TreatedAsEmpty()
        {
            // Arrange
            _repoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync([]);

            _mapperMock
                .Setup(m => m.Map<IList<ProductCategoryModel>>(It.Is<IList<Common.Data.Entities.ProductCategory>>(list => list.Count == 0)))
                .Returns([]);

            var query = new SearchByCategoriesQuery
            {
                CategoryIds = "1,2"
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);

            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _mapperMock.VerifyAll();
        }
    }
}