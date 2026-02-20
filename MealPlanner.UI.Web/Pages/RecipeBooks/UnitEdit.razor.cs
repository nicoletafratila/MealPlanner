using BlazorBootstrap;
using Common.UI;
using MealPlanner.UI.Web.Services.RecipeBooks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages.RecipeBooks
{
    [Authorize]
    public partial class UnitEdit
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = new();

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Parameter]
        public string? Id { get; set; }

        public UnitEditModel Unit { get; private set; } = new();

        [Inject]
        public IUnitService UnitService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            _navItems =
            [
                new BreadcrumbItem { Text = "Units", Href = "recipebooks/unitsoverview" },
                new BreadcrumbItem { Text = "Unit", IsCurrentPage = true },
            ];

            if (!int.TryParse(Id, out var id) || id == 0)
            {
                Unit = new UnitEditModel();
            }
            else
            {
                Unit = await UnitService.GetEditAsync(id) ?? new UnitEditModel { Id = id };
            }
        }

        private async Task SaveAsync()
        {
            await SaveCoreAsync(Unit);
        }

        private async Task SaveCoreAsync(UnitEditModel unit)
        {
            Common.Models.CommandResponse? response;

            if (unit.Id == 0)
            {
                response = await UnitService.AddAsync(unit);
            }
            else
            {
                response = await UnitService.UpdateAsync(unit);
            }

            if (response is null)
            {
                ShowError("Save failed. Please try again.");
                return;
            }

            if (!response.Succeeded)
            {
                ShowError(response.Message ?? "Save failed.");
                return;
            }

            ShowInfo("Data has been saved successfully");
            NavigateToOverview();
        }

        private async Task DeleteAsync()
        {
            if (Unit.Id == 0)
                return;

            var options = new ConfirmDialogOptions
            {
                YesButtonText = "OK",
                YesButtonColor = ButtonColor.Success,
                NoButtonText = "Cancel",
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: "Are you sure you want to delete this?",
                message1: "This will delete the record. Once deleted it cannot be rolled back.",
                message2: "Do you want to proceed?",
                confirmDialogOptions: options);

            if (!confirmation)
                return;

            await DeleteCoreAsync(Unit);
        }

        private async Task DeleteCoreAsync(UnitEditModel unit)
        {
            if (unit.Id == 0)
                return;

            var response = await UnitService.DeleteAsync(unit.Id);
            if (response is null)
            {
                ShowError("Delete failed. Please try again.");
                return;
            }

            if (!response.Succeeded)
            {
                ShowError(response.Message ?? "Delete failed.");
                return;
            }

            ShowInfo("Data has been deleted successfully");
            NavigateToOverview();
        }

        private void NavigateToOverview()
        {
            NavigationManager.NavigateTo("recipebooks/unitsoverview");
        }

        private void ShowError(string message)
            => MessageComponent?.ShowError(message);

        private void ShowInfo(string message)
            => MessageComponent?.ShowInfo(message);
    }
}