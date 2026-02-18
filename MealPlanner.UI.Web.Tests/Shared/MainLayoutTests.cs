using BlazorBootstrap;
using Bunit;
using Bunit.TestDoubles;
using Common.UI;
using MealPlanner.UI.Web.Services.Identities;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MealPlanner.UI.Web.Tests.Shared
{
    [TestFixture]
    public class MainLayoutTests : BunitContext
    {
        private readonly BunitAuthorizationContext _authContext;

        public MainLayoutTests()
        {
            _authContext = this.AddAuthorization();
            _authContext.SetNotAuthorized();

            Services.AddScoped<ModalService>();
            Services.AddScoped<ToastService>();
            Services.AddScoped<PreloadService>();

            JSInterop.SetupVoid("window.blazorBootstrap.modal.initialize", _ => true);
            JSInterop.SetupVoid("window.blazorBootstrap.alert.initialize", _ => true); 

            var authServiceMock = new Mock<IAuthenticationService>(MockBehavior.Loose);
            var userServiceMock = new Mock<IApplicationUserService>(MockBehavior.Loose);

            Services.AddSingleton(authServiceMock.Object);
            Services.AddSingleton(userServiceMock.Object);

            Services.AddLogging();
        }

        private class MessageConsumer : ComponentBase
        {
            [CascadingParameter(Name = "MessageComponent")]
            public IMessageComponent? MessageComponent { get; set; }

            [Parameter]
            public string Message { get; set; } = "Child message";

            protected override void OnInitialized()
            {
                MessageComponent?.ShowInfo(Message);
            }
        }

        [Test]
        public async Task ShowError_SetsErrorState_AndMessage_AndClearsInfo()
        {
            // Arrange
            var cut = Render<MainLayout>();

            // Act
            await cut.InvokeAsync(() => cut.Instance.ShowError("Something went wrong"));

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(cut.Instance.IsErrorActive, Is.True);
                Assert.That(cut.Instance.IsInfoActive, Is.False);
                Assert.That(cut.Instance.Message, Is.EqualTo("Something went wrong"));
                Assert.That(cut.Markup, Does.Contain("Something went wrong"));
            });
        }

        [Test]
        public async Task ShowInfo_SetsInfoState_AndMessage_AndClearsError()
        {
            // Arrange
            var cut = Render<MainLayout>();
            await cut.InvokeAsync(() => cut.Instance.ShowError("Old error"));

            // Act
            await cut.InvokeAsync(() => cut.Instance.ShowInfo("Informational message"));

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(cut.Instance.IsErrorActive, Is.False);
                Assert.That(cut.Instance.IsInfoActive, Is.True);
                Assert.That(cut.Instance.Message, Is.EqualTo("Informational message"));
                Assert.That(cut.Markup, Does.Contain("Informational message"));
            });
        }

        [Test]
        public async Task HideError_ClearsErrorFlag_ButKeepsMessage()
        {
            // Arrange
            var cut = Render<MainLayout>();
            await cut.InvokeAsync(() => cut.Instance.ShowError("Error message"));

            // Act
            await cut.InvokeAsync(() => cut.Instance.HideError());

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(cut.Instance.IsErrorActive, Is.False);
                Assert.That(cut.Instance.IsInfoActive, Is.False);
                Assert.That(cut.Instance.Message, Is.EqualTo("Error message"));
            });
        }

        [Test]
        public async Task HideInfo_ClearsInfoFlag_ButKeepsMessage()
        {
            // Arrange
            var cut = Render<MainLayout>();
            await cut.InvokeAsync(() => cut.Instance.ShowInfo("Info message"));

            // Act
            await cut.InvokeAsync(() => cut.Instance.HideInfo());

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(cut.Instance.IsInfoActive, Is.False);
                Assert.That(cut.Instance.IsErrorActive, Is.False);
                Assert.That(cut.Instance.Message, Is.EqualTo("Info message"));
            });
        }

        [Test]
        public async Task ShowError_AfterShowInfo_OverridesInfoState()
        {
            // Arrange
            var cut = Render<MainLayout>();
            await cut.InvokeAsync(() => cut.Instance.ShowInfo("Info first"));

            // Act
            await cut.InvokeAsync(() => cut.Instance.ShowError("Error now"));

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(cut.Instance.IsErrorActive, Is.True);
                Assert.That(cut.Instance.IsInfoActive, Is.False);
                Assert.That(cut.Instance.Message, Is.EqualTo("Error now"));
            });
        }

        [Test]
        public async Task ShowInfo_AfterShowError_OverridesErrorState()
        {
            // Arrange
            var cut = Render<MainLayout>();
            await cut.InvokeAsync(() => cut.Instance.ShowError("Error first"));

            // Act
            await cut.InvokeAsync(() => cut.Instance.ShowInfo("Info now"));

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(cut.Instance.IsErrorActive, Is.False);
                Assert.That(cut.Instance.IsInfoActive, Is.True);
                Assert.That(cut.Instance.Message, Is.EqualTo("Info now"));
            });
        }

        [Test]
        public void Layout_Cascades_IMessageComponent_ToChildren()
        {
            // Arrange
            const string childMessage = "Message from child";

            var cut = Render<MainLayout>(parameters =>
            {
                parameters.Add(p => p.Body, (RenderFragment)(builder =>
                {
                    builder.OpenComponent<MessageConsumer>(0);
                    builder.AddAttribute(1, nameof(MessageConsumer.Message), childMessage);
                    builder.CloseComponent();
                }));
            });

            Assert.Multiple(() =>
            {
                // Assert: child used the cascaded IMessageComponent to call ShowInfo
                Assert.That(cut.Instance.IsInfoActive, Is.True);
                Assert.That(cut.Instance.IsErrorActive, Is.False);
                Assert.That(cut.Instance.Message, Is.EqualTo(childMessage));
                Assert.That(cut.Markup, Does.Contain(childMessage));
            });
        }
    }
}
