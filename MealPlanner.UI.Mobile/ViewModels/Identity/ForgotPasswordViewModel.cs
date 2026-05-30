using CommunityToolkit.Mvvm.Input;
using Identity.Services.Http;
using Identity.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    public partial class ForgotPasswordViewModel(IdentityService authService) : BaseViewModel
    {
        public ForgotPasswordModel Model { get; } = new();

        [RelayCommand]
        private async Task SendAsync()
        {
            if (IsBusy) return;
            ClearMessages();
            IsBusy = true;
            try
            {
                var (success, error) = await authService.ForgotPasswordAsync(Model);
                if (success)
                    SetSuccess("If the email exists, a reset link has been sent.");
                else
                    SetError(error);
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private Task GoBackAsync() => Shell.Current.GoToAsync("..");
    }
}
