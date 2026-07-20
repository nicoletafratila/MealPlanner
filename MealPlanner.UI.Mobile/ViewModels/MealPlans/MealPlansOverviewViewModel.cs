using System.Collections.ObjectModel;
using Common.Pagination;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.MealPlans
{
    public partial class MealPlansOverviewViewModel(IMealPlanService mealPlanService) : BaseViewModel
    {
        private static readonly List<SortingModel> CreatedAtDescendingSorting =
            [new SortingModel { PropertyName = "CreatedAt", Direction = SortDirection.Descending }];

        [ObservableProperty]
        private ObservableCollection<MealPlanModel> _mealPlans = [];

        [ObservableProperty]
        private string? _searchText;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private bool _hasNextPage;

        [ObservableProperty]
        private bool _isLoadingMore;

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task LoadAsync() => await SearchAsync();

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task SearchAsync()
        {
            if (IsBusy)
            {
                return;
            }
            IsBusy = true;
            CurrentPage = 1;
            ClearMessages();
            try
            {
                var filters = BuildFilters();
                var result = await mealPlanService.SearchAsync(new QueryParameters<MealPlanModel> { PageNumber = CurrentPage, Filters = filters.Count > 0 ? filters : null, Sorting = CreatedAtDescendingSorting });
                if (result is not null)
                {
                    MealPlans = new ObservableCollection<MealPlanModel>(result.Items);
                    HasNextPage = result.Metadata.HasNextPage;
                }
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnSearchTextChanged(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                SearchCommand.Execute(null);
            }
        }

        private List<FilterItem> BuildFilters()
        {
            var filters = new List<FilterItem>();
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filters.Add(new FilterItem("Name", SearchText, FilterOperator.Contains, StringComparison.OrdinalIgnoreCase));
            }
            return filters;
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task NextPageAsync()
        {
            if (IsLoadingMore || IsBusy || !HasNextPage)
            {
                return;
            }
            IsLoadingMore = true;
            try
            {
                CurrentPage++;
                await AppendPageAsync();
            }
            finally
            {
                IsLoadingMore = false;
            }
        }

        private async Task AppendPageAsync()
        {
            var filters = BuildFilters();
            var result = await mealPlanService.SearchAsync(new QueryParameters<MealPlanModel> { PageNumber = CurrentPage, Filters = filters.Count > 0 ? filters : null, Sorting = CreatedAtDescendingSorting });
            if (result is not null)
            {
                foreach (var item in result.Items)
                {
                    MealPlans.Add(item);
                }
                HasNextPage = result.Metadata.HasNextPage;
            }
        }

        [RelayCommand]
        private Task AddAsync() => Shell.Current.GoToAsync($"MealPlanEdit?id={Guid.Empty}");

        [RelayCommand]
        private Task EditAsync(MealPlanModel mp) => Shell.Current.GoToAsync($"MealPlanEdit?id={mp.Id}");

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task DeleteAsync(MealPlanModel mp)
        {
            var result = await mealPlanService.DeleteAsync(mp.Id);
            if (result?.Succeeded == true)
            {
                MealPlans.Remove(mp);
            }
            else
            {
                SetError(result?.Message);
            }
        }
    }
}
