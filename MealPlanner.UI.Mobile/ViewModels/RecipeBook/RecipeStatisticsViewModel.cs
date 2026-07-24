using System.Collections.ObjectModel;
using Common.Pagination;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MealPlanner.Services.Http;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    public partial class RecipeStatisticsViewModel(
        IStatisticsService statisticsService,
        RecipeCategoryService recipeCategoryService) : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<CategoryStatisticModel> _categories = [];

        [RelayCommand]
        public async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ClearMessages();
            try
            {
                var categories = await recipeCategoryService.SearchAsync(new QueryParameters<RecipeCategoryModel> { PageSize = 500 });
                var data = await statisticsService.GetFavoriteRecipesAsync(categories?.Items?.ToList() ?? []);
                Categories = data is not null
                    ? new ObservableCollection<CategoryStatisticModel>(BuildCategoryStatistics(data.OrderBy(stat => stat.Title, StringComparer.CurrentCultureIgnoreCase)))
                    : [];
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

        private static IEnumerable<CategoryStatisticModel> BuildCategoryStatistics(IEnumerable<Common.Models.StatisticModel> statistics)
        {
            var stats = statistics.ToList();
            var grandTotal = stats.Sum(stat => stat.Data.Values.Sum(v => v ?? 0));

            foreach (var stat in stats)
            {
                var items = stat.Data
                    .Select(entry => (Name: entry.Key, Value: entry.Value ?? 0))
                    .OrderByDescending(entry => entry.Value)
                    .ToList();
                var maxVal = items.Count > 0 ? items.Max(entry => entry.Value) : 0;
                var totalValue = items.Sum(entry => entry.Value);

                yield return new CategoryStatisticModel
                {
                    Title = stat.Title ?? string.Empty,
                    TotalValue = totalValue,
                    SharePercentage = grandTotal > 0 ? totalValue / grandTotal * 100 : 0,
                    Items = items.Select((entry, index) => new RankedStatisticItemModel
                    {
                        Rank = index + 1,
                        ItemName = entry.Name,
                        Value = entry.Value,
                        BarFraction = maxVal > 0 ? entry.Value / maxVal : 0
                    }).ToList()
                };
            }
        }
    }
}
