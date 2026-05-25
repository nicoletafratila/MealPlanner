using Common.Models;
using Common.Pagination;
using Identity.Api.Controllers;
using Identity.Api.Features.ApplicationUser.Commands.Unlock;
using Identity.Api.Features.ApplicationUser.Commands.Update;
using Identity.Api.Features.ApplicationUser.Queries.GetEdit;
using Identity.Api.Features.ApplicationUser.Queries.Search;
using Identity.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Identity.Api.Tests.Controllers
{
    [TestFixture]
    public class ApplicationUserControllerTests
    {
        private Mock<ISender> _mediatorMock = null!;
        private ApplicationUserController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<ISender>(MockBehavior.Strict);
            _controller = new ApplicationUserController(_mediatorMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        #region SearchAsync

        [Test]
        public async Task SearchAsync_InvalidPageSize_ReturnsBadRequest()
        {
            var result = await _controller.SearchAsync(null, null, "0", "1", CancellationToken.None);

            var badRequest = result.Result as BadRequestObjectResult;
            Assert.That(badRequest, Is.Not.Null);

            _mediatorMock.Verify(
                m => m.Send(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task SearchAsync_InvalidPageNumber_ReturnsBadRequest()
        {
            var result = await _controller.SearchAsync(null, null, "10", "abc", CancellationToken.None);

            var badRequest = result.Result as BadRequestObjectResult;
            Assert.That(badRequest, Is.Not.Null);

            _mediatorMock.Verify(
                m => m.Send(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task SearchAsync_ValidParameters_ReturnsOkWithPagedList()
        {
            var pagedList = new PagedList<ApplicationUserModel>(
                [new() { UserId = "1", Username = "alice" }],
                new Metadata { TotalCount = 1, PageSize = 10, PageNumber = 1 });

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedList);

            var result = await _controller.SearchAsync(null, null, "10", "1", CancellationToken.None);

            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            var returned = ok!.Value as PagedList<ApplicationUserModel>;
            Assert.That(returned, Is.Not.Null);
            Assert.That(returned!.Items, Has.Count.EqualTo(1));

            _mediatorMock.Verify(
                m => m.Send(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task SearchAsync_ValidParameters_BuildsQueryParametersCorrectly()
        {
            SearchQuery? capturedQuery = null;

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<PagedList<ApplicationUserModel>>, CancellationToken>(
                    (q, _) => capturedQuery = (SearchQuery)q)
                .ReturnsAsync(new PagedList<ApplicationUserModel>([], new Metadata()));

            await _controller.SearchAsync(null, null, "5", "2", CancellationToken.None);

            Assert.That(capturedQuery, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(capturedQuery!.QueryParameters!.PageSize, Is.EqualTo(5));
                Assert.That(capturedQuery.QueryParameters.PageNumber, Is.EqualTo(2));
                Assert.That(capturedQuery.QueryParameters.Filters, Is.Null);
                Assert.That(capturedQuery.QueryParameters.Sorting, Is.Null);
            }
        }

        #endregion

        #region GetEditAsync

        [Test]
        public async Task GetEditAsync_UsernameMissing_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GetEditAsync("", CancellationToken.None);

            // Assert
            var badRequest = result.Result as BadRequestObjectResult;
            Assert.That(badRequest, Is.Not.Null);
            Assert.That(badRequest!.Value, Is.EqualTo("Username is required."));

            _mediatorMock.Verify(
                m => m.Send(It.IsAny<GetEditQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetEditAsync_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var username = "alice";

            _mediatorMock
                .Setup(m => m.Send(
                    It.Is<GetEditQuery>(q => q.Name == username),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApplicationUserEditModel
                {
                    Username = null
                });

            // Act
            var result = await _controller.GetEditAsync(username, CancellationToken.None);

            // Assert
            var notFound = result.Result as NotFoundObjectResult;
            Assert.That(notFound, Is.Not.Null);
            Assert.That(notFound!.Value, Is.EqualTo("User 'alice' was not found."));

            _mediatorMock.Verify(
                m => m.Send(
                    It.Is<GetEditQuery>(q => q.Name == username),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetEditAsync_UserFound_ReturnsOkWithModel()
        {
            // Arrange
            var username = "bob";
            var model = new ApplicationUserEditModel
            {
                UserId = "1",
                Username = "bob",
                EmailAddress = "bob@example.com"
            };

            _mediatorMock
                .Setup(m => m.Send(
                    It.Is<GetEditQuery>(q => q.Name == username),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(model);

            // Act
            var result = await _controller.GetEditAsync(username, CancellationToken.None);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var returnedModel = okResult!.Value as ApplicationUserEditModel;
            Assert.That(returnedModel, Is.Not.Null);
            Assert.That(returnedModel!.Username, Is.EqualTo("bob"));

            _mediatorMock.Verify(
                m => m.Send(
                    It.Is<GetEditQuery>(q => q.Name == username),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region UnlockAsync

        [Test]
        public async Task UnlockAsync_NullCommand_ReturnsBadRequest()
        {
            var result = await _controller.UnlockAsync(null!, CancellationToken.None);

            var badRequest = result.Result as BadRequestObjectResult;
            Assert.That(badRequest, Is.Not.Null);

            _mediatorMock.Verify(
                m => m.Send(It.IsAny<UnlockCommand>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task UnlockAsync_EmptyUserId_ReturnsBadRequest()
        {
            var result = await _controller.UnlockAsync(new UnlockCommand { UserId = "" }, CancellationToken.None);

            var badRequest = result.Result as BadRequestObjectResult;
            Assert.That(badRequest, Is.Not.Null);

            _mediatorMock.Verify(
                m => m.Send(It.IsAny<UnlockCommand>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task UnlockAsync_ResponseNull_Returns500()
        {
            var command = new UnlockCommand { UserId = "user-1" };

            _mediatorMock
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync((CommandResponse?)null);

            var result = await _controller.UnlockAsync(command, CancellationToken.None);

            var objResult = result.Result as ObjectResult;
            Assert.That(objResult, Is.Not.Null);
            Assert.That(objResult!.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }

        [Test]
        public async Task UnlockAsync_ResponseFailed_ReturnsBadRequest()
        {
            var command = new UnlockCommand { UserId = "user-1" };
            var failed = CommandResponse.Failed("Unlock failed");

            _mediatorMock
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(failed);

            var result = await _controller.UnlockAsync(command, CancellationToken.None);

            var badRequest = result.Result as BadRequestObjectResult;
            Assert.That(badRequest, Is.Not.Null);
            Assert.That(badRequest!.Value, Is.SameAs(failed));
        }

        [Test]
        public async Task UnlockAsync_ResponseSucceeded_ReturnsOk()
        {
            var command = new UnlockCommand { UserId = "user-1" };
            var success = CommandResponse.Success();

            _mediatorMock
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(success);

            var result = await _controller.UnlockAsync(command, CancellationToken.None);

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.SameAs(success));

            _mediatorMock.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region PutAsync

        [Test]
        public async Task PutAsync_ModelNull_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.PutAsync(null!, CancellationToken.None);

            // Assert
            var badRequest = result.Result as BadRequestObjectResult;
            Assert.That(badRequest, Is.Not.Null);
            Assert.That(badRequest!.Value, Is.EqualTo("Model is required."));

            _mediatorMock.Verify(
                m => m.Send(It.IsAny<UpdateCommand>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task PutAsync_ResponseNull_Returns500()
        {
            // Arrange
            var model = new ApplicationUserEditModel
            {
                UserId = "1",
                Username = "bob",
                EmailAddress = "bob@example.com"
            };

            _mediatorMock
                .Setup(m => m.Send(
                    It.Is<UpdateCommand>(c => c.Model == model),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((CommandResponse?)null);

            // Act
            var result = await _controller.PutAsync(model, CancellationToken.None);

            // Assert
            var objResult = result.Result as ObjectResult;
            Assert.That(objResult, Is.Not.Null);
            Assert.That(objResult!.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));

            _mediatorMock.Verify(
                m => m.Send(
                    It.Is<UpdateCommand>(c => c.Model == model),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task PutAsync_ResponseFailed_ReturnsBadRequestWithResponse()
        {
            // Arrange
            var model = new ApplicationUserEditModel
            {
                UserId = "1",
                Username = "bob",
                EmailAddress = "bob@example.com"
            };

            var failed = CommandResponse.Failed("Error message");

            _mediatorMock
                .Setup(m => m.Send(
                    It.Is<UpdateCommand>(c => c.Model == model),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(failed);

            // Act
            var result = await _controller.PutAsync(model, CancellationToken.None);

            // Assert
            var badRequest = result.Result as BadRequestObjectResult;
            Assert.That(badRequest, Is.Not.Null);
            Assert.That(badRequest!.Value, Is.SameAs(failed));

            _mediatorMock.Verify(
                m => m.Send(
                    It.Is<UpdateCommand>(c => c.Model == model),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task PutAsync_ResponseSucceeded_ReturnsOkWithResponse()
        {
            // Arrange
            var model = new ApplicationUserEditModel
            {
                UserId = "1",
                Username = "bob",
                EmailAddress = "bob@example.com"
            };

            var success = CommandResponse.Success();

            _mediatorMock
                .Setup(m => m.Send(
                    It.Is<UpdateCommand>(c => c.Model == model),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(success);

            // Act
            var result = await _controller.PutAsync(model, CancellationToken.None);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.SameAs(success));

            _mediatorMock.Verify(
                m => m.Send(
                    It.Is<UpdateCommand>(c => c.Model == model),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion
    }
}