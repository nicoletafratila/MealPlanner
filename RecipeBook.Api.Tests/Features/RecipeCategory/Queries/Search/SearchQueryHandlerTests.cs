using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Common.Pagination;
using Moq;
using NUnit.Framework;
using RecipeBook.Api.Features.RecipeCategory.Queries.Search;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;
using RecipeCategoryEntity = Common.Data.Entities.RecipeCategory;

namespace RecipeBook.Api.Tests.Features.RecipeCategory.Queries.Search
{
    [TestFixture]
    public class SearchQueryHandlerTests
    {
        private Mock<IRecipeCategoryRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private SearchQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IRecipeCategoryRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _handler = new SearchQueryHandler(_repoMock.Object, _mapperMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() =>
                _ = new SearchQueryHandler(null!, _mapperMock.Object));
        }

        [Test]
        public void Ctor_NullMapper_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() =>
                _ = new SearchQueryHandler(_repoMock.Object, null!));
        }

        [Test]
        public async Task Handle_NullRequest_ReturnsEmptyPagedList()
        {
            // Act
            var result = await _handler.Handle(null!, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.Zero);
            }
            _repoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Never);
            _mapperMock.Verify(m => m.Map<IList<RecipeCategoryModel>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Handle_NullQueryParameters_ReturnsEmptyPagedList()
        {
            // Arrange
            var query = new SearchQuery
            {
                QueryParameters = null
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.Zero);
            }
            _repoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Never);
            _mapperMock.Verify(m => m.Map<IList<RecipeCategoryModel>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoFiltersOrSorting_MapsAndPaginatesAllResults()
        {
            // Arrange
            var entities = new List<RecipeCategoryEntity>
            {
                new() { Id = 1, Name = "Cat1", DisplaySequence = 1 },
                new() { Id = 2, Name = "Cat2", DisplaySequence = 2 }
            };

            var models = new List<RecipeCategoryModel>
            {
                new() { Id = 1, Name = "Cat1", DisplaySequence = 1 },
                new() { Id = 2, Name = "Cat2", DisplaySequence = 2 }
            };

            _repoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<RecipeCategoryModel>>(entities))
                .Returns(models);

            var qp = new QueryParameters<RecipeCategoryModel>
            {
                Filters = null,
                Sorting = null,
                PageNumber = 1,
                PageSize = 10
            };

            var query = new SearchQuery
            {
                QueryParameters = qp
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result.Items, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items.Select(x => x.Id), Is.EquivalentTo([1, 2]));
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(2));
            }

            _repoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<RecipeCategoryModel>>(entities), Times.Once);
        }

        [Test]
        public async Task Handle_MapperReturnsNull_HandledAsEmptyList()
        {
            // Arrange
            var entities = new List<RecipeCategoryEntity>
            {
                new() { Id = 1, Name = "Cat1", DisplaySequence = 1 }
            };

            _repoMock
                .Setup(r => r.GetAllAsync(CancellationToken.None))
                .ReturnsAsync(entities);

            _mapperMock
                .Setup(m => m.Map<IList<RecipeCategoryModel>>(entities))
                .Returns([]);

            var qp = new QueryParameters<RecipeCategoryModel>
            {
                Filters = null,
                Sorting = null,
                PageNumber = 1,
                PageSize = 10
            };

            var query = new SearchQuery
            {
                QueryParameters = qp
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.Zero);
            }

            _repoMock.Verify(r => r.GetAllAsync(CancellationToken.None), Times.Once);
            _mapperMock.Verify(m => m.Map<IList<RecipeCategoryModel>>(entities), Times.Once);
        }
    }
}