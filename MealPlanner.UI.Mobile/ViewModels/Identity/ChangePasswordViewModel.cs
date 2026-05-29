using CommunityToolkit.Mvvm.Input;
using Identity.Services.Core.Http;
using Identity.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    public partial class ChangePasswordViewModel(IdentityService authService) : BaseViewModel
    {
        public ChangePasswordModel Model { get; } = new();

        [RelayCommand]
        private async Task ChangeAsync()
        {
            if (IsBusy) return;
            ClearMessages();
            if (Model.NewPassword != Model.ConfirmPassword)
            {
                SetError("New passwords do not match.");
                return;
            }
            IsBusy = true;
            try
            {
                var (success, error) = await authService.ChangePasswordAsync(Model);
                if (success)
                {
                    SetSuccess("Password changed successfully.");
                    await Task.Delay(1500);
                    await Shell.Current.GoToAsync("..");
                }
                else SetError(error);
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private Task GoBackAsync() => Shell.Current.GoToAsync("..");
    }
}
