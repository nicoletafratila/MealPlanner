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
    public class ForgotPasswordViewModelTests
    {
        private Mock<IAuthenticationService> _authServiceMock = null!;
        private ForgotPasswordViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _authServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);
            _viewModel = new ForgotPasswordViewModel(_authServiceMock.Object);
        }

        [Test]
        public async Task SendAsync_EmptyEmail_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.EmailAddress = string.Empty;

            await _viewModel.SendCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(IdentitySharedMessages.EmailAddressRequired));
            _authServiceMock.Verify(x => x.ForgotPasswordAsync(It.IsAny<ForgotPasswordModel>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task SendAsync_Success_SetsSuccessMessageAndCallsService()
        {
            _viewModel.Model.EmailAddress = "john@doe.com";

            _authServiceMock
                .Setup(x => x.ForgotPasswordAsync(_viewModel.Model, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            await _viewModel.SendCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.SuccessMessage, Is.EqualTo(MealPlannerSharedMessages.ForgotPasswordEmailSent));
                Assert.That(_viewModel.ErrorMessage, Is.Null);
                Assert.That(_viewModel.IsBusy, Is.False);
            }
            _authServiceMock.Verify(x => x.ForgotPasswordAsync(_viewModel.Model, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task SendAsync_Failure_SetsErrorMessage()
        {
            _viewModel.Model.EmailAddress = "john@doe.com";

            _authServiceMock
                .Setup(x => x.ForgotPasswordAsync(_viewModel.Model, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Failed("No account found for this email"));

            await _viewModel.SendCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo("No account found for this email"));
        }

        // GoBackAsync calls Shell.Current.GoToAsync directly with no surrounding try/catch, so it
        // is not unit-testable in this host and is intentionally skipped.
    }
}
