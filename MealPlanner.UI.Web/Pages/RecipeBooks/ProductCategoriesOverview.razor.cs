using BlazorBootstrap;
using Blazored.SessionStorage;
using Common.Constants;
using Common.Pagination;
using Common.UI;
using Identity.Services.Core;
using Identity.Services.Http;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages.RecipeBooks
{
    [Authorize]
    public partial class ProductCategoriesOverview
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = [];
        private GridTemplate<ProductCategoryModel>? _categoriesGrid;
        private string _tableGridClass = CssClasses.GridTemplateEmptyClass;
        private BlazorBootstrap.SortDirection _nameSortDirection = BlazorBootstrap.SortDirection.Ascending;
        private int _gridKey = 0;
        private bool _firstLoad = true;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Inject]
        public IProductCategoryService ProductCategoriesService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        public ISessionStorageService SessionStorage { get; set; } = default!;

        protected override void OnInitialized()
        {
            _navItems =
            [
                new BreadcrumbItem { Text = Resources.ProductCategoriesOverview.BreadcrumbHome, Href = "recipebooks/recipesoverview" }
            ];
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            var stored = await SessionStorage.GetItemAsync<QueryParameters<ProductCategoryModel>>();
            var nameSort = stored?.Sorting?.FirstOrDefault(s => s.PropertyName == "Name");
            var direction = (BlazorBootstrap.SortDirection)(int)(nameSort?.Direction ?? Common.Pagination.SortDirection.Ascending);

            if (direction != _nameSortDirection)
            {
                _nameSortDirection = direction;
                _gridKey++;
                StateHasChanged();
            }
        }

        private async Task<GridSettings?> SettingsProviderAsync()
        {
            var qp = await SessionStorage.GetItemAsync<QueryParameters<ProductCategoryModel>>();
            return qp is null ? null : new GridSettings { PageNumber = qp.PageNumber, PageSize = qp.PageSize };
        }

        private void New()
        {
            NavigationManager.NavigateTo("recipebooks/productcategoryedit/");
        }

        private void Update(ProductCategoryModel item)
        {
            if (item is null)
                return;

            NavigationManager.NavigateTo($"recipebooks/productcategoryedit/{item.Id}");
        }

        private async Task DeleteAsync(ProductCategoryModel item)
        {
            if (item is null)
                return;

            var options = new ConfirmDialogOptions
            {
                YesButtonText = Resources.ProductCategoriesOverview.DialogYesButton,
                YesButtonColor = ButtonColor.Success,
                NoButtonText = Resources.ProductCategoriesOverview.DialogNoButton,
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: Resources.ProductCategoriesOverview.DeleteDialogTitle,
                message1: Resources.ProductCategoriesOverview.DeleteDialogMessage1,
                message2: Resources.ProductCategoriesOverview.DeleteDialogMessage2,
                confirmDialogOptions: options);

            if (!confirmation)
                return;

            await DeleteCoreAsync(item);
        }

        private async Task DeleteCoreAsync(ProductCategoryModel item)
        {
            var response = await ProductCategoriesService.DeleteAsync(item.Id);
            if (response is null)
            {
                await ShowErrorAsync(Resources.ProductCategoriesOverview.DeleteFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.ProductCategoriesOverview.DeleteFailed);
                return;
            }

            await ShowInfoAsync(Resources.ProductCategoriesOverview.DeleteSucceeded);

            if (_categoriesGrid is not null)
                await _categoriesGrid.RefreshDataAsync();
        }

        private async Task<GridDataProviderResult<ProductCategoryModel>> DataProviderAsync(
             GridDataProviderRequest<ProductCategoryModel> request)
        {
            var queryParameters = new QueryParameters<ProductCategoryModel>
            {
                Filters = request.Filters?
                    .Select(f => new Common.Pagination.FilterItem(f.PropertyName, f.Value, (Common.Pagination.FilterOperator)(int)f.Operator, f.StringComparison))
                    .ToList(),
                Sorting = request.Sorting?
                    .Select(s => new SortingModel { PropertyName = s.SortString, Direction = (Common.Pagination.SortDirection)(int)s.SortDirection })
                    .ToList() ?? [],
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var isFirstLoad = _firstLoad;
            _firstLoad = false;

            var result = await ProductCategoriesService.SearchAsync(queryParameters)
                         ?? new PagedList<ProductCategoryModel>([], new Metadata());

            var items = result.Items ?? [];

            if (!isFirstLoad)
                await SessionStorage.SetItemAsync(queryParameters);

            _tableGridClass = items.Count == 0
                ? CssClasses.GridTemplateEmptyClass
                : CssClasses.GridTemplateWithItemsClass + " grid-additional-columns";

            StateHasChanged();
            return new GridDataProviderResult<ProductCategoryModel>
            {
                Data = items,
                TotalCount = result.Metadata?.TotalCount ?? 0
            };
        }

        private async Task ShowErrorAsync(string message)
           => await MessageComponent!.ShowErrorAsync(message);

        private async Task ShowInfoAsync(string message)
            => await MessageComponent!.ShowInfoAsync(message);
    }
}