using Common.Http;
using Common.Models;
using Identity.Services.Http;
using Identity.Shared.Models;
using Identity.Shared.Resources;
using MealPlanner.Services.Http;
using MealPlanner.UI.Mobile.Services;
using MealPlanner.UI.Mobile.ViewModels;
using MealPlanner.UI.Mobile.ViewModels.Identity;
using Moq;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.Identity
{
    [TestFixture]
    public class LoginViewModelTests
    {
        private Mock<IAuthenticationService> _authServiceMock = null!;
        private Mock<IMealPlanService> _mealPlanServiceMock = null!;
        private Mock<ITokenProvider> _tokenProviderMock = null!;
        private AppShellViewModel _appShellViewModel = null!;
        private LoginViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _authServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);
            _mealPlanServiceMock = new Mock<IMealPlanService>(MockBehavior.Loose);
            _tokenProviderMock = new Mock<ITokenProvider>(MockBehavior.Loose);

            var authStateService = new AuthenticationStateService(_tokenProviderMock.Object);
            _appShellViewModel = new AppShellViewModel(_mealPlanServiceMock.Object, authStateService);
            _viewModel = new LoginViewModel(_authServiceMock.Object, _appShellViewModel);
        }

        [Test]
        public async Task LoginAsync_EmptyUsername_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Username = string.Empty;

            await _viewModel.LoginCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo(IdentitySharedMessages.UsernameRequired));
                Assert.That(_viewModel.SuccessMessage, Is.Null);
            }

            _authServiceMock.Verify(
                x => x.LoginAsync(It.IsAny<LoginModel>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task LoginAsync_WhitespaceUsername_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Username = "   ";

            await _viewModel.LoginCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(IdentitySharedMessages.UsernameRequired));
            _authServiceMock.Verify(
                x => x.LoginAsync(It.IsAny<LoginModel>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task LoginAsync_Success_CallsAuthServiceWithCorrectCredentials()
        {
            _viewModel.Username = "johndoe";
            _viewModel.Password = "s3cret";
            _viewModel.RememberMe = true;

            _authServiceMock
                .Setup(x => x.LoginAsync(
                    It.Is<LoginModel>(m => m.Username == "johndoe" && m.Password == "s3cret" && m.RememberLogin),
                    CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            // Shell.Current is null in the unit test host; the navigation call inside the try/catch
            // throws a NullReferenceException that gets swallowed by the catch block, so we only
            // assert the service interaction here, not ErrorMessage/SuccessMessage.
            await _viewModel.LoginCommand.ExecuteAsync(null);

            _authServiceMock.Verify(
                x => x.LoginAsync(
                    It.Is<LoginModel>(m => m.Username == "johndoe" && m.Password == "s3cret" && m.RememberLogin),
                    CancellationToken.None),
                Times.Once);
        }

        [Test]
        public async Task LoginAsync_Failure_SetsErrorMessage()
        {
            _viewModel.Username = "johndoe";
            _viewModel.Password = "wrong";

            _authServiceMock
                .Setup(x => x.LoginAsync(It.IsAny<LoginModel>(), CancellationToken.None))
                .ReturnsAsync(CommandResponse.Failed("Invalid credentials"));

            await _viewModel.LoginCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("Invalid credentials"));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        // GoToRegisterAsync/GoToForgotPasswordAsync call Shell.Current.GoToAsync directly with no
        // surrounding try/catch, so they are not unit-testable in this host and are intentionally skipped.
    }
}
