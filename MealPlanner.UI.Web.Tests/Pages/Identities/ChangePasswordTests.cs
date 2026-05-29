using BlazorBootstrap;
using Bunit;
using Common.Models;
using Common.UI;
using Identity.Services.Core;
using Identity.Shared.Models;
using MealPlanner.UI.Web.Pages.Identities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MealPlanner.UI.Web.Tests.Pages.Identities
{
    [TestFixture]
    public class ChangePasswordTests
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

            _ctx.Services.AddScoped<BreadcrumbService>();
            _ctx.Services.AddScoped<ModalService>();
            _ctx.Services.AddScoped<PreloadService>();

            _ctx.JSInterop.SetupVoid("window.blazorBootstrap.dropdown.initialize", _ => true);
        }

        [TearDown]
        public void TearDown() => _ctx.Dispose();

        private IRenderedComponent<ChangePassword> RenderComponent(string? userId = "user-id", string? name = null)
        {
            var qs = new List<string>();
            if (userId is not null) qs.Add($"userId={userId}");
            if (name is not null) qs.Add($"name={name}");
            var query = qs.Count > 0 ? "?" + string.Join("&", qs) : string.Empty;

            _ctx.Services.GetRequiredService<NavigationManager>()
                .NavigateTo($"http://localhost/identities/change-password{query}");

            return _ctx.Render<ChangePassword>(ps =>
                ps.AddCascadingValue("MessageComponent", _messageComponentMock.Object));
        }

        private static System.Reflection.MethodInfo GetMethod(string name) =>
            typeof(ChangePassword).GetMethod(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

        [Test]
        public void OnInitialized_WithValidUserId_PopulatesModel()
        {
            var cut = RenderComponent(userId: "user-id");

            Assert.That(cut.Instance.ChangePasswordModel.UserId, Is.EqualTo("user-id"));
        }

        [Test]
        public void OnInitialized_WithMissingUserId_RendersInvalidLinkMessage()
        {
            _ctx.Services.GetRequiredService<NavigationManager>()
                .NavigateTo("http://localhost/identities/change-password");

            var cut = _ctx.Render<ChangePassword>(ps =>
                ps.AddCascadingValue("MessageComponent", _messageComponentMock.Object));

            Assert.That(cut.Markup, Does.Contain("Invalid link").IgnoreCase.Or.Contain("invalid").IgnoreCase);
        }

        [Test]
        public async Task OnSubmitAsync_Success_ShowsInfoAndNavigatesBack()
        {
            var result = new CommandResponse { Succeeded = true, Message = "Password changed successfully." };

            _authServiceMock
                .Setup(s => s.ChangePasswordAsync(It.IsAny<ChangePasswordModel>(), It.IsAny<CancellationToken>()))
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
                m => m.ShowInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.That(nav.Uri, Does.Contain("userprofile").IgnoreCase);
        }

        [Test]
        public async Task OnSubmitAsync_Failure_ShowsErrorMessage()
        {
            var result = new CommandResponse { Succeeded = false, Message = "Incorrect current password." };

            _authServiceMock
                .Setup(s => s.ChangePasswordAsync(It.IsAny<ChangePasswordModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            var cut = RenderComponent();
            var method = GetMethod("OnSubmitAsync");

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("Incorrect current password.", It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task OnSubmitAsync_NullResult_ShowsGenericError()
        {
            _authServiceMock
                .Setup(s => s.ChangePasswordAsync(It.IsAny<ChangePasswordModel>(), It.IsAny<CancellationToken>()))
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
        public void NavigateBack_WithName_NavigatesToUserProfileWithName()
        {
            var cut = RenderComponent(userId: "user-id", name: "testuser");
            var method = GetMethod("NavigateBack");
            Assert.That(method, Is.Not.Null);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            cut.InvokeAsync(() => method.Invoke(cut.Instance, [])).GetAwaiter().GetResult();

            Assert.That(nav.Uri, Does.Contain("userprofile/testuser").IgnoreCase);
        }

        [Test]
        public void NavigateBack_WithoutName_NavigatesToUserProfileWithoutName()
        {
            var cut = RenderComponent(userId: "user-id", name: null);
            var method = GetMethod("NavigateBack");

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            cut.InvokeAsync(() => method.Invoke(cut.Instance, [])).GetAwaiter().GetResult();

            Assert.That(nav.Uri, Does.Contain("userprofile").IgnoreCase);
            Assert.That(nav.Uri, Does.Not.Contain("userprofile/"));
        }

        [Test]
        public async Task OnSubmitAsync_Success_NavigatesToUserProfileWithName()
        {
            var result = new CommandResponse { Succeeded = true, Message = "Password changed successfully." };

            _authServiceMock
                .Setup(s => s.ChangePasswordAsync(It.IsAny<ChangePasswordModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            var cut = RenderComponent(userId: "user-id", name: "testuser");
            var method = GetMethod("OnSubmitAsync");
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            Assert.That(nav.Uri, Does.Contain("userprofile/testuser").IgnoreCase);
        }
    }
}
