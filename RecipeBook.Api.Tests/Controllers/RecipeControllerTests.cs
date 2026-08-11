using Common.Models;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RecipeBook.Api.Controllers;
using RecipeBook.Api.Features.Recipe.Commands.Add;
using RecipeBook.Api.Features.Recipe.Commands.Delete;
using RecipeBook.Api.Features.Recipe.Commands.Update;
using RecipeBook.Api.Features.Recipe.Queries.GetById;
using RecipeBook.Api.Features.Recipe.Queries.GetEdit;
using RecipeBook.Api.Features.Recipe.Queries.GetShoppingListProducts;
using RecipeBook.Api.Features.Recipe.Queries.Search;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Controllers
{
    [TestFixture]
    public class RecipeControllerTests
    {
        private Mock<ISender> _senderMock = null!;
        private RecipeController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _senderMock = new Mock<ISender>(MockBehavior.Strict);
            _controller = new RecipeController(_senderMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Test]
        public async Task GetByIdAsync_SendsGetByIdQuery()
        {
            var recipeId = Guid.NewGuid();
            var model = new RecipeModel { Id = recipeId, Name = "R1" };

            _senderMock
                .Setup(m => m.Send(It.Is<GetByIdQuery>(q => q.Id == recipeId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(model);

            var result = await _controller.GetByIdAsync(recipeId, CancellationToken.None);

            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.SameAs(model));

            _senderMock.Verify(m => m.Send(It.IsAny<GetByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetEditAsync_SendsGetEditQuery()
        {
            var editId = Guid.NewGuid();
            var model = new RecipeEditModel { Id = editId, Name = "EditRecipe" };

            _senderMock
                .Setup(m => m.Send(It.Is<GetEditQuery>(q => q.Id == editId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(model);

            var result = await _controller.GetEditAsync(editId, CancellationToken.None);

            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.SameAs(model));

            _senderMock.Verify(m => m.Send(It.IsAny<GetEditQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetShoppingListProductsAsync_SendsQuery_WithToken()
        {
            var items = new[] { new ShoppingListProductEditModel { Product = new ProductModel() { Id = Guid.NewGuid() } } };
            _controller.HttpContext.Request.Headers.Authorization = "Bearer token123";

            var shopId = Guid.NewGuid();
            var slpRecipeId = Guid.NewGuid();
            _senderMock
                .Setup(m => m.Send(
                    It.Is<GetShoppingListProductsQuery>(q =>
                        q.RecipeId == slpRecipeId &&
                        q.ShopId == shopId &&
                        q.AuthToken == "token123"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(items);

            var result = await _controller.GetShoppingListProductsAsync(slpRecipeId, shopId, CancellationToken.None);

            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.SameAs(items));

            _senderMock.Verify(m => m.Send(It.IsAny<GetShoppingListProductsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SearchAsync_InvalidPageParams_ReturnsBadRequest()
        {
            var result = await _controller.SearchAsync(null, null, "abc", "1", CancellationToken.None);
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            _senderMock.Verify(m => m.Send(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SearchAsync_ValidParams_SendsSearchQuery()
        {
            var paged = new PagedList<RecipeModel>([], new Metadata());
            SearchQuery? captured = null;

            _senderMock
                .Setup(m => m.Send(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<PagedList<RecipeModel>>, CancellationToken>((q, _) => captured = (SearchQuery)q)
                .ReturnsAsync(paged);

            var result = await _controller.SearchAsync(null, null, "10", "2", CancellationToken.None);

            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(ok!.Value, Is.SameAs(paged));

                Assert.That(captured, Is.Not.Null);
            }
            using (Assert.EnterMultipleScope())
            {
                Assert.That(captured!.QueryParameters!.PageSize, Is.EqualTo(10));
                Assert.That(captured!.QueryParameters!.PageNumber, Is.EqualTo(2));
            }

            _senderMock.Verify(m => m.Send(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task PostAsync_SendsAddCommand()
        {
            var model = new RecipeEditModel { Id = Guid.Empty, Name = "NewR" };
            var response = CommandResponse.Success();

            _senderMock
                .Setup(m => m.Send(It.Is<AddCommand>(c => c.Model == model), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var result = await _controller.PostAsync(model, CancellationToken.None);

            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.SameAs(response));

            _senderMock.Verify(m => m.Send(It.IsAny<AddCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task PutAsync_SendsUpdateCommand()
        {
            var model = new RecipeEditModel { Id = Guid.NewGuid(), Name = "Updated" };
            var response = CommandResponse.Success();

            _senderMock
                .Setup(m => m.Send(It.Is<UpdateCommand>(c => c.Model == model), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var result = await _controller.PutAsync(model, CancellationToken.None);

            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.SameAs(response));

            _senderMock.Verify(m => m.Send(It.IsAny<UpdateCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_SendsDeleteCommand_WithToken()
        {
            var response = CommandResponse.Success();
            _controller.HttpContext.Request.Headers.Authorization = "Bearer tok";

            var deleteId = Guid.NewGuid();
            _senderMock
                .Setup(m => m.Send(
                    It.Is<DeleteCommand>(c => c.Id == deleteId && c.AuthToken == "tok"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var result = await _controller.DeleteAsync(deleteId, CancellationToken.None);

            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.SameAs(response));

            _senderMock.Verify(m => m.Send(It.IsAny<DeleteCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}