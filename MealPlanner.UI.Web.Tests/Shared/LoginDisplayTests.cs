using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using Common.Models;
using Common.UI;
using Identity.Shared.Models;
using MealPlanner.UI.Web.Services.Identities;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace MealPlanner.UI.Web.Tests.Shared
{
    [TestFixture]
    public class LoginDisplayTests 
    {
        private Mock<IAuthenticationService> _authServiceMock = null!;
        private Mock<IApplicationUserService> _userServiceMock = null!;
        private Mock<IMessageComponent> _messageComponentMock = null!;
        private Mock<ILogger<LoginDisplay>> _loggerMock = null!;
        private BunitAuthorizationContext _authContext;
        private BunitContext _ctx = null!;

        [SetUp]
        public void Setup()
        {
            _ctx = new BunitContext();
            _authContext = _ctx.AddAuthorization();

            _authServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);
            _userServiceMock = new Mock<IApplicationUserService>(MockBehavior.Strict);
            _messageComponentMock = new Mock<IMessageComponent>(MockBehavior.Loose);
            _loggerMock = new Mock<ILogger<LoginDisplay>>(MockBehavior.Loose);

            _ctx.Services.AddSingleton(_authServiceMock.Object);
            _ctx.Services.AddSingleton(_userServiceMock.Object);
            _ctx.Services.AddSingleton(_messageComponentMock.Object);
            _ctx.Services.AddSingleton(_loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
            _authServiceMock.Reset();
            _userServiceMock.Reset();
            _messageComponentMock.Reset();
            _loggerMock.Reset();
        }

        private static AuthenticationState CreateAuthState(bool isAuthenticated, string? name = null)
        {
            ClaimsIdentity identity;

            if (isAuthenticated)
            {
                var claims = new List<Claim>();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    claims.Add(new Claim(ClaimTypes.Name, name));
                }

                identity = new ClaimsIdentity(claims, "TestAuthType");
            }
            else
            {
                identity = new ClaimsIdentity(); // not authenticated
            }

            var principal = new ClaimsPrincipal(identity);
            return new AuthenticationState(principal);
        }

        private IRenderedComponent<LoginDisplay> RenderWithAuthAndMessageComponent(AuthenticationState authState)
        {
            var authTask = Task.FromResult(authState);

            return _ctx.Render<LoginDisplay>(parameters => parameters
                .AddCascadingValue("MessageComponent", _messageComponentMock.Object)
                .AddCascadingValue(authTask)
            );
        }

        // ---------- OnInitializedAsync ----------
        [Test]
        public void OnInitializedAsync_DoesNothing_WhenUserNotAuthenticated()
        {
            // Arrange
            _authContext.SetNotAuthorized();
            var authState = CreateAuthState(isAuthenticated: false, name: null);

            _userServiceMock
                .Setup(s => s.GetEditAsync(It.IsAny<string>()))
                .Throws(new Exception("Should not be called"));

            // Act
            var cut = RenderWithAuthAndMessageComponent(authState);

            // Assert
            Assert.That(cut.Instance.ApplicationUser, Is.Null);
            _userServiceMock.Verify(s => s.GetEditAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void OnInitializedAsync_LoadsUser_WhenAuthenticatedWithName()
        {
            // Arrange
            const string username = "test.user";
            _authContext.SetAuthorized(username);
            var authState = CreateAuthState(isAuthenticated: true, name: username);

            var userModel = new ApplicationUserEditModel();
            _userServiceMock
                .Setup(s => s.GetEditAsync(username))
                .ReturnsAsync(userModel);

            // Act
            var cut = RenderWithAuthAndMessageComponent(authState);

            // Assert
            Assert.That(cut.Instance.ApplicationUser, Is.SameAs(userModel));
            _userServiceMock.Verify(s => s.GetEditAsync(username), Times.Once);
        }

        [Test]
        public void OnInitializedAsync_LogsErrorAndShowsMessage_WhenUserLoadFails()
        {
            // Arrange
            const string username = "test.user";
            var authState = CreateAuthState(isAuthenticated: true, name: username);

            _userServiceMock
                .Setup(s => s.GetEditAsync(username))
                .ThrowsAsync(new Exception("boom"));

            // Act
            var cut = RenderWithAuthAndMessageComponent(authState);

            // Assert: UI message
            _messageComponentMock.Verify(
                m => m.ShowError("Failed to load user profile."),
                Times.Once);

            // Assert: logging (basic check on LogError call)
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) =>
                        v.ToString()!.Contains("Failed to load ApplicationUser")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        // ---------- LogoutAsync ----------
        [Test]
        public async Task LogoutAsync_NavigatesToRoot_WhenSucceeded()
        {
            // Arrange
            var authState = CreateAuthState(isAuthenticated: true, name: "test.user");

            _userServiceMock
                .Setup(s => s.GetEditAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUserEditModel());

            _authServiceMock
                .Setup(s => s.LogoutAsync())
                .ReturnsAsync(new CommandResponse { Succeeded = true });

            var cut = RenderWithAuthAndMessageComponent(authState);
            var nav = _ctx.Services.GetRequiredService<NavigationManager>() as BunitNavigationManager
                      ?? throw new InvalidOperationException("NavigationManager not available");

            // Act
            await cut.Instance.LogoutAsync();

            // Assert
            Assert.That(nav.Uri, Is.EqualTo("http://localhost/"));
            _authServiceMock.Verify(s => s.LogoutAsync(), Times.Once);
        }

        [Test]
        public async Task LogoutAsync_ShowsErrorMessage_WhenResponseFailed()
        {
            // Arrange
            const string username = "test.user";
            _authContext.SetAuthorized(username);
            var authState = CreateAuthState(isAuthenticated: true, name: username);

            _userServiceMock
                .Setup(s => s.GetEditAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUserEditModel());

            var response = new CommandResponse
            {
                Succeeded = false,
                Message = "Logout failed: test"
            };

            _authServiceMock
                .Setup(s => s.LogoutAsync())
                .ReturnsAsync(response);

            var cut = RenderWithAuthAndMessageComponent(authState);

            // Act
            await cut.Instance.LogoutAsync();

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowError("Logout failed: test"),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) =>
                        v.ToString()!.Contains("Logout failed.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public async Task LogoutAsync_ShowsGenericError_WhenResponseIsNull()
        {
            // Arrange
            const string username = "test.user";
            _authContext.SetAuthorized(username);
            var authState = CreateAuthState(isAuthenticated: true, name: username);

            _userServiceMock
                .Setup(s => s.GetEditAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUserEditModel());

            _authServiceMock
                .Setup(s => s.LogoutAsync())
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderWithAuthAndMessageComponent(authState);

            // Act
            await cut.Instance.LogoutAsync();

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowError("Logout failed. Please try again."),
                Times.Once);
        }

        [Test]
        public async Task LogoutAsync_LogsErrorAndShowsGenericMessage_OnException()
        {
            // Arrange
            const string username = "test.user";
            _authContext.SetAuthorized(username);
            var authState = CreateAuthState(isAuthenticated: true, name: username);

            _userServiceMock
                .Setup(s => s.GetEditAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUserEditModel());

            _authServiceMock
                .Setup(s => s.LogoutAsync())
                .ThrowsAsync(new Exception("boom"));

            var cut = RenderWithAuthAndMessageComponent(authState);

            // Act
            await cut.Instance.LogoutAsync();

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowError("Unexpected error during logout. Please try again."),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) =>
                        v.ToString()!.Contains("Unexpected error during logout")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}