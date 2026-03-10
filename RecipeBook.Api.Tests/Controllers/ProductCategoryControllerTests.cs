using Common.Models;
using Common.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RecipeBook.Api.Controllers;
using RecipeBook.Api.Features.ProductCategory.Commands.Add;
using RecipeBook.Api.Features.ProductCategory.Commands.Delete;
using RecipeBook.Api.Features.ProductCategory.Commands.Update;
using RecipeBook.Api.Features.ProductCategory.Queries.GetEdit;
using RecipeBook.Api.Features.ProductCategory.Queries.Search;
using RecipeBook.Api.Features.ProductCategory.Queries.SearchByCategories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Controllers
{
    [TestFixture]
    public class ProductCategoryControllerTests
    {
        private Mock<ISender> _senderMock = null!;
        private ProductCategoryController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _senderMock = new Mock<ISender>(MockBehavior.Strict);
            _controller = new ProductCategoryController(_senderMock.Object);
        }

        [Test]
        public async Task GetEditAsync_SendsGetEditQuery()
        {
            var model = new ProductCategoryEditModel { Id = 5, Name = "Dairy" };

            _senderMock
                .Setup(m => m.Send(It.Is<GetEditQuery>(q => q.Id == 5), It.IsAny<CancellationToken>()))
                .ReturnsAsync(model);

            var result = await _controller.GetEditAsync(5, CancellationToken.None);

            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.SameAs(model));

            _senderMock.Verify(m => m.Send(It.IsAny<GetEditQuery>(), It.IsAny<CancellationToken>()), Times.Once);
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
            var paged = new PagedList<ProductCategoryModel>([], new Metadata());
            SearchQuery? captured = null;

            _senderMock
                .Setup(m => m.Send(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<PagedList<ProductCategoryModel>>, CancellationToken>((q, _) => captured = (SearchQuery)q)
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
        public async Task SearchByCategoriesAsync_SendsSearchByCategoriesQuery()
        {
            var models = new List<ProductCategoryModel>
            {
                new() { Id = 1, Name = "Dairy" }
            };

            _senderMock
                .Setup(m => m.Send(It.Is<SearchByCategoriesQuery>(q => q.CategoryIds == "1,2"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(models);

            var result = await _controller.SearchByCategoriesAsync("1,2", CancellationToken.None);

            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.SameAs(models));

            _senderMock.Verify(m => m.Send(It.IsAny<SearchByCategoriesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task PostAsync_SendsAddCommand()
        {
            var model = new ProductCategoryEditModel { Id = 0, Name = "NewCat" };
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
            var model = new ProductCategoryEditModel { Id = 2, Name = "UpdatedCat" };
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
        public async Task DeleteAsync_SendsDeleteCommand()
        {
            var response = CommandResponse.Success();

            _senderMock
                .Setup(m => m.Send(It.Is<DeleteCommand>(c => c.Id == 9), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var result = await _controller.DeleteAsync(9, CancellationToken.None);

            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.SameAs(response));

            _senderMock.Verify(m => m.Send(It.IsAny<DeleteCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}