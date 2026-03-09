using Common.Models;
using Common.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RecipeBook.Api.Controllers;
using RecipeBook.Api.Features.Unit.Commands.Add;
using RecipeBook.Api.Features.Unit.Commands.Delete;
using RecipeBook.Api.Features.Unit.Commands.Update;
using RecipeBook.Api.Features.Unit.Queries.GetEdit;
using RecipeBook.Api.Features.Unit.Queries.Search;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Controllers
{
    [TestFixture]
    public class UnitControllerTests
    {
        private Mock<ISender> _senderMock = null!;
        private UnitController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _senderMock = new Mock<ISender>(MockBehavior.Strict);
            _controller = new UnitController(_senderMock.Object);
        }

        [Test]
        public async Task GetEditAsync_SendsGetEditQuery_WithCorrectId()
        {
            // Arrange
            var model = new UnitEditModel { Id = 5, Name = "kg" };

            _senderMock
                .Setup(m => m.Send(It.Is<GetEditQuery>(q => q.Id == 5), It.IsAny<CancellationToken>()))
                .ReturnsAsync(model);

            // Act
            var result = await _controller.GetEditAsync(5);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.SameAs(model));

            _senderMock.Verify(m => m.Send(It.IsAny<GetEditQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SearchAsync_InvalidPageParameters_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.SearchAsync(null, null, "abc", "1");

            // Assert
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            _senderMock.Verify(m => m.Send(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SearchAsync_ValidParameters_SendsSearchQuery()
        {
            // Arrange
            var paged = new PagedList<UnitModel>([], new Metadata());
            SearchQuery? capturedQuery = null;

            _senderMock
                .Setup(m => m.Send(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<PagedList<UnitModel>>, CancellationToken>((q, _) => capturedQuery = (SearchQuery)q)
                .ReturnsAsync(paged);

            // Act
            var result = await _controller.SearchAsync(
                filters: null,
                sorting: null,
                pageSize: "10",
                pageNumber: "2");

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(ok!.Value, Is.SameAs(paged));

                Assert.That(capturedQuery, Is.Not.Null);
            }
            using (Assert.EnterMultipleScope())
            {
                Assert.That(capturedQuery!.QueryParameters, Is.Not.Null);
                Assert.That(capturedQuery!.QueryParameters!.PageSize, Is.EqualTo(10));
                Assert.That(capturedQuery!.QueryParameters!.PageNumber, Is.EqualTo(2));
            }

            _senderMock.Verify(m => m.Send(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task PostAsync_SendsAddCommand_WithModel()
        {
            // Arrange
            var model = new UnitEditModel { Id = 0, Name = "kg" };
            var response = CommandResponse.Success();

            _senderMock
                .Setup(m => m.Send(It.Is<AddCommand>(c => c.Model == model), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.PostAsync(model);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.SameAs(response));

            _senderMock.Verify(m => m.Send(It.IsAny<AddCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task PutAsync_SendsUpdateCommand_WithModel()
        {
            // Arrange
            var model = new UnitEditModel { Id = 1, Name = "g" };
            var response = CommandResponse.Success();

            _senderMock
                .Setup(m => m.Send(It.Is<UpdateCommand>(c => c.Model == model), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.PutAsync(model);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.SameAs(response));

            _senderMock.Verify(m => m.Send(It.IsAny<UpdateCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_SendsDeleteCommand_WithId()
        {
            // Arrange
            var response = CommandResponse.Success();

            _senderMock
                .Setup(m => m.Send(It.Is<DeleteCommand>(c => c.Id == 7), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.DeleteAsync(7);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.SameAs(response));

            _senderMock.Verify(m => m.Send(It.IsAny<DeleteCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}