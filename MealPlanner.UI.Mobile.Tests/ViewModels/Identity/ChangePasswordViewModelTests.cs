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
    public class ChangePasswordViewModelTests
    {
        private Mock<IAuthenticationService> _authServiceMock = null!;
        private ChangePasswordViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _authServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);
            _viewModel = new ChangePasswordViewModel(_authServiceMock.Object);
        }

        [Test]
        public async Task ChangeAsync_EmptyCurrentPassword_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.CurrentPassword = string.Empty;

            await _viewModel.ChangeCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(IdentitySharedMessages.CurrentPasswordRequired));
            _authServiceMock.Verify(x => x.ChangePasswordAsync(It.IsAny<ChangePasswordModel>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task ChangeAsync_EmptyNewPassword_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.CurrentPassword = "current1";
            _viewModel.Model.NewPassword = string.Empty;

            await _viewModel.ChangeCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(IdentitySharedMessages.NewPasswordRequired));
            _authServiceMock.Verify(x => x.ChangePasswordAsync(It.IsAny<ChangePasswordModel>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task ChangeAsync_EmptyConfirmPassword_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.CurrentPassword = "current1";
            _viewModel.Model.NewPassword = "newpass1";
            _viewModel.Model.ConfirmPassword = string.Empty;

            await _viewModel.ChangeCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(IdentitySharedMessages.ConfirmPasswordRequired));
            _authServiceMock.Verify(x => x.ChangePasswordAsync(It.IsAny<ChangePasswordModel>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task ChangeAsync_PasswordMismatch_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.CurrentPassword = "current1";
            _viewModel.Model.NewPassword = "newpass1";
            _viewModel.Model.ConfirmPassword = "newpass2";

            await _viewModel.ChangeCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(MealPlannerSharedMessages.NewPasswordsDoNotMatch));
            _authServiceMock.Verify(x => x.ChangePasswordAsync(It.IsAny<ChangePasswordModel>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task ChangeAsync_Success_CallsChangePasswordAsync()
        {
            _viewModel.Model.CurrentPassword = "current1";
            _viewModel.Model.NewPassword = "newpass1";
            _viewModel.Model.ConfirmPassword = "newpass1";

            _authServiceMock
                .Setup(x => x.ChangePasswordAsync(_viewModel.Model, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            // Success path awaits Task.Delay(1500) and then calls Shell.Current.GoToAsync inside a
            // try/catch, so the NRE gets swallowed; only the service call is verified here.
            await _viewModel.ChangeCommand.ExecuteAsync(null);

            _authServiceMock.Verify(x => x.ChangePasswordAsync(_viewModel.Model, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task ChangeAsync_Failure_SetsErrorMessage()
        {
            _viewModel.Model.CurrentPassword = "current1";
            _viewModel.Model.NewPassword = "newpass1";
            _viewModel.Model.ConfirmPassword = "newpass1";

            _authServiceMock
                .Setup(x => x.ChangePasswordAsync(_viewModel.Model, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Failed("Current password is incorrect"));

            await _viewModel.ChangeCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo("Current password is incorrect"));
        }

        // GoBackAsync calls Shell.Current.GoToAsync directly with no surrounding try/catch, so it
        // is not unit-testable in this host and is intentionally skipped.
    }
}
