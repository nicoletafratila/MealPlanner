using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Identity.Services.Http;
using Identity.Shared.Models;
using Identity.Shared.Resources;
using MealPlanner.Shared.Resources;
using MealPlanner.UI.Mobile.Services;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    public partial class UserProfileViewModel(ApplicationUserService userService, MobileAuthStateService authState, AuthenticationService authService, SecureStorageTokenProvider tokenProvider) : BaseViewModel
    {
        [ObservableProperty]
        private ApplicationUserEditModel? _model;

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ClearMessages();
            try
            {
                var userName = await authState.GetUserNameAsync();
                if (userName is not null)
                    Model = await userService.GetEditAsync(userName);
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
                if (result?.Succeeded == true) SetSuccess(MealPlannerSharedMessages.ProfileUpdatedSuccess);
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
