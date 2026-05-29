using CommunityToolkit.Mvvm.Input;
using Identity.Services.Core.Http;
using Identity.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    public partial class ResetPasswordViewModel(IdentityService authService) : BaseViewModel
    {
        public ResetPasswordModel Model { get; } = new();

        [RelayCommand]
        private async Task ResetAsync()
        {
            if (IsBusy) return;
            ClearMessages();
            if (Model.NewPassword != Model.ConfirmPassword)
            {
                SetError("Passwords do not match.");
                return;
            }
            IsBusy = true;
            try
            {
                var (success, error) = await authService.ResetPasswordAsync(Model);
                if (success)
                {
                    SetSuccess("Password reset successfully. Please log in.");
                    await Task.Delay(1500);
                    await Shell.Current.GoToAsync("..");
                }
                else SetError(error);
            }
            finally { IsBusy = false; }
        }
    }
}
