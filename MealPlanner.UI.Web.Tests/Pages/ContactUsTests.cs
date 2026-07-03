using Bunit;
using Common.Models;
using Common.UI;
using Identity.Services.Http;
using Identity.Shared.Models;
using MealPlanner.UI.Web.Pages;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MealPlanner.UI.Web.Tests.Pages
{
    [TestFixture]
    public class ContactUsTests
    {
        private BunitContext _ctx = null!;
        private Mock<IContactUsService> _contactUsServiceMock = null!;
        private Mock<IMessageComponent> _messageComponentMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _contactUsServiceMock = new Mock<IContactUsService>(MockBehavior.Strict);
            _messageComponentMock = new Mock<IMessageComponent>(MockBehavior.Loose);

            _ctx.Services.AddSingleton(_contactUsServiceMock.Object);
            _ctx.Services.AddSingleton(_messageComponentMock.Object);
            _ctx.Services.AddLogging();
        }

        [TearDown]
        public void TearDown() => _ctx.Dispose();

        private IRenderedComponent<ContactUs> RenderComponent() =>
            _ctx.Render<ContactUs>(ps =>
                ps.AddCascadingValue("MessageComponent", _messageComponentMock.Object));

        private static System.Reflection.MethodInfo GetMethod(string name) =>
            typeof(ContactUs).GetMethod(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

        [Test]
        public async Task OnSubmitAsync_Success_ShowsInfoMessage()
        {
            var result = new CommandResponse { Succeeded = true, Message = "Your message has been sent." };

            _contactUsServiceMock
                .Setup(s => s.SendAsync(It.IsAny<ContactUsModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            var cut = RenderComponent();
            var method = GetMethod("OnSubmitAsync");

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            _messageComponentMock.Verify(
                m => m.ShowInfoAsync("Your message has been sent.", It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task OnSubmitAsync_Success_ClearsModel()
        {
            var result = new CommandResponse { Succeeded = true, Message = "Sent" };

            _contactUsServiceMock
                .Setup(s => s.SendAsync(It.IsAny<ContactUsModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            var cut = RenderComponent();
            cut.Instance.Model.Name = "Jane";
            cut.Instance.Model.EmailAddress = "jane@example.com";
            cut.Instance.Model.Subject = "Hi";
            cut.Instance.Model.Message = "Hello";

            var method = GetMethod("OnSubmitAsync");
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method.Invoke(cut.Instance, [])!;
                await task;
            });

            using (Assert.EnterMultipleScope())
            {
                Assert.That(cut.Instance.Model.Name, Is.EqualTo(string.Empty));
                Assert.That(cut.Instance.Model.EmailAddress, Is.EqualTo(string.Empty));
                Assert.That(cut.Instance.Model.Subject, Is.EqualTo(string.Empty));
                Assert.That(cut.Instance.Model.Message, Is.EqualTo(string.Empty));
            }
        }

        [Test]
        public async Task OnSubmitAsync_Failure_ShowsErrorMessage()
        {
            var result = new CommandResponse { Succeeded = false, Message = "Something went wrong." };

            _contactUsServiceMock
                .Setup(s => s.SendAsync(It.IsAny<ContactUsModel>(), It.IsAny<CancellationToken>()))
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
        public async Task OnSubmitAsync_NullResult_ShowsErrorMessage()
        {
            _contactUsServiceMock
                .Setup(s => s.SendAsync(It.IsAny<ContactUsModel>(), It.IsAny<CancellationToken>()))
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
    }
}
