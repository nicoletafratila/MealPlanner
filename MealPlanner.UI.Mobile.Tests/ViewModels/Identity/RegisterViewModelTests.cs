using Common.Models;
using Identity.Services.Http;
using Identity.Shared.Models;
using Identity.Shared.Resources;
using MealPlanner.Shared.Resources;
using MealPlanner.UI.Mobile.ViewModels.Identity;
using Moq;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.Identity
{
    [TestFixture]
    public class RegisterViewModelTests
    {
        private Mock<IAuthenticationService> _authServiceMock = null!;
        private RegisterViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _authServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);
            _viewModel = new RegisterViewModel(_authServiceMock.Object);
        }

        [Test]
        public async Task RegisterAsync_EmptyUsername_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Username = string.Empty;

            await _viewModel.RegisterCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(IdentitySharedMessages.UsernameRequired));
            _authServiceMock.Verify(x => x.RegisterAsync(It.IsAny<RegistrationModel>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task RegisterAsync_EmptyEmail_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Username = "johndoe";
            _viewModel.Model.EmailAddress = string.Empty;

            await _viewModel.RegisterCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(IdentitySharedMessages.EmailAddressRequired));
            _authServiceMock.Verify(x => x.RegisterAsync(It.IsAny<RegistrationModel>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task RegisterAsync_EmptyConfirmPassword_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Username = "johndoe";
            _viewModel.Model.EmailAddress = "john@doe.com";
            _viewModel.Model.ConfirmPassword = string.Empty;

            await _viewModel.RegisterCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(IdentitySharedMessages.ConfirmPasswordRequired));
            _authServiceMock.Verify(x => x.RegisterAsync(It.IsAny<RegistrationModel>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task RegisterAsync_PasswordMismatch_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Username = "johndoe";
            _viewModel.Model.EmailAddress = "john@doe.com";
            _viewModel.Model.Password = "password1";
            _viewModel.Model.ConfirmPassword = "password2";

            await _viewModel.RegisterCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(MealPlannerSharedMessages.PasswordsDoNotMatch));
            _authServiceMock.Verify(x => x.RegisterAsync(It.IsAny<RegistrationModel>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task RegisterAsync_PrivacyPolicyNotAccepted_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Username = "johndoe";
            _viewModel.Model.EmailAddress = "john@doe.com";
            _viewModel.Model.Password = "password1";
            _viewModel.Model.ConfirmPassword = "password1";
            _viewModel.Model.AcceptedPrivacyPolicy = false;

            await _viewModel.RegisterCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(IdentitySharedMessages.PrivacyPolicyRequired));
            _authServiceMock.Verify(x => x.RegisterAsync(It.IsAny<RegistrationModel>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task RegisterAsync_Success_CallsRegisterAsync()
        {
            _viewModel.Model.Username = "johndoe";
            _viewModel.Model.EmailAddress = "john@doe.com";
            _viewModel.Model.Password = "password1";
            _viewModel.Model.ConfirmPassword = "password1";
            _viewModel.Model.AcceptedPrivacyPolicy = true;

            _authServiceMock
                .Setup(x => x.RegisterAsync(_viewModel.Model, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            // Success path awaits Task.Delay(1500) and then calls Shell.Current.GoToAsync inside a
            // try/catch, so the NRE gets swallowed; only the service call is verified here.
            await _viewModel.RegisterCommand.ExecuteAsync(null);

            _authServiceMock.Verify(x => x.RegisterAsync(_viewModel.Model, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task RegisterAsync_Failure_SetsErrorMessage()
        {
            _viewModel.Model.Username = "johndoe";
            _viewModel.Model.EmailAddress = "john@doe.com";
            _viewModel.Model.Password = "password1";
            _viewModel.Model.ConfirmPassword = "password1";
            _viewModel.Model.AcceptedPrivacyPolicy = true;

            _authServiceMock
                .Setup(x => x.RegisterAsync(_viewModel.Model, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Failed("Username already taken"));

            await _viewModel.RegisterCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo("Username already taken"));
        }

        // GoBackAsync/OpenPrivacyPolicyAsync call Shell.Current.GoToAsync directly with no
        // surrounding try/catch, so they are not unit-testable in this host and are intentionally skipped.
    }
}
