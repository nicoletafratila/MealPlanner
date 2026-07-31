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
    public class ResetPasswordViewModelTests
    {
        private Mock<IAuthenticationService> _authServiceMock = null!;
        private ResetPasswordViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _authServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);
            _viewModel = new ResetPasswordViewModel(_authServiceMock.Object);
        }

        [Test]
        public void ApplyQueryAttributes_PopulatesModelFromQuery_UrlDecoded()
        {
            var query = new Dictionary<string, object>
            {
                ["userId"] = Uri.EscapeDataString("user name/1"),
                ["token"] = Uri.EscapeDataString("abc+def/ghi=?token")
            };

            _viewModel.ApplyQueryAttributes(query);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Model.UserId, Is.EqualTo("user name/1"));
                Assert.That(_viewModel.Model.Token, Is.EqualTo("abc+def/ghi=?token"));
            }
        }

        [Test]
        public async Task ResetAsync_EmptyNewPassword_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.NewPassword = string.Empty;

            await _viewModel.ResetCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(IdentitySharedMessages.NewPasswordRequired));
            _authServiceMock.Verify(x => x.ResetPasswordAsync(It.IsAny<ResetPasswordModel>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task ResetAsync_EmptyConfirmPassword_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.NewPassword = "newpass1";
            _viewModel.Model.ConfirmPassword = string.Empty;

            await _viewModel.ResetCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(IdentitySharedMessages.ConfirmPasswordRequired));
            _authServiceMock.Verify(x => x.ResetPasswordAsync(It.IsAny<ResetPasswordModel>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task ResetAsync_PasswordMismatch_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.NewPassword = "newpass1";
            _viewModel.Model.ConfirmPassword = "newpass2";

            await _viewModel.ResetCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(MealPlannerSharedMessages.PasswordsDoNotMatch));
            _authServiceMock.Verify(x => x.ResetPasswordAsync(It.IsAny<ResetPasswordModel>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task ResetAsync_Success_CallsResetPasswordAsync()
        {
            _viewModel.Model.NewPassword = "newpass1";
            _viewModel.Model.ConfirmPassword = "newpass1";

            _authServiceMock
                .Setup(x => x.ResetPasswordAsync(_viewModel.Model, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            // Success path awaits Task.Delay(1500) and then calls Shell.Current.GoToAsync inside a
            // try/catch, so the NRE gets swallowed; only the service call is verified here.
            await _viewModel.ResetCommand.ExecuteAsync(null);

            _authServiceMock.Verify(x => x.ResetPasswordAsync(_viewModel.Model, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task ResetAsync_Failure_SetsErrorMessage()
        {
            _viewModel.Model.NewPassword = "newpass1";
            _viewModel.Model.ConfirmPassword = "newpass1";

            _authServiceMock
                .Setup(x => x.ResetPasswordAsync(_viewModel.Model, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Failed("Invalid or expired token"));

            await _viewModel.ResetCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo("Invalid or expired token"));
        }
    }
}
