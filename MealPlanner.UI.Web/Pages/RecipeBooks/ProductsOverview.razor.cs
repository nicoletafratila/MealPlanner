using BlazorBootstrap;
using Blazored.SessionStorage;
using Common.Constants;
using Common.Pagination;
using Common.UI;
using MealPlanner.UI.Web.Services.Identities;
using MealPlanner.UI.Web.Services.RecipeBooks;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages.RecipeBooks
{
    [Authorize]
    public partial class ProductsOverview
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = [];
        private GridTemplate<ProductModel>? _productsGrid;
        private string _tableGridClass = CssClasses.GridTemplateEmptyHorizontalClass;
        private SortDirection _nameSortDirection = SortDirection.Ascending;
        private int _gridKey = 0;
        private bool _firstLoad = true;
        private int _pageBeforeFilter = 1;
        private string _lastFiltersKey = "";
        private int _pageSize = 20;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Inject]
        public IProductService ProductService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        public ISessionStorageService SessionStorage { get; set; } = default!;

        protected override void OnInitialized()
        {
            _navItems =
            [
                new BreadcrumbItem { Text = Resources.ProductsOverview.BreadcrumbHome, Href = "recipebooks/recipesoverview" }
            ];
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            var stored = await SessionStorage.GetItemAsync<QueryParameters<ProductModel>>();
            var nameSort = stored?.Sorting?.FirstOrDefault(s => s.PropertyName == "Name");
            var direction = nameSort?.Direction ?? SortDirection.Ascending;

            if (direction != _nameSortDirection)
            {
                _nameSortDirection = direction;
                _gridKey++;
                StateHasChanged();
            }
        }

        private async Task<GridSettings?> SettingsProviderAsync()
            => await SessionStorage.GetItemAsync<QueryParameters<ProductModel>>();

        private void New()
        {
            NavigationManager.NavigateTo("recipebooks/productedit/");
        }

        private void Update(ProductModel item)
        {
            if (item is null)
                return;

            NavigationManager.NavigateTo($"recipebooks/productedit/{item.Id}");
        }

        private async Task DeleteAsync(ProductModel item)
        {
            if (item is null)
                return;

            var options = new ConfirmDialogOptions
            {
                YesButtonText = Resources.ProductsOverview.DialogYesButton,
                YesButtonColor = ButtonColor.Success,
                NoButtonText = Resources.ProductsOverview.DialogNoButton,
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: Resources.ProductsOverview.DeleteDialogTitle,
                message1: Resources.ProductsOverview.DeleteDialogMessage1,
                message2: Resources.ProductsOverview.DeleteDialogMessage2,
                confirmDialogOptions: options);

            if (!confirmation)
                return;

            await DeleteCoreAsync(item);
        }

        private async Task DeleteCoreAsync(ProductModel item)
        {
            var response = await ProductService.DeleteAsync(item.Id);
            if (response is null)
            {
                await ShowErrorAsync(Resources.ProductsOverview.DeleteFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.ProductsOverview.DeleteFailed);
                return;
            }

            await ShowInfoAsync(Resources.ProductsOverview.DeleteSucceeded);

            if (_productsGrid is not null)
                await _productsGrid.RefreshDataAsync();
        }

        private async Task<GridDataProviderResult<ProductModel>> DataProviderAsync(
            GridDataProviderRequest<ProductModel> request)
        {
            if (request.PageSize > 0)
                _pageSize = request.PageSize;

            var filtersKey = GetFiltersKey(request.Filters);
            var filterCleared = !_firstLoad && _lastFiltersKey != "" && filtersKey == "";
            var filterApplied = !_firstLoad && _lastFiltersKey == "" && filtersKey != "";

            if (filterCleared)
            {
                var restoreParameters = new QueryParameters<ProductModel>
                {
                    Filters = [],
                    Sorting = request.Sorting?
                        .Select(QueryParameters<ProductModel>.ToModel)
                        .ToList() ?? [],
                    PageNumber = _pageBeforeFilter,
                    PageSize = _pageSize
                };
                await SessionStorage.SetItemAsync(restoreParameters);
                _lastFiltersKey = filtersKey;
                _firstLoad = true;
                _gridKey++;
                StateHasChanged();
                return new GridDataProviderResult<ProductModel> { Data = [], TotalCount = 0 };
            }

            if (filterApplied)
            {
                var resetParameters = new QueryParameters<ProductModel>
                {
                    Filters = request.Filters,
                    Sorting = request.Sorting?
                        .Select(QueryParameters<ProductModel>.ToModel)
                        .ToList() ?? [],
                    PageNumber = 1,
                    PageSize = _pageSize
                };
                await SessionStorage.SetItemAsync(resetParameters);
                _lastFiltersKey = filtersKey;
                _firstLoad = true;
                _gridKey++;
                StateHasChanged();
                return new GridDataProviderResult<ProductModel> { Data = [], TotalCount = 0 };
            }

            _lastFiltersKey = filtersKey;

            if (filtersKey == "")
                _pageBeforeFilter = request.PageNumber;

            var queryParameters = new QueryParameters<ProductModel>
            {
                Filters = request.Filters?.ToList() ?? [],
                Sorting = request.Sorting?
                    .Select(QueryParameters<ProductModel>.ToModel)
                    .ToList()!,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var isFirstLoad = _firstLoad;
            _firstLoad = false;

            var result = await ProductService.SearchAsync(queryParameters)
                         ?? new PagedList<ProductModel>([], new Metadata());

            var items = result.Items ?? [];

            if (!isFirstLoad)
            {
                var sessionParameters = new QueryParameters<ProductModel>
                {
                    Filters = request.Filters,
                    Sorting = queryParameters.Sorting,
                    PageNumber = request.PageNumber,
                    PageSize = _pageSize
                };
                await SessionStorage.SetItemAsync(sessionParameters);
            }

            _tableGridClass = items.Count == 0
                ? CssClasses.GridTemplateEmptyHorizontalClass
                : CssClasses.GridTemplateWithItemsHorizontalClass;

            StateHasChanged();
            return new GridDataProviderResult<ProductModel>
            {
                Data = items,
                TotalCount = result.Metadata?.TotalCount ?? 0
            };
        }

        private string GetFiltersKey(IEnumerable<FilterItem>? filters)
        {
            if (filters == null) return "";
            return string.Join("|", filters
                .Where(f => !string.IsNullOrEmpty(f.Value))
                .OrderBy(f => f.PropertyName)
                .Select(f => $"{f.PropertyName}={f.Operator}:{f.Value}"));
        }

        private async Task ShowErrorAsync(string message)
            => await MessageComponent!.ShowErrorAsync(message);

        private async Task ShowInfoAsync(string message)
            => await MessageComponent!.ShowInfoAsync(message);
    }
}