using CommunityToolkit.Mvvm.Input;
using Identity.Services.Http;
using Identity.Shared.Models;

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
                SetError("Passwords do not match.");
                return;
            }
            IsBusy = true;
            try
            {
                var result = await authService.RegisterAsync(Model);
                if (result?.Succeeded == true)
                {
                    SetSuccess("Registration successful. Please log in.");
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
