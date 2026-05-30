using BlazorBootstrap;
using Common.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages.RecipeBooks
{
    [Authorize]
    public partial class ProductCategoryEdit
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = new();

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Parameter]
        public string? Id { get; set; }

        public ProductCategoryEditModel ProductCategory { get; set; } = new();

        [Inject]
        public IProductCategoryService ProductCategoryService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            _navItems =
            [
                new BreadcrumbItem { Text = Resources.ProductCategoryEdit.BreadcrumbProductCategories, Href = "recipebooks/productcategoriesoverview" },
                new BreadcrumbItem { Text = Resources.ProductCategoryEdit.BreadcrumbProductCategory, IsCurrentPage = true },
            ];

            if (!int.TryParse(Id, out var id) || id == 0)
            {
                ProductCategory = new ProductCategoryEditModel();
            }
            else
            {
                ProductCategory = await ProductCategoryService.GetEditAsync(id)
                                  ?? new ProductCategoryEditModel { Id = id };
            }
        }

        private async Task SaveAsync()
        {
            await SaveCoreAsync(ProductCategory);
        }

        private async Task SaveCoreAsync(ProductCategoryEditModel productCategory)
        {
            Common.Models.CommandResponse? response;

            if (productCategory.Id == 0)
            {
                response = await ProductCategoryService.AddAsync(productCategory);
            }
            else
            {
                response = await ProductCategoryService.UpdateAsync(productCategory);
            }

            if (response is null)
            {
                await ShowErrorAsync(Resources.ProductCategoryEdit.SaveFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.ProductCategoryEdit.SaveFailed);
                return;
            }

            await ShowInfoAsync(Resources.ProductCategoryEdit.SaveSucceeded);
            NavigateToOverview();
        }

        private async Task DeleteAsync()
        {
            if (ProductCategory.Id == 0)
                return;

            var options = new ConfirmDialogOptions
            {
                YesButtonText = Resources.ProductCategoryEdit.DialogYesButton,
                YesButtonColor = ButtonColor.Success,
                NoButtonText = Resources.ProductCategoryEdit.DialogNoButton,
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: Resources.ProductCategoryEdit.DeleteDialogTitle,
                message1: Resources.ProductCategoryEdit.DeleteDialogMessage1,
                message2: Resources.ProductCategoryEdit.DeleteDialogMessage2,
                confirmDialogOptions: options);

            if (!confirmation)
                return;

            await DeleteCoreAsync(ProductCategory);
        }

        private async Task DeleteCoreAsync(ProductCategoryEditModel productCategory)
        {
            if (productCategory.Id == 0)
                return;

            var response = await ProductCategoryService.DeleteAsync(productCategory.Id);
            if (response is null)
            {
                await ShowErrorAsync(Resources.ProductCategoryEdit.DeleteFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.ProductCategoryEdit.DeleteFailed);
                return;
            }

            await ShowInfoAsync(Resources.ProductCategoryEdit.DeleteSucceeded);
            NavigateToOverview();
        }

        private void NavigateToOverview()
        {
            NavigationManager.NavigateTo("recipebooks/productcategoriesoverview");
        }

        private async Task ShowErrorAsync(string message)
            => await MessageComponent!.ShowErrorAsync(message);

        private async Task ShowInfoAsync(string message)
            => await MessageComponent!.ShowInfoAsync(message);
    }
}