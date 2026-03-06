using AutoMapper;
using Moq;
using RecipeBook.Api.Features.Product.Queries.GetEdit;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.Product.Queries.GetEdit
{
    [TestFixture]
    public class GetEditQueryHandlerTests
    {
        private Mock<IProductRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private GetEditQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IProductRepository>(MockBehavior.Strict);
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
            var entity = new Common.Data.Entities.Product
            {
                Id = id,
                Name = "Product1",
                ProductCategoryId = 10
            };

            var mapped = new ProductEditModel
            {
                Id = id,
                Name = "Product1",
                ProductCategoryId = 10
            };

            _repoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<ProductEditModel>(entity))
                .Returns(mapped);

            var query = new GetEditQuery(id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.EqualTo("Product1"));
            }

            _repoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
            _mapperMock.Verify(m => m.Map<ProductEditModel>(entity), Times.Once);
        }

        [Test]
        public async Task Handle_EntityNotFound_ReturnsEmptyModelWithId()
        {
            // Arrange
            const int id = 10;

            _repoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((Common.Data.Entities.Product?)null);

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

            _repoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
            _mapperMock.Verify(m => m.Map<ProductEditModel>(It.IsAny<Common.Data.Entities.Product>()), Times.Never);
        }

        [Test]
        public async Task Handle_MapperReturnsNull_FallsBackToEmptyModelWithId()
        {
            // Arrange
            const int id = 7;
            var entity = new Common.Data.Entities.Product
            {
                Id = id,
                Name = "ProdX",
                ProductCategoryId = 3
            };

            _repoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<ProductEditModel?>(entity))
                .Returns((ProductEditModel?)null);

            var query = new GetEditQuery(id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(id));
            }

            _repoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
            _mapperMock.Verify(m => m.Map<ProductEditModel>(entity), Times.Once);
        }
    }
}