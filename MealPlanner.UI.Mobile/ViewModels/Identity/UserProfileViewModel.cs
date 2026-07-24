using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Identity.Services.Http;
using Identity.Shared.Models;
using Identity.Shared.Resources;
using MealPlanner.Shared.Resources;
using MealPlanner.UI.Mobile.Pages.Identity.Resources;
using MealPlanner.UI.Mobile.Services;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    [QueryProperty(nameof(Username), "username")]
    public partial class UserProfileViewModel(ApplicationUserService userService, MobileAuthStateService authState, AuthenticationService authService, SecureStorageTokenProvider tokenProvider) : BaseViewModel
    {
        [ObservableProperty]
        private ApplicationUserEditModel? _model;

        [ObservableProperty]
        private string? _username;

        [ObservableProperty]
        private bool _isAdmin;

        [ObservableProperty]
        private bool _isOwnProfile = true;

        [ObservableProperty]
        private ImageSource? _profileImage;

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ClearMessages();
            try
            {
                var currentUser = await authState.GetCurrentUserAsync();
                IsAdmin = currentUser.IsInRole("admin");

                var currentUserName = await authState.GetUserNameAsync();
                var targetUsername = string.IsNullOrWhiteSpace(Username) ? currentUserName : Username;
                IsOwnProfile = string.IsNullOrWhiteSpace(Username) ||
                    string.Equals(targetUsername, currentUserName, StringComparison.OrdinalIgnoreCase);

                if (targetUsername is not null)
                {
                    Model = await userService.GetEditAsync(targetUsername);
                    ProfileImage = string.IsNullOrWhiteSpace(Model?.ProfilePictureUrl)
                        ? null
                        : ImageSource.FromUri(new Uri(Model.ProfilePictureUrl));
                }
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task SaveAsync()
        {
            if (IsBusy || Model is null) return;
            ClearMessages();

            if (string.IsNullOrWhiteSpace(Model.Username))
            {
                SetError(IdentitySharedMessages.UsernameRequired);
                return;
            }

            if (string.IsNullOrWhiteSpace(Model.EmailAddress))
            {
                SetError(IdentitySharedMessages.EmailAddressRequired);
                return;
            }

            IsBusy = true;
            try
            {
                var result = await userService.UpdateAsync(Model);
                if (result?.Succeeded == true)
                {
                    SetSuccess(MealPlannerSharedMessages.ProfileUpdatedSuccess);
                    if (!IsOwnProfile)
                    {
                        await Task.Delay(1000);
                        await Shell.Current.GoToAsync("..");
                    }
                }
                else SetError(result?.Message);
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task PickProfilePictureAsync()
        {
            if (Model is null) return;
            try
            {
                var results = await MediaPicker.Default.PickPhotosAsync();
                var result = results?.FirstOrDefault();
                if (result is null) return;
                await using var stream = await result.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                Model.ProfilePicture = ms.ToArray();
                ProfileImage = ImageSource.FromStream(() => new MemoryStream(Model.ProfilePicture));
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task UnlockAsync()
        {
            if (IsBusy || Model is null || string.IsNullOrWhiteSpace(Model.UserId)) return;
            IsBusy = true;
            ClearMessages();
            try
            {
                var result = await userService.UnlockAsync(Model.UserId);
                if (result?.Succeeded == true)
                {
                    Model.IsLockedOut = false;
                    OnPropertyChanged(nameof(Model));
                    SetSuccess(UserProfilePage.UnlockSucceeded);
                }
                else SetError(result?.Message);
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private Task ChangePasswordAsync() => Shell.Current.GoToAsync("ChangePassword");

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task LogoutAsync()
        {
            await authService.LogoutAsync();
            tokenProvider.ClearCredentials();
            await Shell.Current.GoToAsync("//Login");
        }
    }
}
