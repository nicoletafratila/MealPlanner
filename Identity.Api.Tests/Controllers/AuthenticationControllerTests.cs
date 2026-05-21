using System.Security.Claims;
using Common.Models;
using Identity.Api.Controllers;
using Identity.Api.Features.Authentication.Commands.ConfirmEmail;
using Identity.Api.Features.Authentication.Commands.Login;
using Identity.Api.Features.Authentication.Commands.Logout;
using Identity.Api.Features.Authentication.Commands.Register;
using Identity.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Identity.Api.Tests.Controllers
{
    [TestFixture]
    public class AuthenticationControllerTests
    {
        private Mock<ISender> _mediatorMock = null!;
        private Mock<IConfiguration> _configurationMock = null!;
        private AuthenticationController _controller = null!;
        private DefaultHttpContext _httpContext = null!;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<ISender>(MockBehavior.Strict);
            _configurationMock = new Mock<IConfiguration>(MockBehavior.Loose);
            _configurationMock.Setup(c => c["MealPlannerWeb:BaseUrl"]).Returns("https://localhost:7093");

            _controller = new AuthenticationController(_mediatorMock.Object, _configurationMock.Object);

            _httpContext = new DefaultHttpContext();

            var authServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);
            authServiceMock
                .Setup(a => a.SignInAsync(
                    It.IsAny<HttpContext>(),
                    It.IsAny<string>(),
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            _httpContext.RequestServices = new ServiceProviderStub(authServiceMock.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };
        }

        [Test]
        public async Task LoginAsync_SuccessfulResponse_SignsInAndReturnsResponse()
        {
            // Arrange
            var model = new LoginModel
            {
                Username = "user",
                Password = "pwd"
            };

            var claims = new List<KeyValuePair<string, string>>
            {
                new(ClaimTypes.Name, "user"),
                new(ClaimTypes.Role, "admin")
            };

            var loginResponse = new LoginCommandResponse
            {
                Succeeded = true,
                Message = "Login successful.",
                JwtBearer = "token",
                Claims = claims
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(loginResponse);

            var authService = (IAuthenticationService)_httpContext.RequestServices.GetService(typeof(IAuthenticationService))!;
            var authServiceMock = Mock.Get(authService);

            // Act
            var result = await _controller.LoginAsync(model, CancellationToken.None);

            // Assert
            Assert.That(result, Is.SameAs(loginResponse));

            _mediatorMock.Verify(
                m => m.Send(
                    It.Is<LoginCommand>(c => c.Model == model),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            authServiceMock.Verify(a => a.SignInAsync(
                    _httpContext,
                    IdentityConstants.ApplicationScheme,
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<AuthenticationProperties>()),
                Times.Once);
        }

        [Test]
        public async Task LoginAsync_FailedResponse_ReturnsFailedCommandResponse_AndDoesNotSignIn()
        {
            // Arrange
            var model = new LoginModel
            {
                Username = "user",
                Password = "wrong"
            };

            var failedResponse = CommandResponse.Failed("Invalid credentials.");

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResponse);

            var authService = (IAuthenticationService)_httpContext.RequestServices.GetService(typeof(IAuthenticationService))!;
            var authServiceMock = Mock.Get(authService);

            // Act
            var result = await _controller.LoginAsync(model, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Invalid credentials."));
            });

            _mediatorMock.Verify(
                m => m.Send(
                    It.Is<LoginCommand>(c => c.Model == model),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            authServiceMock.Verify(a => a.SignInAsync(
                    It.IsAny<HttpContext>(),
                    It.IsAny<string>(),
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<AuthenticationProperties>()),
                Times.Never);
        }

        [Test]
        public async Task LogoutAsync_SendsLogoutCommand_AndReturnsMediatorResult()
        {
            // Arrange
            var response = CommandResponse.Success();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<LogoutCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.LogoutAsync(CancellationToken.None);

            // Assert
            Assert.That(result, Is.SameAs(response));

            _mediatorMock.Verify(
                m => m.Send(
                    It.IsAny<LogoutCommand>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task RegisterAsync_ForwardsToMediator_AndReturnsResult()
        {
            // Arrange
            var model = new RegistrationModel
            {
                Username = "newuser",
                Password = "Test123!",
                ConfirmPassword = "Test123!",
                EmailAddress = "newuser@example.com"
            };

            var response = CommandResponse.Success();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.RegisterAsync(model, CancellationToken.None);

            // Assert
            Assert.That(result, Is.SameAs(response));

            _mediatorMock.Verify(
                m => m.Send(
                    It.Is<RegisterCommand>(c => c.Model == model),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task ConfirmEmailAsync_SucceededResult_RedirectsToLoginWithConfirmedParam()
        {
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<ConfirmEmailCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CommandResponse.Success());

            var result = await _controller.ConfirmEmailAsync("user-id", "token", CancellationToken.None);

            var redirect = result as RedirectResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.Url, Is.EqualTo("https://localhost:7093/identities/login?emailConfirmed=true"));
        }

        [Test]
        public async Task ConfirmEmailAsync_FailedResult_RedirectsToLoginWithFailedParam()
        {
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<ConfirmEmailCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CommandResponse.Failed("Invalid token."));

            var result = await _controller.ConfirmEmailAsync("user-id", "bad-token", CancellationToken.None);

            var redirect = result as RedirectResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.Url, Is.EqualTo("https://localhost:7093/identities/login?emailConfirmationFailed=true"));
        }

        [Test]
        public async Task ConfirmEmailAsync_NullResult_RedirectsToLoginWithFailedParam()
        {
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<ConfirmEmailCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CommandResponse?)null);

            var result = await _controller.ConfirmEmailAsync("user-id", "token", CancellationToken.None);

            var redirect = result as RedirectResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.Url, Is.EqualTo("https://localhost:7093/identities/login?emailConfirmationFailed=true"));
        }

        [Test]
        public async Task ConfirmEmailAsync_SendsCommandWithCorrectUserIdAndToken()
        {
            ConfirmEmailCommand? captured = null;

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<ConfirmEmailCommand>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<CommandResponse?>, CancellationToken>((cmd, _) => captured = (ConfirmEmailCommand)cmd)
                .ReturnsAsync(CommandResponse.Success());

            await _controller.ConfirmEmailAsync("abc-123", "my-token", CancellationToken.None);

            Assert.That(captured, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(captured!.UserId, Is.EqualTo("abc-123"));
                Assert.That(captured.Token, Is.EqualTo("my-token"));
            });
        }
    }
}