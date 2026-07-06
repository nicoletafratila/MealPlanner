using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Identity.Services.Http;
using Identity.Shared.Models;
using MealPlanner.UI.Mobile.Services;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    public partial class UserProfileViewModel(ApplicationUserService userService, MobileAuthStateService authState, AuthenticationService authService) : BaseViewModel
    {
        [ObservableProperty] private ApplicationUserEditModel? _model;

        [RelayCommand]
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

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (IsBusy || Model is null) return;
            IsBusy = true;
            ClearMessages();
            try
            {
                var result = await userService.UpdateAsync(Model);
                if (result?.Succeeded == true) SetSuccess("Profile updated.");
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

        [RelayCommand]
        private async Task LogoutAsync()
        {
            await authService.LogoutAsync();
            await Shell.Current.GoToAsync("//Login");
        }
    }
}
