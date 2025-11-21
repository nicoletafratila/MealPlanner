using BlazorBootstrap;
using MealPlanner.UI.Web.Services;
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
        private List<BreadcrumbItem> _navItems = default!;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Parameter]
        public string? Id { get; set; }
        public UnitEditModel? Unit { get; set; }

        [Inject]
        public IUnitService? UnitService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _navItems = new List<BreadcrumbItem>
            {
                new BreadcrumbItem{ Text = "Units", Href ="recipebooks/unitsoverview" },
                new BreadcrumbItem{ Text = "Unit", IsCurrentPage = true },
            };

            _ = int.TryParse(Id, out var id);
            if (id == 0)
            {
                Unit = new UnitEditModel();
            }
            else
            {
                Unit = await UnitService!.GetEditAsync(id);
            }
        }

        private async Task SaveAsync()
        {
            var response = Unit?.Id == 0 ? await UnitService!.AddAsync(Unit) : await UnitService!.UpdateAsync(Unit!);
            if (response != null && !response.Succeeded)
            {
                MessageComponent?.ShowError(response.Message!);
            }
            else
            {
                MessageComponent?.ShowInfo("Data has been saved successfully");
                NavigateToOverview();
            }
        }

        private async Task DeleteAsync()
        {
            if (Unit?.Id != 0)
            {
                var options = new ConfirmDialogOptions
                {
                    YesButtonText = "OK",
                    YesButtonColor = ButtonColor.Success,
                    NoButtonText = "Cancel",
                    NoButtonColor = ButtonColor.Danger
                };
                var confirmation = await _dialog.ShowAsync(
                        title: "Are you sure you want to delete this?",
                        message1: "This will delete the record. Once deleted can not be rolled back.",
                        message2: "Do you want to proceed?",
                        confirmDialogOptions: options);

                if (!confirmation)
                    return;

                var response = await UnitService!.DeleteAsync(Unit!.Id);
                if (response != null && !response.Succeeded)
                {
                    MessageComponent?.ShowError(response.Message!);
                }
                else
                {
                    MessageComponent?.ShowInfo("Data has been deleted successfully");
                    NavigateToOverview();
                }
            }
        }

        private void NavigateToOverview()
        {
            NavigationManager?.NavigateTo("recipebooks/unitsoverview");
        }
    }
}
