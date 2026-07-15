using CommunityToolkit.Mvvm.Input;
using Identity.Services.Http;
using Identity.Shared.Models;
using MealPlanner.Shared.Resources;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    public partial class RegisterViewModel(AuthenticationService authService) : BaseViewModel
    {
        public RegistrationModel Model { get; } = new();

        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (IsBusy) return;
            ClearMessages();
            if (Model.Password != Model.ConfirmPassword)
            {
                SetError(MealPlannerSharedMessages.PasswordsDoNotMatch);
                return;
            }
            IsBusy = true;
            try
            {
                var result = await authService.RegisterAsync(Model);
                if (result?.Succeeded == true)
                {
                    SetSuccess(MealPlannerSharedMessages.RegistrationSuccess);
                    await Shell.Current.GoToAsync("..");
                }
                else SetError(result?.Message);
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private Task GoBackAsync() => Shell.Current.GoToAsync("..");
    }
}
