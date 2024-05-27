using BlazorBootstrap;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ProductCategoryEdit
    {
        [Parameter]
        public string? Id { get; set; }
        public EditProductCategoryModel? ProductCategory { get; set; }

        [Inject]
        public IProductCategoryService? ProductCategoryService { get; set; }

        [Inject]
        public IProductCategoryService? ProductCategoriesService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [CascadingParameter(Name = "MessageComponent")]
        protected IMessageComponent? MessageComponent { get; set; }

        protected ConfirmDialog dialog = default!;

        protected override async Task OnInitializedAsync()
        {
            _ = int.TryParse(Id, out var id);

            if (id == 0)
            {
                var categories = await ProductCategoriesService!.GetAllAsync();
                ProductCategory = new EditProductCategoryModel();
            }
            else
            {
                ProductCategory = await ProductCategoryService!.GetEditAsync(id);
            }
        }

        private async void SaveAsync()
        {
            var response = ProductCategory?.Id == 0 ? await ProductCategoryService!.AddAsync(ProductCategory) : await ProductCategoryService!.UpdateAsync(ProductCategory!);
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
            if (ProductCategory?.Id != 0)
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

                var response = await ProductCategoryService!.DeleteAsync(ProductCategory!.Id);
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
            NavigationManager?.NavigateTo("/productcategoriesoverview");
        }
    }
}
