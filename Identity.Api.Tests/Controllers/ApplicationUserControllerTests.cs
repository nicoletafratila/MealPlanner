using Common.Models;
using Identity.Api.Controllers;
using Identity.Api.Features.ApplicationUser.Commands.Update;
using Identity.Api.Features.ApplicationUser.Queries.GetEdit;
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