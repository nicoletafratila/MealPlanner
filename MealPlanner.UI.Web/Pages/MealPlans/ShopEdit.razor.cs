using BlazorBootstrap;
using Common.Models;
using Common.UI;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services.MealPlans;
using MealPlanner.UI.Web.Services.RecipeBooks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages.MealPlans
{
    [Authorize]
    public partial class ShopEdit
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = [];

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Parameter]
        public string? Id { get; set; }

        public ShopEditModel Shop { get; set; } = new();

        [Inject]
        public IShopService ShopService { get; set; } = default!;

        [Inject]
        public IProductCategoryService ProductCategoriesService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            _navItems =
            [
                new BreadcrumbItem { Text = "Shops", Href = "mealplans/shopsoverview" },
                new BreadcrumbItem { Text = "Shop", IsCurrentPage = true },
            ];

            _ = int.TryParse(Id, out var id);
            if (id == 0)
            {
                var categories = await ProductCategoriesService.SearchAsync();
                var items = categories?.Items ?? [];
                Shop = new ShopEditModel(items);
            }
            else
            {
                Shop = await ShopService.GetEditAsync(id) ?? new ShopEditModel([]);
            }
        }

        private async Task SaveAsync()
        {
            await SaveCoreAsync(Shop);
        }

        private async Task SaveCoreAsync(ShopEditModel shop)
        {
            CommandResponse? response;

            if (shop.Id == 0)
            {
                response = await ShopService.AddAsync(shop);
            }
            else
            {
                response = await ShopService.UpdateAsync(shop);
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
            if (Shop.Id == 0)
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
                message1: "This will delete the record. Once deleted can not be rolled back.",
                message2: "Do you want to proceed?",
                confirmDialogOptions: options);

            if (!confirmation)
                return;

            await DeleteCoreAsync(Shop);
        }

        private async Task DeleteCoreAsync(ShopEditModel shop)
        {
            if (shop.Id == 0)
                return;

            var response = await ShopService.DeleteAsync(shop.Id);
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

        private bool CanMoveUp(ShopDisplaySequenceEditModel item)
        {
            if (Shop.DisplaySequence is not { Count: > 1 })
                return false;

            var index = Shop.DisplaySequence.IndexOf(item);
            return index > 0;
        }

        private void MoveUp(ShopDisplaySequenceEditModel item)
        {
            if (Shop.DisplaySequence is not { Count: > 1 })
                return;

            var index = Shop.DisplaySequence.IndexOf(item);
            if (index <= 0)
                return;

            Shop.DisplaySequence.RemoveAt(index);
            Shop.DisplaySequence.Insert(index - 1, item);
            Shop.DisplaySequence.SetIndexes();
        }

        private bool CanMoveDown(ShopDisplaySequenceEditModel item)
        {
            if (Shop.DisplaySequence is not { Count: > 1 })
                return false;

            var index = Shop.DisplaySequence.IndexOf(item);
            return index >= 0 && index < Shop.DisplaySequence.Count - 1;
        }

        private void MoveDown(ShopDisplaySequenceEditModel item)
        {
            if (Shop.DisplaySequence is not { Count: > 1 })
                return;

            var index = Shop.DisplaySequence.IndexOf(item);
            if (index < 0 || index >= Shop.DisplaySequence.Count - 1)
                return;

            Shop.DisplaySequence.RemoveAt(index);
            Shop.DisplaySequence.Insert(index + 1, item);
            Shop.DisplaySequence.SetIndexes();
        }

        private void NavigateToOverview()
        {
            NavigationManager.NavigateTo("mealplans/shopsoverview");
        }

        private void ShowError(string message)
            => MessageComponent?.ShowError(message);

        private void ShowInfo(string message)
            => MessageComponent?.ShowInfo(message);
    }
}