using BlazorBootstrap;
using Blazored.SessionStorage;
using Common.Constants;
using Common.Models;
using Common.Pagination;
using Common.UI;
using Identity.Services;
using MealPlanner.Services;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Services;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages.RecipeBooks
{
    [Authorize]
    public partial class RecipesOverview
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = [];
        private GridTemplate<RecipeModel>? _recipesGrid;
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
        public IRecipeService RecipeService { get; set; } = default!;

        [Inject]
        public IMealPlanService MealPlanService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        public ISessionStorageService SessionStorage { get; set; } = default!;

        protected override void OnInitialized()
        {
            _navItems =
            [
                new BreadcrumbItem { Text = Resources.RecipesOverview.BreadcrumbHome, Href = "recipebooks/recipesoverview" }
            ];
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            var stored = await SessionStorage.GetItemAsync<QueryParameters<RecipeModel>>();
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
            => await SessionStorage.GetItemAsync<QueryParameters<RecipeModel>>();

        private void New()
        {
            NavigationManager.NavigateTo("recipebooks/recipeedit/");
        }

        private void Update(RecipeModel item)
        {
            if (item is null)
                return;

            NavigationManager.NavigateTo($"recipebooks/recipeedit/{item.Id}");
        }

        private async Task DeleteAsync(RecipeModel item)
        {
            if (item is null)
                return;

            var options = new ConfirmDialogOptions
            {
                YesButtonText = Resources.RecipesOverview.DialogYesButton,
                YesButtonColor = ButtonColor.Success,
                NoButtonText = Resources.RecipesOverview.DialogNoButton,
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: Resources.RecipesOverview.DeleteDialogTitle,
                message1: Resources.RecipesOverview.DeleteDialogMessage1,
                message2: Resources.RecipesOverview.DeleteDialogMessage2,
                confirmDialogOptions: options);

            if (!confirmation)
                return;

            await DeleteCoreAsync(item);
        }

        private async Task DeleteCoreAsync(RecipeModel item)
        {
            var response = await RecipeService.DeleteAsync(item.Id);
            if (response is null)
            {
                await ShowErrorAsync(Resources.RecipesOverview.DeleteFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.RecipesOverview.DeleteFailed);
                return;
            }

            await ShowInfoAsync(Resources.RecipesOverview.DeleteSucceeded);

            if (_recipesGrid is not null)
                await _recipesGrid.RefreshDataAsync();
        }

        private async Task<GridDataProviderResult<RecipeModel>> DataProviderAsync(GridDataProviderRequest<RecipeModel> request)
        {
            if (request.PageSize > 0)
                _pageSize = request.PageSize;

            var filtersKey = GetFiltersKey(request.Filters);
            var filterCleared = !_firstLoad && _lastFiltersKey != "" && filtersKey == "";
            var filterApplied = !_firstLoad && _lastFiltersKey == "" && filtersKey != "";

            if (filterCleared)
            {
                var restoreParameters = new QueryParameters<RecipeModel>
                {
                    Filters = [],
                    Sorting = request.Sorting?
                        .Select(QueryParameters<RecipeModel>.ToModel)
                        .ToList() ?? [],
                    PageNumber = _pageBeforeFilter,
                    PageSize = _pageSize
                };
                await SessionStorage.SetItemAsync(restoreParameters);
                _lastFiltersKey = filtersKey;
                _firstLoad = true;
                _gridKey++;
                StateHasChanged();
                return new GridDataProviderResult<RecipeModel> { Data = [], TotalCount = 0 };
            }

            if (filterApplied)
            {
                var resetParameters = new QueryParameters<RecipeModel>
                {
                    Filters = request.Filters,
                    Sorting = request.Sorting?
                        .Select(QueryParameters<RecipeModel>.ToModel)
                        .ToList() ?? [],
                    PageNumber = 1,
                    PageSize = _pageSize
                };
                await SessionStorage.SetItemAsync(resetParameters);
                _lastFiltersKey = filtersKey;
                _firstLoad = true;
                _gridKey++;
                StateHasChanged();
                return new GridDataProviderResult<RecipeModel> { Data = [], TotalCount = 0 };
            }

            _lastFiltersKey = filtersKey;

            if (filtersKey == "")
                _pageBeforeFilter = request.PageNumber;

            var queryParameters = new QueryParameters<RecipeModel>
            {
                Filters = request.Filters?.ToList() ?? [],
                Sorting = request.Sorting?
                    .Select(QueryParameters<RecipeModel>.ToModel)
                    .ToList()!,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var isFirstLoad = _firstLoad;
            _firstLoad = false;

            var result = await RecipeService.SearchAsync(queryParameters)
                         ?? new PagedList<RecipeModel>([], new Metadata());

            var items = result.Items ?? [];

            if (!isFirstLoad)
            {
                var sessionParameters = new QueryParameters<RecipeModel>
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
            return new GridDataProviderResult<RecipeModel>
            {
                Data = items,
                TotalCount = result.Metadata?.TotalCount ?? 0
            };
        }

        private async Task AddToMealPlanAsync(RecipeModel recipe)
        {
            if (recipe is null)
                return;

            var mealPlan = await MealPlanService.GetCurrentAsync();
            if (mealPlan is null)
            {
                await ShowErrorAsync(Resources.RecipesOverview.NoCurrentMealPlan);
                return;
            }

            var mealPlanToAdd = await MealPlanService.GetEditAsync(mealPlan.Id);
            if (mealPlanToAdd is null)
            {
                await ShowErrorAsync(Resources.RecipesOverview.SaveFailedMessage);
                return;
            }

            mealPlanToAdd.Recipes ??= [];

            var existing = mealPlanToAdd.Recipes.FirstOrDefault(r => r.Id == recipe.Id);
            if (existing is not null)
                return;

            mealPlanToAdd.Recipes.Add(recipe);
            mealPlanToAdd.Recipes.SetIndexes();

            var response = await MealPlanService.UpdateAsync(mealPlanToAdd);
            if (response is null || !response.Succeeded)
            {
                await ShowErrorAsync(response?.Message ?? Resources.RecipesOverview.SaveFailedMessage);
                return;
            }

            await ShowInfoAsync(Resources.RecipesOverview.RecipeAdded);
        }

        private async Task ShowErrorAsync(string message)
            => await MessageComponent!.ShowErrorAsync(message);

        private async Task ShowInfoAsync(string message)
            => await MessageComponent!.ShowInfoAsync(message);

        private string GetFiltersKey(IEnumerable<FilterItem>? filters)
        {
            if (filters == null) return "";
            return string.Join("|", filters
                .Where(f => !string.IsNullOrEmpty(f.Value))
                .OrderBy(f => f.PropertyName)
                .Select(f => $"{f.PropertyName}={f.Operator}:{f.Value}"));
        }
    }
}