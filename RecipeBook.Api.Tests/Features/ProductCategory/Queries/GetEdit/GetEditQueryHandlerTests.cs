using AutoMapper;
using Moq;
using RecipeBook.Api.Features.ProductCategory.Queries.GetEdit;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.ProductCategory.Queries.GetEdit
{
    [TestFixture]
    public class GetEditQueryHandlerTests
    {
        private Mock<IProductCategoryRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private GetEditQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IProductCategoryRepository>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _handler = new GetEditQueryHandler(_repoMock.Object, _mapperMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new GetEditQueryHandler(null!, _mapperMock.Object));
        }

        [Test]
        public void Ctor_NullMapper_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new GetEditQueryHandler(_repoMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_EntityFound_ReturnsMappedModel()
        {
            // Arrange
            const int id = 5;
            var entity = new Common.Data.Entities.ProductCategory
            {
                Id = id,
                Name = "Category1"
            };

            var mapped = new ProductCategoryEditModel
            {
                Id = id,
                Name = "Category1"
            };

            _repoMock
                .Setup(r => r.GetByIdAsync(id, CancellationToken.None   ))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<ProductCategoryEditModel>(entity))
                .Returns(mapped);

            var query = new GetEditQuery(id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.EqualTo("Category1"));
            }

            _repoMock.Verify(r => r.GetByIdAsync(id, CancellationToken.None), Times.Once);
            _mapperMock.Verify(m => m.Map<ProductCategoryEditModel>(entity), Times.Once);
        }

        [Test]
        public async Task Handle_EntityNotFound_ReturnsEmptyModelWithId()
        {
            // Arrange
            const int id = 10;

            _repoMock
                .Setup(r => r.GetByIdAsync(id, CancellationToken.None))
                .ReturnsAsync((Common.Data.Entities.ProductCategory?)null);

            var query = new GetEditQuery(id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.Null.Or.Empty);
            }

            _repoMock.Verify(r => r.GetByIdAsync(id, CancellationToken.None), Times.Once);
            _mapperMock.Verify(m => m.Map<ProductCategoryEditModel>(It.IsAny<Common.Data.Entities.ProductCategory>()), Times.Never);
        }

        [Test]
        public async Task Handle_MapperReturnsNull_FallsBackToEmptyModelWithId()
        {
            // Arrange
            const int id = 7;
            var entity = new Common.Data.Entities.ProductCategory
            {
                Id = id,
                Name = "CategoryX"
            };

            _repoMock
                .Setup(r => r.GetByIdAsync(id, CancellationToken.None))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<ProductCategoryEditModel?>(entity))
                .Returns((ProductCategoryEditModel?)null);

            var query = new GetEditQuery(id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(id));
            }

            _repoMock.Verify(r => r.GetByIdAsync(id, CancellationToken.None), Times.Once);
            _mapperMock.Verify(m => m.Map<ProductCategoryEditModel>(entity), Times.Once);
        }
    }
}