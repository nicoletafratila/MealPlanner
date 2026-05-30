using BlazorBootstrap;
using Common.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Services.Http;
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
                new BreadcrumbItem { Text = Resources.UnitEdit.BreadcrumbUnits, Href = "recipebooks/unitsoverview" },
                new BreadcrumbItem { Text = Resources.UnitEdit.BreadcrumbUnit, IsCurrentPage = true },
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
                await ShowErrorAsync(Resources.UnitEdit.SaveFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.UnitEdit.SaveFailed);
                return;
            }

            await ShowInfoAsync(Resources.UnitEdit.SaveSucceeded);
            NavigateToOverview();
        }

        private async Task DeleteAsync()
        {
            if (Unit.Id == 0)
                return;

            var options = new ConfirmDialogOptions
            {
                YesButtonText = Resources.UnitEdit.DialogYesButton,
                YesButtonColor = ButtonColor.Success,
                NoButtonText = Resources.UnitEdit.DialogNoButton,
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: Resources.UnitEdit.DeleteDialogTitle,
                message1: Resources.UnitEdit.DeleteDialogMessage1,
                message2: Resources.UnitEdit.DeleteDialogMessage2,
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
                await ShowErrorAsync(Resources.UnitEdit.DeleteFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.UnitEdit.DeleteFailed);
                return;
            }

            await ShowInfoAsync(Resources.UnitEdit.DeleteSucceeded);
            NavigateToOverview();
        }

        private void NavigateToOverview()
        {
            NavigationManager.NavigateTo("recipebooks/unitsoverview");
        }

        private async Task ShowErrorAsync(string message)
            => await MessageComponent!.ShowErrorAsync(message);

        private async Task ShowInfoAsync(string message)
            => await MessageComponent!.ShowInfoAsync(message);
    }
}