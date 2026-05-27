using Bunit;
using Common.Models;
using Common.UI;
using Identity.Services;
using Identity.Shared.Models;
using MealPlanner.UI.Web.Pages.Identities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MealPlanner.UI.Web.Tests.Pages.Identities
{
    [TestFixture]
    public class ForgotPasswordTests
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
        }

        [TearDown]
        public void TearDown() => _ctx.Dispose();

        private IRenderedComponent<ForgotPassword> RenderComponent() =>
            _ctx.Render<ForgotPassword>(ps =>
                ps.AddCascadingValue("MessageComponent", _messageComponentMock.Object));

        private static System.Reflection.MethodInfo GetMethod(string name) =>
            typeof(ForgotPassword).GetMethod(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

        [Test]
        public async Task OnSubmitAsync_Success_ShowsInfoMessage()
        {
            var result = new CommandResponse { Succeeded = true, Message = "Reset link sent." };

            _authServiceMock
                .Setup(s => s.ForgotPasswordAsync(It.IsAny<ForgotPasswordModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            var cut = RenderComponent();
            var method = GetMethod("OnSubmitAsync");
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            _messageComponentMock.Verify(
                m => m.ShowInfoAsync("Reset link sent.", It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task OnSubmitAsync_Failure_ShowsErrorMessage()
        {
            var result = new CommandResponse { Succeeded = false, Message = "Something went wrong." };

            _authServiceMock
                .Setup(s => s.ForgotPasswordAsync(It.IsAny<ForgotPasswordModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            var cut = RenderComponent();
            var method = GetMethod("OnSubmitAsync");

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("Something went wrong.", It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task OnSubmitAsync_NullResult_ShowsGenericError()
        {
            _authServiceMock
                .Setup(s => s.ForgotPasswordAsync(It.IsAny<ForgotPasswordModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent();
            var method = GetMethod("OnSubmitAsync");

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            _messageComponentMock.Verify(
                m => m.ShowErrorAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public void NavigateToLogin_NavigatesToLoginPage()
        {
            var cut = RenderComponent();
            var method = GetMethod("NavigateToLogin");
            Assert.That(method, Is.Not.Null);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            cut.InvokeAsync(() => method.Invoke(cut.Instance, [])).GetAwaiter().GetResult();

            Assert.That(nav.Uri, Is.EqualTo("http://localhost/identities/login"));
        }
    }
}
