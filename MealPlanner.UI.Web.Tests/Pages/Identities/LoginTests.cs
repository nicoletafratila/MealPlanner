using Bunit;
using Common.Models;
using Common.UI;
using Identity.Shared.Models;
using MealPlanner.UI.Web.Pages.Identities;
using MealPlanner.UI.Web.Services.Identities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MealPlanner.UI.Web.Tests.Pages.Identities
{
    [TestFixture]
    public class LoginTests
    {
        private BunitContext _ctx = null!;
        private Mock<IAuthenticationService> _authServiceMock = null!;
        private Mock<IMessageComponent> _messageComponentMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _authServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);
            _messageComponentMock = new Mock<IMessageComponent>(MockBehavior.Loose);

            _ctx.Services.AddSingleton(_authServiceMock.Object);
            _ctx.Services.AddSingleton(_messageComponentMock.Object);

            _ctx.Services.AddLogging();

            _ctx.JSInterop
                .SetupVoid("focusElement", _ => true)
                .SetVoidResult();
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
        }

        private IRenderedComponent<Login> RenderComponent()
        {
            return _ctx.Render<Login>(ps =>
            {
                ps.AddCascadingValue("MessageComponent", _messageComponentMock.Object);
            });
        }

        // ---------- OnLoginAsync ----------
        [Test]
        public async Task OnLoginAsync_Success_NavigatesToRoot()
        {
            // Arrange
            var result = new CommandResponse
            {
                Succeeded = true,
                Message = "ok"
            };

            _authServiceMock
                .Setup(s => s.LoginAsync(It.IsAny<LoginModel>(), CancellationToken.None))
                .ReturnsAsync(result);

            var cut = RenderComponent();

            // Set credentials
            cut.Instance.LoginModel.Username = "user";
            cut.Instance.LoginModel.Password = "pass";

            var method = typeof(Login).GetMethod("OnLoginAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            Assert.That(nav.Uri, Is.EqualTo("http://localhost/")); // bUnit default base is http://localhost/
        }

        [Test]
        public async Task OnLoginAsync_Failure_ShowsErrorMessage()
        {
            // Arrange
            var result = new CommandResponse
            {
                Succeeded = false,
                Message = "Invalid credentials"
            };

            _authServiceMock
                .Setup(s => s.LoginAsync(It.IsAny<LoginModel>(), CancellationToken.None))
                .ReturnsAsync(result);

            var cut = RenderComponent();

            var method = typeof(Login).GetMethod("OnLoginAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("Invalid credentials", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
        }

        [Test]
        public async Task OnLoginAsync_NullResult_ShowsGenericError()
        {
            // Arrange
            _authServiceMock
                .Setup(s => s.LoginAsync(It.IsAny<LoginModel>(), CancellationToken.None))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent();

            var method = typeof(Login).GetMethod("OnLoginAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("Login failed. Please try again.", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
        }

        // ---------- HandleKeyDown ----------
        [Test]
        public async Task HandleKeyDown_Enter_TriggersLogin()
        {
            // Arrange
            var result = new CommandResponse
            {
                Succeeded = true
            };

            _authServiceMock
                .Setup(s => s.LoginAsync(It.IsAny<LoginModel>(), CancellationToken.None))
                .ReturnsAsync(result);

            var cut = RenderComponent();

            var method = typeof(Login).GetMethod("HandleKeyDown", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var args = new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs
            {
                Key = "Enter"
            };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [args])!;
                await task;
            });

            // Assert
            _authServiceMock.Verify(
                s => s.LoginAsync(It.IsAny<LoginModel>(), CancellationToken.None),
                Times.Once);
        }

        [Test]
        public async Task HandleKeyDown_NonEnter_DoesNotTriggerLogin()
        {
            // Arrange
            var cut = RenderComponent();

            var method = typeof(Login).GetMethod("HandleKeyDown", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var args = new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs
            {
                Key = "Escape"
            };

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [args])!;
                await task;
            });

            // Assert
            _authServiceMock.Verify(
                s => s.LoginAsync(It.IsAny<LoginModel>(), CancellationToken.None),
                Times.Never);
        }

        // ---------- OnAfterRenderAsync ----------
        [Test]
        public void OnAfterRenderAsync_FirstRender_FocusesUsername()
        {
            // Arrange
            var cut = RenderComponent();

            // The JSInterop call is stubbed in SetUp. We just assert no exception and
            // that the call has been invoked once.
            var invocations = _ctx.JSInterop.Invocations;

            Assert.That(
                invocations.Any(i => i.Identifier == "focusElement" &&
                                     i.Arguments.Count == 1 &&
                                     i.Arguments[0]?.ToString() == "username"),
                Is.True);
        }
    }
}