using BlazorBootstrap;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class UnitsOverview
    {
        public UnitEditModel? Unit { get; set; }
        public IList<UnitModel>? Units { get; set; }

        [Inject]
        public IUnitService? UnitsService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [CascadingParameter(Name = "MessageComponent")]
        protected IMessageComponent? MessageComponent { get; set; }

        protected ConfirmDialog dialog = default!;

        protected override async Task OnInitializedAsync()
        {
            await RefreshAsync();
        }

        private void New()
        {
            NavigationManager?.NavigateTo($"unitedit/");
        }

        private void Update(UnitModel item)
        {
            NavigationManager?.NavigateTo($"unitedit/{item.Id}");
        }

        private async void DeleteAsync(UnitModel item)
        {
            if (item != null)
            {
                var options = new ConfirmDialogOptions
                {
                    YesButtonText = "OK",
                    YesButtonColor = ButtonColor.Success,
                    NoButtonText = "Cancel",
                    NoButtonColor = ButtonColor.Danger
                };
                var confirmation = await dialog.ShowAsync(
                        title: "Are you sure you want to delete this?",
                        message1: "This will delete the record. Once deleted can not be rolled back.",
                        message2: "Do you want to proceed?",
                        confirmDialogOptions: options);

                if (!confirmation)
                    return;

                var response = await UnitsService!.DeleteAsync(item.Id);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    MessageComponent?.ShowError(response);
                }
                else
                {
                    MessageComponent?.ShowInfo("Data has been deleted successfully");
                    await RefreshAsync();
                }
            }
        }

        private async Task RefreshAsync()
        {
            Units = await UnitsService!.GetAllAsync();
            StateHasChanged();
        }
    }
}
