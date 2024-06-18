using BlazorBootstrap;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class UnitEdit
    {
        private List<BreadcrumbItem>? NavItems { get; set; }

        [Parameter]
        public string? Id { get; set; }
        public UnitEditModel? Unit { get; set; }

        [Inject]
        public IUnitService? UnitService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [CascadingParameter(Name = "MessageComponent")]
        protected IMessageComponent? MessageComponent { get; set; }

        protected ConfirmDialog dialog = default!;

        protected override async Task OnInitializedAsync()
        {
            NavItems = new List<BreadcrumbItem>
            {
                new BreadcrumbItem{ Text = "Home", Href ="/" },
                new BreadcrumbItem{ Text = "Units", Href ="/unitsoverview" },
                new BreadcrumbItem{ Text = "Unit", IsCurrentPage = true },
            };

            _ = int.TryParse(Id, out var id);
            if (id == 0)
            {
                var categories = await UnitService!.GetAllAsync();
                Unit = new UnitEditModel();
            }
            else
            {
                Unit = await UnitService!.GetEditAsync(id);
            }
        }

        private async void SaveAsync()
        {
            var response = Unit?.Id == 0 ? await UnitService!.AddAsync(Unit) : await UnitService!.UpdateAsync(Unit!);
            if (!string.IsNullOrWhiteSpace(response))
            {
                MessageComponent?.ShowError(response);
            }
            else
            {
                MessageComponent?.ShowInfo("Data has been saved successfully");
                NavigateToOverview();
            }
        }

        private async void DeleteAsync()
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
                var confirmation = await dialog.ShowAsync(
                        title: "Are you sure you want to delete this?",
                        message1: "This will delete the record. Once deleted can not be rolled back.",
                        message2: "Do you want to proceed?",
                        confirmDialogOptions: options);

                if (!confirmation)
                    return;

                var response = await UnitService!.DeleteAsync(Unit!.Id);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    MessageComponent?.ShowError(response);
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
            NavigationManager?.NavigateTo("/Unitsoverview");
        }
    }
}
