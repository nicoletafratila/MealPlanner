using BlazorBootstrap;
using Bunit;
using Bunit.TestDoubles;
using Common.Models;
using Common.UI;
using Identity.Shared.Models;
using MealPlanner.UI.Web.Pages.Identities;
using Identity.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MealPlanner.UI.Web.Tests.Pages.Identities
{
    [TestFixture]
    public class UserProfileTests
    {
        private BunitContext _ctx = null!;
        private Mock<IApplicationUserService> _appUserServiceMock = null!;
        private Mock<IMessageComponent> _messageComponentMock = null!;
        private BunitAuthorizationContext _authContext = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _appUserServiceMock = new Mock<IApplicationUserService>(MockBehavior.Loose);
            _messageComponentMock = new Mock<IMessageComponent>(MockBehavior.Loose);

            _ctx.Services.AddSingleton(_appUserServiceMock.Object);
            _ctx.Services.AddSingleton(_messageComponentMock.Object);
            _ctx.Services.AddLogging();

            _authContext = _ctx.AddAuthorization();
            _ctx.Services.AddScoped<BreadcrumbService>();
            _ctx.Services.AddScoped<ModalService>();
            _ctx.Services.AddScoped<PreloadService>();

            _ctx.JSInterop.SetupVoid("window.blazorBootstrap.dropdown.initialize", _ => true);
        }

        [TearDown]
        public void TearDown() => _ctx.Dispose();

        private IRenderedComponent<UserProfile> RenderWithName(string name, string userId = "user-id")
        {
            _appUserServiceMock
                .Setup(s => s.GetEditAsync(name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApplicationUserEditModel
                {
                    UserId = userId,
                    Username = name,
                    FirstName = "Test",
                    LastName = "User"
                });

            return _ctx.Render<UserProfile>(ps =>
            {
                ps.AddCascadingValue("MessageComponent", _messageComponentMock.Object);
                ps.Add(p => p.Name, name);
            });
        }

        private IRenderedComponent<UserProfile> RenderWithoutName()
        {
            return _ctx.Render<UserProfile>(ps =>
                ps.AddCascadingValue("MessageComponent", _messageComponentMock.Object));
        }

        private static System.Reflection.MethodInfo GetMethod(string name) =>
            typeof(UserProfile).GetMethod(name,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

        // ---------- OnInitializedAsync ----------

        [Test]
        public void OnInitializedAsync_WithNoName_CreatesEmptyApplicationUser()
        {
            var cut = RenderWithoutName();

            Assert.That(cut.Instance.ApplicationUser, Is.Not.Null);

            _appUserServiceMock.Verify(
                s => s.GetEditAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public void OnInitializedAsync_WithName_LoadsApplicationUserFromService()
        {
            var cut = RenderWithName("testuser", userId: "user-id");

            Assert.That(cut.Instance.ApplicationUser, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(cut.Instance.ApplicationUser!.Username, Is.EqualTo("testuser"));
                Assert.That(cut.Instance.ApplicationUser.UserId, Is.EqualTo("user-id"));
            });

            _appUserServiceMock.Verify(
                s => s.GetEditAsync("testuser", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ---------- SaveAsync ----------

        [Test]
        public async Task SaveAsync_Success_AsAdmin_ShowsInfoAndNavigatesToUsersOverview()
        {
            _authContext.SetRoles("admin");

            _appUserServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<ApplicationUserEditModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CommandResponse { Succeeded = true });

            var cut = RenderWithName("testuser");
            var method = GetMethod("SaveAsync");
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            _messageComponentMock.Verify(
                m => m.ShowInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.That(nav.Uri, Does.Contain("usersoverview").IgnoreCase);
        }

        [Test]
        public async Task SaveAsync_Success_AsMember_ShowsInfoAndNavigatesToRecipesOverview()
        {
            _appUserServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<ApplicationUserEditModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CommandResponse { Succeeded = true });

            var cut = RenderWithName("testuser");
            var method = GetMethod("SaveAsync");
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            _messageComponentMock.Verify(
                m => m.ShowInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.That(nav.Uri, Does.Contain("recipesoverview").IgnoreCase);
        }

        [Test]
        public async Task SaveAsync_Failure_ShowsErrorMessage()
        {
            _appUserServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<ApplicationUserEditModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CommandResponse { Succeeded = false, Message = "Save failed." });

            var cut = RenderWithName("testuser");
            var method = GetMethod("SaveAsync");
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            var initialUri = nav.Uri;

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("Save failed.", It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.That(nav.Uri, Is.EqualTo(initialUri));
        }

        [Test]
        public async Task SaveAsync_NullResponse_AsAdmin_ShowsInfoAndNavigatesToUsersOverview()
        {
            _authContext.SetRoles("admin");

            _appUserServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<ApplicationUserEditModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderWithName("testuser");
            var method = GetMethod("SaveAsync");
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            _messageComponentMock.Verify(
                m => m.ShowInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.That(nav.Uri, Does.Contain("usersoverview").IgnoreCase);
        }

        // ---------- NavigateToOverview ----------

        [Test]
        public void NavigateToOverview_AsAdmin_NavigatesToUsersOverview()
        {
            _authContext.SetRoles("admin");

            var cut = RenderWithName("testuser");
            var method = GetMethod("NavigateToOverview");
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            cut.InvokeAsync(() => method.Invoke(cut.Instance, [])).GetAwaiter().GetResult();

            Assert.That(nav.Uri, Does.Contain("usersoverview").IgnoreCase);
        }

        [Test]
        public void NavigateToOverview_AsMember_NavigatesToRecipesOverview()
        {
            var cut = RenderWithName("testuser");
            var method = GetMethod("NavigateToOverview");
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            cut.InvokeAsync(() => method.Invoke(cut.Instance, [])).GetAwaiter().GetResult();

            Assert.That(nav.Uri, Does.Contain("recipesoverview").IgnoreCase);
        }

        // ---------- NavigateToChangePassword ----------

        [Test]
        public void NavigateToChangePassword_NavigatesToChangePasswordWithUserIdAndName()
        {
            var cut = RenderWithName("testuser", userId: "user-id");
            var method = GetMethod("NavigateToChangePassword");
            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            cut.InvokeAsync(() => method.Invoke(cut.Instance, [])).GetAwaiter().GetResult();

            Assert.That(nav.Uri, Does.Contain("change-password").IgnoreCase);
            Assert.That(nav.Uri, Does.Contain("userId=user-id"));
            Assert.That(nav.Uri, Does.Contain("name=testuser"));
        }

        // ---------- UnlockUserAsync ----------

        private IRenderedComponent<UserProfile> RenderLockedUser(string name = "testuser", string userId = "user-id")
        {
            _appUserServiceMock
                .Setup(s => s.GetEditAsync(name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApplicationUserEditModel
                {
                    UserId = userId,
                    Username = name,
                    FirstName = "Test",
                    LastName = "User",
                    IsLockedOut = true
                });

            return _ctx.Render<UserProfile>(ps =>
            {
                ps.AddCascadingValue("MessageComponent", _messageComponentMock.Object);
                ps.Add(p => p.Name, name);
            });
        }

        [Test]
        public async Task UnlockUserAsync_Success_UpdatesIsLockedOut_AndShowsInfo()
        {
            _appUserServiceMock
                .Setup(s => s.UnlockAsync("user-id", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CommandResponse { Succeeded = true });

            var cut = RenderLockedUser(userId: "user-id");
            var method = GetMethod("UnlockUserAsync");

            Assert.That(cut.Instance.ApplicationUser!.IsLockedOut, Is.True);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            Assert.That(cut.Instance.ApplicationUser!.IsLockedOut, Is.False);

            _messageComponentMock.Verify(
                m => m.ShowInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowErrorAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task UnlockUserAsync_Failure_ShowsError_AndIsLockedOutUnchanged()
        {
            _appUserServiceMock
                .Setup(s => s.UnlockAsync("user-id", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CommandResponse { Succeeded = false, Message = "Unlock failed." });

            var cut = RenderLockedUser(userId: "user-id");
            var method = GetMethod("UnlockUserAsync");

            Assert.That(cut.Instance.ApplicationUser!.IsLockedOut, Is.True);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            Assert.That(cut.Instance.ApplicationUser!.IsLockedOut, Is.True);

            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("Unlock failed.", It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
