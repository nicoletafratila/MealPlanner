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
    public class ResetPasswordTests
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

        private IRenderedComponent<ResetPassword> RenderComponent(string? userId = "user-id", string? token = "reset-token")
        {
            var qs = new List<string>();
            if (userId is not null) qs.Add($"userId={userId}");
            if (token is not null) qs.Add($"token={token}");
            var query = qs.Count > 0 ? "?" + string.Join("&", qs) : string.Empty;

            _ctx.Services.GetRequiredService<NavigationManager>()
                .NavigateTo($"http://localhost/identities/reset-password{query}");

            return _ctx.Render<ResetPassword>(ps =>
                ps.AddCascadingValue("MessageComponent", _messageComponentMock.Object));
        }

        private static System.Reflection.MethodInfo GetMethod(string name) =>
            typeof(ResetPassword).GetMethod(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

        [Test]
        public void OnInitialized_WithValidQueryParams_PopulatesModel()
        {
            var cut = RenderComponent(userId: "user-id", token: "reset-token");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(cut.Instance.ResetPasswordModel.UserId, Is.EqualTo("user-id"));
                Assert.That(cut.Instance.ResetPasswordModel.Token, Is.EqualTo("reset-token"));
            }
        }

        [Test]
        public void OnInitialized_WithMissingUserId_RendersInvalidLinkMessage()
        {
            _ctx.Services.GetRequiredService<NavigationManager>()
                .NavigateTo("http://localhost/identities/reset-password?token=reset-token");

            var cut = _ctx.Render<ResetPassword>(ps =>
                ps.AddCascadingValue("MessageComponent", _messageComponentMock.Object));

            Assert.That(cut.Markup, Does.Contain("invalid").IgnoreCase.Or.Contain("expired").IgnoreCase);
        }

        [Test]
        public void OnInitialized_WithMissingToken_RendersInvalidLinkMessage()
        {
            _ctx.Services.GetRequiredService<NavigationManager>()
                .NavigateTo("http://localhost/identities/reset-password?userId=user-id");

            var cut = _ctx.Render<ResetPassword>(ps =>
                ps.AddCascadingValue("MessageComponent", _messageComponentMock.Object));

            Assert.That(cut.Markup, Does.Contain("invalid").IgnoreCase.Or.Contain("expired").IgnoreCase);
        }

        [Test]
        public async Task OnSubmitAsync_Success_ShowsInfoAndNavigatesToLogin()
        {
            var result = new CommandResponse { Succeeded = true, Message = "Password reset successfully." };

            _authServiceMock
                .Setup(s => s.ResetPasswordAsync(It.IsAny<ResetPasswordModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            var cut = RenderComponent();
            var method = GetMethod("OnSubmitAsync");
            Assert.That(method, Is.Not.Null);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            _messageComponentMock.Verify(
                m => m.ShowInfoAsync("Password reset successfully.", It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.That(nav.Uri, Is.EqualTo("http://localhost/identities/login"));
        }

        [Test]
        public async Task OnSubmitAsync_Failure_ShowsErrorMessage()
        {
            var result = new CommandResponse { Succeeded = false, Message = "Reset failed." };

            _authServiceMock
                .Setup(s => s.ResetPasswordAsync(It.IsAny<ResetPasswordModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            var cut = RenderComponent();
            var method = GetMethod("OnSubmitAsync");

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("Reset failed.", It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task OnSubmitAsync_NullResult_ShowsGenericError()
        {
            _authServiceMock
                .Setup(s => s.ResetPasswordAsync(It.IsAny<ResetPasswordModel>(), It.IsAny<CancellationToken>()))
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
