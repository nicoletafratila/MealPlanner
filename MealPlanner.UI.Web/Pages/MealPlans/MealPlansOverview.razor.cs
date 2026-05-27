using BlazorBootstrap;
using Blazored.SessionStorage;
using Common.Constants;
using Common.Pagination;
using Common.UI;
using Identity.Services;
using MealPlanner.Services;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages.MealPlans
{
    [Authorize]
    public partial class MealPlansOverview
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = [];
        private GridTemplate<MealPlanModel>? _mealPlansGrid;
        private string _tableGridClass = CssClasses.GridTemplateEmptyClass;
        private SortDirection _nameSortDirection = SortDirection.Ascending;
        private int _gridKey = 0;
        private bool _firstLoad = true;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

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
                new BreadcrumbItem { Text = Resources.MealPlansOverview.BreadcrumbHome, Href = "recipebooks/recipesoverview" }
            ];
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            var stored = await SessionStorage.GetItemAsync<QueryParameters<MealPlanModel>>();
            var nameSort = stored?.Sorting?.FirstOrDefault(s => s.PropertyName == "Name");
            var direction = nameSort?.Direction ?? SortDirection.Ascending;

            if (direction != _nameSortDirection)
            {
                _nameSortDirection = direction;
                _gridKey++;
                StateHasChanged();
            }
        }

        private void New()
        {
            NavigationManager.NavigateTo("mealplans/mealplanedit/");
        }

        private void Update(MealPlanModel item)
        {
            if (item is null)
                return;

            NavigationManager.NavigateTo($"mealplans/mealplanedit/{item.Id}");
        }

        private async Task DeleteAsync(MealPlanModel item)
        {
            if (item is null)
                return;

            var options = new ConfirmDialogOptions
            {
                YesButtonText = Resources.MealPlansOverview.DialogYesButton,
                YesButtonColor = ButtonColor.Success,
                NoButtonText = Resources.MealPlansOverview.DialogNoButton,
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: Resources.MealPlansOverview.DeleteDialogTitle,
                message1: Resources.MealPlansOverview.DeleteDialogMessage1,
                message2: Resources.MealPlansOverview.DeleteDialogMessage2,
                confirmDialogOptions: options);

            if (!confirmation)
                return;

            await DeleteCoreAsync(item);
        }

        private async Task DeleteCoreAsync(MealPlanModel item)
        {
            var response = await MealPlanService.DeleteAsync(item.Id);
            if (response is null)
            {
                await ShowErrorAsync(Resources.MealPlansOverview.DeleteFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.MealPlansOverview.DeleteFailed);
                return;
            }

            await ShowInfoAsync(Resources.MealPlansOverview.DeleteSucceeded);

            if (_mealPlansGrid is not null)
                await _mealPlansGrid.RefreshDataAsync();
        }

        private async Task<GridSettings?> SettingsProviderAsync()
            => await SessionStorage.GetItemAsync<QueryParameters<MealPlanModel>>();

        private async Task<GridDataProviderResult<MealPlanModel>> DataProviderAsync(
            GridDataProviderRequest<MealPlanModel> request)
        {
            var queryParameters = new QueryParameters<MealPlanModel>
            {
                Filters = request.Filters,
                Sorting = request.Sorting?
                    .Select(QueryParameters<MealPlanModel>.ToModel)
                    .ToList()!,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };

            var isFirstLoad = _firstLoad;
            _firstLoad = false;

            var result = await MealPlanService.SearchAsync(queryParameters)
                         ?? new PagedList<MealPlanModel>([], new Metadata());

            var items = result.Items ?? [];

            if (!isFirstLoad)
                await SessionStorage.SetItemAsync(queryParameters);

            _tableGridClass = items.Count == 0
                ? CssClasses.GridTemplateEmptyClass
                : CssClasses.GridTemplateWithItemsClass + " grid-additional-columns";

            StateHasChanged();

            return new GridDataProviderResult<MealPlanModel>
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