using Common.Models;
using Common.Pagination;
using MealPlanner.Api.Controllers;
using MealPlanner.Api.Features.MealPlan.Commands.Add;
using MealPlanner.Api.Features.MealPlan.Commands.Delete;
using MealPlanner.Api.Features.MealPlan.Commands.Update;
using MealPlanner.Api.Features.MealPlan.Queries.GetEdit;
using MealPlanner.Api.Features.MealPlan.Queries.GetShoppingListProducts;
using MealPlanner.Api.Features.MealPlan.Queries.Search;
using MealPlanner.Api.Features.MealPlan.Queries.SearchByRecipeId;
using MealPlanner.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MealPlanner.Api.Tests.Controllers
{
    [TestFixture]
    public class MealPlanControllerTests
    {
        private Mock<ISender> _senderMock = null!;
        private MealPlanController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _senderMock = new Mock<ISender>(MockBehavior.Strict);
            _controller = new MealPlanController(_senderMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _senderMock.Reset();
        }

        [Test]
        public async Task GetEditAsync_SendsGetEditMealPlanQuery()
        {
            // Arrange
            var editModel = new MealPlanEditModel { Id = 5, Name = "Plan1" };

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
        public async Task GetShoppingListProductsAsync_SendsGetShoppingListProductsQuery()
        {
            // Arrange
            var products = new List<ShoppingListProductEditModel>
            {
                new() { ShoppingListId = 1, Product = new RecipeBook.Shared.Models.ProductModel(){ Id = 10 } },
                new() { ShoppingListId = 1, Product = new RecipeBook.Shared.Models.ProductModel(){ Id = 11 } }
            };

            GetShoppingListProductsQuery? captured = null;

            _senderMock
                .Setup(m => m.Send(It.IsAny<GetShoppingListProductsQuery>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<IList<ShoppingListProductEditModel>?>, CancellationToken>((q, _) =>
                {
                    captured = (GetShoppingListProductsQuery)q;
                })
                .ReturnsAsync(products);

            // Act
            var result = await _controller.GetShoppingListProductsAsync(3, 7, CancellationToken.None);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(ok!.Value, Is.SameAs(products));

                Assert.That(captured, Is.Not.Null);
            });
            Assert.Multiple(() =>
            {
                Assert.That(captured!.MealPlanId, Is.EqualTo(3));
                Assert.That(captured!.ShopId, Is.EqualTo(7));
            });

            _senderMock.Verify(m => m.Send(It.IsAny<GetShoppingListProductsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
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
            var paged = new PagedList<MealPlanModel>([], new Metadata());
            SearchQuery? captured = null;

            _senderMock
                .Setup(m => m.Send(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<PagedList<MealPlanModel>>, CancellationToken>((q, _) =>
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
        public async Task SearchByRecipeIdAsync_SendsSearchByRecipeIdQuery()
        {
            // Arrange
            var models = new List<MealPlanModel>
            {
                new() { Id = 1, Name = "Plan1" },
                new() { Id = 2, Name = "Plan2" }
            };

            SearchByRecipeIdQuery? captured = null;

            _senderMock
                .Setup(m => m.Send(It.IsAny<SearchByRecipeIdQuery>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<IList<MealPlanModel>>, CancellationToken>((q, _) =>
                {
                    captured = (SearchByRecipeIdQuery)q;
                })
                .ReturnsAsync(models);

            // Act
            var result = await _controller.SearchByRecipeIdAsync(10, CancellationToken.None);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(ok!.Value, Is.SameAs(models));

                Assert.That(captured, Is.Not.Null);
            });
            Assert.That(captured!.RecipeId, Is.EqualTo(10));

            _senderMock.Verify(m => m.Send(It.IsAny<SearchByRecipeIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task PostAsync_SendsAddCommand()
        {
            // Arrange
            var model = new MealPlanEditModel { Id = 0, Name = "NewPlan" };
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
            var model = new MealPlanEditModel { Id = 2, Name = "UpdatedPlan" };
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