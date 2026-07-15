using CommunityToolkit.Mvvm.Input;
using Identity.Services.Http;
using Identity.Shared.Models;
using MealPlanner.Shared.Resources;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    public partial class ForgotPasswordViewModel(AuthenticationService authService) : BaseViewModel
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
                var result = await authService.ForgotPasswordAsync(Model);
                if (result?.Succeeded == true)
                    SetSuccess(MealPlannerSharedMessages.ForgotPasswordEmailSent);
                else
                    SetError(result?.Message);
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private Task GoBackAsync() => Shell.Current.GoToAsync("..");
    }
}
