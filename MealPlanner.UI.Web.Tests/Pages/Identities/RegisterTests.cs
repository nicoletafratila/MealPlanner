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
    public class RegisterTests
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
        public void TearDown()
        {
            _ctx.Dispose();
        }

        private IRenderedComponent<Register> RenderComponent()
        {
            return _ctx.Render<Register>(ps =>
            {
                ps.AddCascadingValue("MessageComponent", _messageComponentMock.Object);
            });
        }

        [Test]
        public async Task OnRegisterAsync_Success_NavigatesToLogin()
        {
            // Arrange
            var result = new CommandResponse { Succeeded = true, Message = "Registration successful." };

            _authServiceMock
                .Setup(s => s.RegisterAsync(It.IsAny<RegistrationModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            var cut = RenderComponent();
            var method = typeof(Register).GetMethod("OnRegisterAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            Assert.That(nav.Uri, Is.EqualTo("http://localhost/"));
        }

        [Test]
        public async Task OnRegisterAsync_Failure_ShowsErrorMessage()
        {
            // Arrange
            var result = new CommandResponse { Succeeded = false, Message = "Username is already taken." };

            _authServiceMock
                .Setup(s => s.RegisterAsync(It.IsAny<RegistrationModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            var cut = RenderComponent();
            var method = typeof(Register).GetMethod("OnRegisterAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync("Username is already taken.", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
        }

        [Test]
        public async Task OnRegisterAsync_NullResult_ShowsGenericError()
        {
            // Arrange
            _authServiceMock
                .Setup(s => s.RegisterAsync(It.IsAny<RegistrationModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent();
            var method = typeof(Register).GetMethod("OnRegisterAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowErrorAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None),
                Times.Once);
        }

        [Test]
        public void NavigateToLogin_NavigatesToLoginPage()
        {
            // Arrange
            var cut = RenderComponent();
            var method = typeof(Register).GetMethod("NavigateToLogin", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();

            // Act
            cut.InvokeAsync(() =>
            {
                method!.Invoke(cut.Instance, []);
            }).GetAwaiter().GetResult();

            // Assert
            Assert.That(nav.Uri, Is.EqualTo("http://localhost/identities/login"));
        }
    }
}
