using AutoMapper;
using MealPlanner.Api.Features.ShoppingList.Queries.GetEdit;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using Moq;

namespace MealPlanner.Api.Tests.Features.ShoppingList.Queries.GetEdit
{
    [TestFixture]
    public class GetEditQueryHandlerTests
    {
        private Mock<IShoppingListRepository> _repoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private GetEditQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IShoppingListRepository>(MockBehavior.Strict);
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
            var entity = new Common.Data.Entities.ShoppingList
            {
                Id = id,
                Name = "List1"
            };

            var mapped = new ShoppingListEditModel
            {
                Id = id,
                Name = "List1"
            };

            _repoMock
                .Setup(r => r.GetByIdIncludeProductsAsync(id))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<ShoppingListEditModel>(entity))
                .Returns(mapped);

            var query = new GetEditQuery(id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.EqualTo("List1"));
            });

            _repoMock.Verify(r => r.GetByIdIncludeProductsAsync(id), Times.Once);
            _mapperMock.Verify(m => m.Map<ShoppingListEditModel>(entity), Times.Once);
        }

        [Test]
        public async Task Handle_EntityNotFound_ReturnsEmptyModelWithId()
        {
            // Arrange
            const int id = 10;

            _repoMock
                .Setup(r => r.GetByIdIncludeProductsAsync(id))
                .ReturnsAsync((Common.Data.Entities.ShoppingList?)null);

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

            _repoMock.Verify(r => r.GetByIdIncludeProductsAsync(id), Times.Once);
            _mapperMock.Verify(m => m.Map<ShoppingListEditModel>(It.IsAny<Common.Data.Entities.ShoppingList>()), Times.Never);
        }

        [Test]
        public async Task Handle_MapperReturnsNull_FallsBackToEmptyModelWithId()
        {
            // Arrange
            const int id = 7;
            var entity = new Common.Data.Entities.ShoppingList
            {
                Id = id,
                Name = "SomeList"
            };

            _repoMock
                .Setup(r => r.GetByIdIncludeProductsAsync(id))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<ShoppingListEditModel?>(entity))
                .Returns((ShoppingListEditModel?)null);

            var query = new GetEditQuery(id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(id));
            });

            _repoMock.Verify(r => r.GetByIdIncludeProductsAsync(id), Times.Once);
            _mapperMock.Verify(m => m.Map<ShoppingListEditModel>(entity), Times.Once);
        }
    }
}