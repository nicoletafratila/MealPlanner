using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Common.Http;
using Common.Models;
using Identity.Services.Http;
using Identity.Shared.Models;
using Identity.Shared.Resources;
using MealPlanner.Shared.Resources;
using MealPlanner.UI.Mobile.Services;
using MealPlanner.UI.Mobile.ViewModels.Identity;
using Moq;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.Identity
{
    [TestFixture]
    public class UserProfileViewModelTests
    {
        // UserProfilePage's generated resource accessor class is `internal` to
        // MealPlanner.UI.Mobile (it's a UI executable, so its resx uses ResXFileCodeGenerator),
        // so it isn't visible from this test assembly. The literal value below is copied verbatim
        // from MealPlanner.UI.Mobile/Pages/Identity/Resources/UserProfilePage.resx.
        private const string UnlockSucceededMessage = "User has been unlocked successfully";

        private Mock<IApplicationUserService> _userServiceMock = null!;
        private Mock<IAuthenticationService> _authServiceMock = null!;
        private Mock<ITokenProvider> _tokenProviderMock = null!;
        private AuthenticationStateService _authState = null!;
        private UserProfileViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _userServiceMock = new Mock<IApplicationUserService>(MockBehavior.Strict);
            _authServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);
            _tokenProviderMock = new Mock<ITokenProvider>(MockBehavior.Strict);

            _authState = new AuthenticationStateService(_tokenProviderMock.Object);
            _viewModel = new UserProfileViewModel(_userServiceMock.Object, _authState, _authServiceMock.Object);
        }

        /// <summary>
        /// Hand-builds a JWT with an unsigned/empty signature segment whose payload contains the
        /// given claims, base64-encoded with the standard (non URL-safe) alphabet, matching what
        /// AuthenticationStateService.ParseClaims expects (it only re-pads, it never remaps '-'/'_').
        /// </summary>
        private static string BuildJwt(IDictionary<string, object> claims)
        {
            var header = Base64Encode("{\"alg\":\"none\",\"typ\":\"JWT\"}");
            var payload = Base64Encode(JsonSerializer.Serialize(claims));
            return $"{header}.{payload}.";
        }

        private static string Base64Encode(string value) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(value)).TrimEnd('=');

        [Test]
        public async Task LoadAsync_OwnProfile_LoadsCurrentUserModel()
        {
            var jwt = BuildJwt(new Dictionary<string, object>
            {
                ["sub"] = "user-1",
                ["name"] = "alice"
            });
            _tokenProviderMock.Setup(x => x.GetTokenAsync(CancellationToken.None)).ReturnsAsync(jwt);

            var editModel = new ApplicationUserEditModel { Username = "alice", EmailAddress = "alice@test.com", IsLockedOut = false };
            _userServiceMock.Setup(x => x.GetEditAsync("alice", CancellationToken.None)).ReturnsAsync(editModel);

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.IsAdmin, Is.False);
                Assert.That(_viewModel.IsOwnProfile, Is.True);
                Assert.That(_viewModel.Model, Is.SameAs(editModel));
                Assert.That(_viewModel.IsLockedOut, Is.False);
                Assert.That(_viewModel.ProfileImage, Is.Null);
                Assert.That(_viewModel.ErrorMessage, Is.Null);
            }
        }

        [Test]
        public async Task LoadAsync_AdminUser_SetsIsAdminTrue()
        {
            var jwt = BuildJwt(new Dictionary<string, object>
            {
                ["sub"] = "user-1",
                ["name"] = "alice",
                [ClaimTypes.Role] = "admin"
            });
            _tokenProviderMock.Setup(x => x.GetTokenAsync(CancellationToken.None)).ReturnsAsync(jwt);

            var editModel = new ApplicationUserEditModel { Username = "alice", EmailAddress = "alice@test.com" };
            _userServiceMock.Setup(x => x.GetEditAsync("alice", CancellationToken.None)).ReturnsAsync(editModel);

            await _viewModel.LoadCommand.ExecuteAsync(null);

            Assert.That(_viewModel.IsAdmin, Is.True);
        }

        [Test]
        public async Task LoadAsync_OtherUsersProfile_SetsIsOwnProfileFalse()
        {
            var jwt = BuildJwt(new Dictionary<string, object>
            {
                ["sub"] = "user-1",
                ["name"] = "alice"
            });
            _tokenProviderMock.Setup(x => x.GetTokenAsync(CancellationToken.None)).ReturnsAsync(jwt);

            _viewModel.Username = "bob";

            var editModel = new ApplicationUserEditModel { Username = "bob", EmailAddress = "bob@test.com" };
            _userServiceMock.Setup(x => x.GetEditAsync("bob", CancellationToken.None)).ReturnsAsync(editModel);

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.IsOwnProfile, Is.False);
                Assert.That(_viewModel.Model, Is.SameAs(editModel));
            }
        }

        [Test]
        public async Task LoadAsync_ModelHasProfilePictureUrl_LoadsModelWithUrl()
        {
            // Note: converting the URL into an ImageSource (via ImageSource.FromUri) isn't
            // reliably testable in this headless MAUI test host (mirrors a known limitation
            // already present in ProductEditViewModelTests), so this only asserts that the
            // underlying model data was loaded correctly rather than the derived ProfileImage.
            var jwt = BuildJwt(new Dictionary<string, object>
            {
                ["sub"] = "user-1",
                ["name"] = "alice"
            });
            _tokenProviderMock.Setup(x => x.GetTokenAsync(CancellationToken.None)).ReturnsAsync(jwt);

            var editModel = new ApplicationUserEditModel { Username = "alice", EmailAddress = "alice@test.com", ProfilePictureUrl = "https://example.com/pic.jpg" };
            _userServiceMock.Setup(x => x.GetEditAsync("alice", CancellationToken.None)).ReturnsAsync(editModel);

            await _viewModel.LoadCommand.ExecuteAsync(null);

            Assert.That(_viewModel.Model?.ProfilePictureUrl, Is.EqualTo("https://example.com/pic.jpg"));
        }

        [Test]
        public async Task SaveAsync_ModelIsNull_DoesNotCallService()
        {
            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.Null);
                Assert.That(_viewModel.Model, Is.Null);
            }
            _userServiceMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUserEditModel>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task SaveAsync_EmptyUsername_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model = new ApplicationUserEditModel { Username = string.Empty, EmailAddress = "alice@test.com" };

            await _viewModel.SaveCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(IdentitySharedMessages.UsernameRequired));
            _userServiceMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUserEditModel>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task SaveAsync_EmptyEmail_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model = new ApplicationUserEditModel { Username = "alice", EmailAddress = string.Empty };

            await _viewModel.SaveCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo(IdentitySharedMessages.EmailAddressRequired));
            _userServiceMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUserEditModel>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task SaveAsync_Success_OwnProfile_CallsUpdateAsync()
        {
            var model = new ApplicationUserEditModel { Username = "alice", EmailAddress = "alice@test.com" };
            _viewModel.Model = model;
            // IsOwnProfile defaults to true, so no Shell.Current navigation happens on success.

            _userServiceMock.Setup(x => x.UpdateAsync(model, CancellationToken.None)).ReturnsAsync(CommandResponse.Success());

            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.SuccessMessage, Is.EqualTo(MealPlannerSharedMessages.ProfileUpdatedSuccess));
                Assert.That(_viewModel.ErrorMessage, Is.Null);
            }
            _userServiceMock.Verify(x => x.UpdateAsync(model, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task SaveAsync_Failure_SetsErrorMessage()
        {
            var model = new ApplicationUserEditModel { Username = "alice", EmailAddress = "alice@test.com" };
            _viewModel.Model = model;

            _userServiceMock.Setup(x => x.UpdateAsync(model, CancellationToken.None)).ReturnsAsync(CommandResponse.Failed("Username already in use"));

            await _viewModel.SaveCommand.ExecuteAsync(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo("Username already in use"));
        }

        [Test]
        public async Task UnlockAsync_ModelUserIdEmpty_DoesNotCallService()
        {
            _viewModel.Model = new ApplicationUserEditModel { Username = "alice", EmailAddress = "alice@test.com", UserId = string.Empty };

            await _viewModel.UnlockCommand.ExecuteAsync(null);

            _userServiceMock.Verify(x => x.UnlockAsync(It.IsAny<string>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task UnlockAsync_Success_SetsIsLockedOutFalseAndSuccessMessage()
        {
            var model = new ApplicationUserEditModel { Username = "alice", EmailAddress = "alice@test.com", UserId = "user-1", IsLockedOut = true };
            _viewModel.Model = model;
            _viewModel.IsLockedOut = true;

            _userServiceMock.Setup(x => x.UnlockAsync("user-1", CancellationToken.None)).ReturnsAsync(CommandResponse.Success());

            await _viewModel.UnlockCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.IsLockedOut, Is.False);
                Assert.That(model.IsLockedOut, Is.False);
                Assert.That(_viewModel.SuccessMessage, Is.EqualTo(UnlockSucceededMessage));
            }
        }

        [Test]
        public async Task UnlockAsync_Failure_SetsErrorMessageAndKeepsLockedOut()
        {
            var model = new ApplicationUserEditModel { Username = "alice", EmailAddress = "alice@test.com", UserId = "user-1", IsLockedOut = true };
            _viewModel.Model = model;
            _viewModel.IsLockedOut = true;

            _userServiceMock.Setup(x => x.UnlockAsync("user-1", CancellationToken.None)).ReturnsAsync(CommandResponse.Failed("Unlock failed"));

            await _viewModel.UnlockCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("Unlock failed"));
                Assert.That(_viewModel.IsLockedOut, Is.True);
            }
        }

        // PickProfilePictureAsync uses MediaPicker.Default, which is not available/unit-testable
        // in this host, so it is intentionally skipped.

        // LogoutAsync and ChangePasswordAsync call Shell.Current directly with no surrounding
        // try/catch (LogoutAsync unconditionally awaits authService.LogoutAsync() and then
        // Shell.Current.GoToAsync in sequence, with no guard clause to stop before the navigation
        // call), so invoking them would throw an unobserved NullReferenceException in this host.
        // They are intentionally skipped rather than exercised.
    }
}
