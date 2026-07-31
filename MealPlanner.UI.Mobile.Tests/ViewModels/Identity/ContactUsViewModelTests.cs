using Common.Models;
using Identity.Services.Http;
using Identity.Shared.Models;
using Identity.Shared.Resources;
using MealPlanner.UI.Mobile.ViewModels.Identity;
using Moq;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.Identity
{
    [TestFixture]
    public class ContactUsViewModelTests
    {
        // ContactUsPage's generated resource accessor class is `internal` to MealPlanner.UI.Mobile
        // (it's a UI executable, so its resx uses ResXFileCodeGenerator), so it isn't visible from
        // this test assembly. The expected literal values below are copied verbatim from
        // MealPlanner.UI.Mobile/Pages/Identity/Resources/ContactUsPage.resx.
        private const string NameRequiredMessage = "Name is required.";
        private const string SubjectRequiredMessage = "Subject is required.";
        private const string MessageRequiredMessage = "Message is required.";

        private Mock<IContactUsService> _contactUsServiceMock = null!;
        private ContactUsViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _contactUsServiceMock = new Mock<IContactUsService>(MockBehavior.Strict);
            _viewModel = new ContactUsViewModel(_contactUsServiceMock.Object);
        }

        [Test]
        public async Task SendAsync_EmptyName_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Name = string.Empty;

            await _viewModel.SendCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(NameRequiredMessage));
            _contactUsServiceMock.Verify(x => x.SendAsync(It.IsAny<ContactUsModel>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task SendAsync_EmptyEmail_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Name = "John Doe";
            _viewModel.Model.EmailAddress = string.Empty;

            await _viewModel.SendCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(IdentitySharedMessages.EmailAddressRequired));
            _contactUsServiceMock.Verify(x => x.SendAsync(It.IsAny<ContactUsModel>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task SendAsync_EmptySubject_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Name = "John Doe";
            _viewModel.Model.EmailAddress = "john@doe.com";
            _viewModel.Model.Subject = string.Empty;

            await _viewModel.SendCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(SubjectRequiredMessage));
            _contactUsServiceMock.Verify(x => x.SendAsync(It.IsAny<ContactUsModel>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task SendAsync_EmptyMessage_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Name = "John Doe";
            _viewModel.Model.EmailAddress = "john@doe.com";
            _viewModel.Model.Subject = "Hello";
            _viewModel.Model.Message = string.Empty;

            await _viewModel.SendCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(MessageRequiredMessage));
            _contactUsServiceMock.Verify(x => x.SendAsync(It.IsAny<ContactUsModel>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task SendAsync_Success_ClearsModelAndSetsSuccessMessage()
        {
            var model = _viewModel.Model;
            model.Name = "John Doe";
            model.EmailAddress = "john@doe.com";
            model.Subject = "Hello";
            model.Message = "Test message";

            _contactUsServiceMock
                .Setup(x => x.SendAsync(model, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success("Thanks for reaching out"));

            await _viewModel.SendCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.SuccessMessage, Is.EqualTo("Thanks for reaching out"));
                Assert.That(_viewModel.ErrorMessage, Is.Null);
                Assert.That(model.Name, Is.Empty);
                Assert.That(model.EmailAddress, Is.Empty);
                Assert.That(model.Subject, Is.Empty);
                Assert.That(model.Message, Is.Empty);
            }
            _contactUsServiceMock.Verify(x => x.SendAsync(model, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task SendAsync_Failure_SetsErrorMessageAndKeepsModel()
        {
            var model = _viewModel.Model;
            model.Name = "John Doe";
            model.EmailAddress = "john@doe.com";
            model.Subject = "Hello";
            model.Message = "Test message";

            _contactUsServiceMock
                .Setup(x => x.SendAsync(model, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Failed("Could not send message"));

            await _viewModel.SendCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("Could not send message"));
                Assert.That(model.Name, Is.EqualTo("John Doe"));
            }
        }
    }
}
