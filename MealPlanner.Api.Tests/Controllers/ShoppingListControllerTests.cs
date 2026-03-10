using Common.Models;
using Common.Pagination;
using MealPlanner.Api.Controllers;
using MealPlanner.Api.Features.ShoppingList.Commands.Add;
using MealPlanner.Api.Features.ShoppingList.Commands.Delete;
using MealPlanner.Api.Features.ShoppingList.Commands.MakeShoppingList;
using MealPlanner.Api.Features.ShoppingList.Commands.Update;
using MealPlanner.Api.Features.ShoppingList.Queries.GetEdit;
using MealPlanner.Api.Features.ShoppingList.Queries.Search;
using MealPlanner.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MealPlanner.Api.Tests.Controllers
{
    [TestFixture]
    public class ShoppingListControllerTests
    {
        private Mock<ISender> _senderMock = null!;
        private ShoppingListController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _senderMock = new Mock<ISender>(MockBehavior.Strict);
            _controller = new ShoppingListController(_senderMock.Object);
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
            var editModel = new ShoppingListEditModel { Id = 5, Name = "List1" };

            _senderMock
                .Setup(m => m.Send(It.Is<GetEditQuery>(q => q.Id == 5), It.IsAny<CancellationToken>()))
                .ReturnsAsync(editModel);

            // Act
            var result = await _controller.GetEditAsync(5, CancellationToken.None);

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
            var paged = new PagedList<ShoppingListModel>([], new Metadata());
            SearchQuery? captured = null;

            _senderMock
                .Setup(m => m.Send(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<PagedList<ShoppingListModel>>, CancellationToken>((q, _) =>
                {
                    captured = (SearchQuery)q;
                })
                .ReturnsAsync(paged);

            // Act
            var result = await _controller.SearchAsync(null, null, "10", "2", CancellationToken.None);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(ok!.Value, Is.SameAs(paged));

                Assert.That(captured, Is.Not.Null);
            });
            Assert.Multiple(() =>
            {
                Assert.That(captured!.QueryParameters, Is.Not.Null);
                Assert.That(captured!.QueryParameters!.PageSize, Is.EqualTo(10));
                Assert.That(captured!.QueryParameters!.PageNumber, Is.EqualTo(2));
            });

            _senderMock.Verify(m => m.Send(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task MakeShoppingListAsync_SendsMakeShoppingListCommand()
        {
            // Arrange
            var createModel = new ShoppingListCreateModel
            {
                MealPlanId = 3,
                ShopId = 7
            };

            var editModel = new ShoppingListEditModel
            {
                Id = 10,
                Name = "AutoList"
            };

            MakeShoppingListCommand? captured = null;

            _senderMock
                .Setup(m => m.Send(It.IsAny<MakeShoppingListCommand>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<ShoppingListEditModel?>, CancellationToken>((c, _) =>
                {
                    captured = (MakeShoppingListCommand)c;
                })
                .ReturnsAsync(editModel);

            // Act
            var result = await _controller.MakeShoppingListAsync(createModel, CancellationToken.None);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(ok!.Value, Is.SameAs(editModel));

                Assert.That(captured, Is.Not.Null);
            });
            Assert.Multiple(() =>
            {
                Assert.That(captured!.MealPlanId, Is.EqualTo(3));
                Assert.That(captured!.ShopId, Is.EqualTo(7));
            });

            _senderMock.Verify(m => m.Send(It.IsAny<MakeShoppingListCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task PostAsync_SendsAddCommand()
        {
            // Arrange
            var model = new ShoppingListEditModel { Id = 0, Name = "NewList" };
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
            var model = new ShoppingListEditModel { Id = 2, Name = "UpdatedList" };
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
            var response = CommandResponse.Success();

            _senderMock
                .Setup(m => m.Send(It.Is<DeleteCommand>(c => c.Id == 9), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.DeleteAsync(9, CancellationToken.None);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.SameAs(response));

            _senderMock.Verify(m => m.Send(It.IsAny<DeleteCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}