using AutoMapper;
using MealPlanner.Api.Features.Shop.Queries.GetEdit;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using Moq;

namespace MealPlanner.Api.Tests.Features.Shop.Queries.GetEdit
{
    [TestFixture]
    public class GetEditQueryHandlerTests
    {
        private Mock<IShopRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private GetEditQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IShopRepository>(MockBehavior.Strict);
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
            var entity = new Common.Data.Entities.Shop
            {
                Id = id,
                Name = "Shop1"
            };

            var mapped = new ShopEditModel
            {
                Id = id,
                Name = "Shop1"
            };

            _repoMock
                .Setup(r => r.GetByIdIncludeDisplaySequenceAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<ShopEditModel>(entity))
                .Returns(mapped);

            var query = new GetEditQuery(id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.EqualTo("Shop1"));
            });

            _repoMock.Verify(r => r.GetByIdIncludeDisplaySequenceAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<ShopEditModel>(entity), Times.Once);
        }

        [Test]
        public async Task Handle_EntityNotFound_ReturnsEmptyModelWithId()
        {
            // Arrange
            const int id = 10;

            _repoMock
                .Setup(r => r.GetByIdIncludeDisplaySequenceAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Common.Data.Entities.Shop?)null);

            var query = new GetEditQuery(id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.Null.Or.Empty);
            });

            _repoMock.Verify(r => r.GetByIdIncludeDisplaySequenceAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<ShopEditModel>(It.IsAny<Common.Data.Entities.Shop>()), Times.Never);
        }

        [Test]
        public async Task Handle_MapperReturnsNull_FallsBackToEmptyModelWithId()
        {
            // Arrange
            const int id = 7;
            var entity = new Common.Data.Entities.Shop
            {
                Id = id,
                Name = "ShopX"
            };

            _repoMock
                .Setup(r => r.GetByIdIncludeDisplaySequenceAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<ShopEditModel?>(entity))
                .Returns((ShopEditModel?)null);

            var query = new GetEditQuery(id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(id));
            });

            _repoMock.Verify(r => r.GetByIdIncludeDisplaySequenceAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<ShopEditModel>(entity), Times.Once);
        }
    }
}