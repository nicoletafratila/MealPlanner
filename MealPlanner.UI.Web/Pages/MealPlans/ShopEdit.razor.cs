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
                new BreadcrumbItem { Text = Resources.ShopEdit.BreadcrumbShops, Href = "mealplans/shopsoverview" },
                new BreadcrumbItem { Text = Resources.ShopEdit.BreadcrumbShop, IsCurrentPage = true },
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
                await ShowErrorAsync(Resources.ShopEdit.SaveFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.ShopEdit.SaveFailed);
                return;
            }

            await ShowInfoAsync(Resources.ShopEdit.SaveSucceeded);
            NavigateToOverview();
        }

        private async Task DeleteAsync()
        {
            if (Shop.Id == 0)
                return;

            var options = new ConfirmDialogOptions
            {
                YesButtonText = Resources.ShopEdit.DialogYesButton,
                YesButtonColor = ButtonColor.Success,
                NoButtonText = Resources.ShopEdit.DialogNoButton,
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: Resources.ShopEdit.DeleteDialogTitle,
                message1: Resources.ShopEdit.DeleteDialogMessage1,
                message2: Resources.ShopEdit.DeleteDialogMessage2,
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
                await ShowErrorAsync(Resources.ShopEdit.DeleteFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.ShopEdit.DeleteFailed);
                return;
            }

            await ShowInfoAsync(Resources.ShopEdit.DeleteSucceeded);
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

        private async Task ShowErrorAsync(string message)
            => await MessageComponent!.ShowErrorAsync(message);

        private async Task ShowInfoAsync(string message)
            => await MessageComponent!.ShowInfoAsync(message);
    }
}