using Common.Models;
using Common.Pagination;
using MealPlanner.Api.Controllers;
using MealPlanner.Api.Features.Shop.Commands.Add;
using MealPlanner.Api.Features.Shop.Commands.Delete;
using MealPlanner.Api.Features.Shop.Commands.Update;
using MealPlanner.Api.Features.Shop.Queries.GetEdit;
using MealPlanner.Api.Features.Shop.Queries.Search;
using MealPlanner.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MealPlanner.Api.Tests.Controllers
{
    [TestFixture]
    public class ShopControllerTests
    {
        private Mock<ISender> _senderMock = null!;
        private ShopController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _senderMock = new Mock<ISender>(MockBehavior.Strict);
            _controller = new ShopController(_senderMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _senderMock.Reset();
        }

        [Test]
        public async Task GetEditAsync_SendsGetEditQuery()
        {
            // Arrange
            var id = Guid.NewGuid();
            var editModel = new ShopEditModel { Id = id, Name = "Shop1" };

            _senderMock
                .Setup(m => m.Send(It.Is<GetEditQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(editModel);

            // Act
            var result = await _controller.GetEditAsync(id, CancellationToken.None);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.SameAs(editModel));

            _senderMock.Verify(m => m.Send(It.IsAny<GetEditQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SearchAsync_InvalidPageParams_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.SearchAsync(null, null, "abc", "1", CancellationToken.None);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            _senderMock.Verify(m => m.Send(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SearchAsync_ValidParams_SendsSearchQuery()
        {
            // Arrange
            var paged = new PagedList<ShopModel>([], new Metadata());
            SearchQuery? captured = null;

            _senderMock
                .Setup(m => m.Send(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<PagedList<ShopModel>>, CancellationToken>((q, _) =>
                {
                    captured = (SearchQuery)q;
                })
                .ReturnsAsync(paged);

            // Act
            var result = await _controller.SearchAsync(null, null, "10", "2", CancellationToken.None);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(ok!.Value, Is.SameAs(paged));

                Assert.That(captured, Is.Not.Null);
            }
            using (Assert.EnterMultipleScope())
            {
                Assert.That(captured!.QueryParameters, Is.Not.Null);
                Assert.That(captured!.QueryParameters!.PageSize, Is.EqualTo(10));
                Assert.That(captured!.QueryParameters!.PageNumber, Is.EqualTo(2));
            }

            _senderMock.Verify(m => m.Send(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task PostAsync_SendsAddCommand()
        {
            // Arrange
            var model = new ShopEditModel { Id = Guid.Empty, Name = "NewShop" };
            var response = CommandResponse.Success();

            _senderMock
                .Setup(m => m.Send(It.Is<AddCommand>(c => c.Model == model), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.PostAsync(model, CancellationToken.None);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.SameAs(response));

            _senderMock.Verify(m => m.Send(It.IsAny<AddCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task PutAsync_SendsUpdateCommand()
        {
            // Arrange
            var model = new ShopEditModel { Id = Guid.NewGuid(), Name = "UpdatedShop" };
            var response = CommandResponse.Success();

            _senderMock
                .Setup(m => m.Send(It.Is<UpdateCommand>(c => c.Model == model), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.PutAsync(model, CancellationToken.None);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.SameAs(response));

            _senderMock.Verify(m => m.Send(It.IsAny<UpdateCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_SendsDeleteCommand()
        {
            // Arrange
            var id = Guid.NewGuid();
            var response = CommandResponse.Success();

            _senderMock
                .Setup(m => m.Send(It.Is<DeleteCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.DeleteAsync(id, CancellationToken.None);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.SameAs(response));

            _senderMock.Verify(m => m.Send(It.IsAny<DeleteCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}