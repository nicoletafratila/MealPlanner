using BlazorBootstrap;
using Blazored.SessionStorage;
using Common.Constants;
using Common.Pagination;
using MealPlanner.UI.Web.Services;
using MealPlanner.UI.Web.Services.RecipeBooks;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages.RecipeBooks
{
    [Authorize]
    public partial class ProductCategoriesOverview
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = default!;
        private GridTemplate<ProductCategoryModel>? _categoriesGrid = default!;
        private string _tableGridClass = CssClasses.GridTemplateEmptyClass;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Inject]
        public IProductCategoryService? ProductCategoriesService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public ISessionStorageService? SessionStorage { get; set; }

        protected override void OnInitialized()
        {
            _navItems = new List<BreadcrumbItem>
            {
                 new BreadcrumbItem{ Text = "Home", Href ="recipebooks/recipesoverview" }
            };
        }

        private void New()
        {
            NavigationManager?.NavigateTo($"recipebooks/productcategoryedit/");
        }

        private void Update(ProductCategoryModel item)
        {
            NavigationManager?.NavigateTo($"recipebooks/productcategoryedit/{item.Id}");
        }

        private async Task DeleteAsync(ProductCategoryModel item)
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
                var confirmation = await _dialog.ShowAsync(
                        title: "Are you sure you want to delete this?",
                        message1: "This will delete the record. Once deleted can not be rolled back.",
                        message2: "Do you want to proceed?",
                        confirmDialogOptions: options);

                if (!confirmation)
                    return;

                var response = await ProductCategoriesService!.DeleteAsync(item.Id);
                if (response != null && !response.Succeeded)
                {
                    MessageComponent?.ShowError(response.Message!);
                }
                else
                {
                    MessageComponent?.ShowInfo("Data has been deleted successfully");
                    await _categoriesGrid!.RefreshDataAsync();
                }
            }
        }

        private async Task<GridDataProviderResult<ProductCategoryModel>> DataProviderAsync(GridDataProviderRequest<ProductCategoryModel> request)
        {
            var queryParameters = new QueryParameters<ProductCategoryModel>()
            {
                Filters = request.Filters,
                Sorting = request.Sorting?.Select(x => QueryParameters<ProductCategoryModel>.ToModel(x)).ToList(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };

            var result = await ProductCategoriesService!.SearchAsync(queryParameters);
            if (result == null || result.Items == null)
            {
                result = new PagedList<ProductCategoryModel>(new List<ProductCategoryModel>(), new Metadata());
            }
            await SessionStorage!.SetItemAsync(queryParameters);
            _tableGridClass = result!.Items!.Count == 0 ? CssClasses.GridTemplateEmptyClass : CssClasses.GridTemplateWithItemsClass + " grid-additional-columns";
            StateHasChanged();
            return new GridDataProviderResult<ProductCategoryModel> { Data = result!.Items, TotalCount = result.Metadata!.TotalCount };
        }
    }
}
