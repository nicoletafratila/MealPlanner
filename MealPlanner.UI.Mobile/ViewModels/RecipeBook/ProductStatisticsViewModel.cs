using System.Collections.ObjectModel;
using Common.Pagination;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MealPlanner.Services.Http;
using RecipeBook.Services.Core.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    public partial class ProductStatisticsViewModel(
        IStatisticsService statisticsService,
        ProductCategoryService productCategoryService) : BaseViewModel
    {
        [ObservableProperty] private ObservableCollection<StatisticEntryModel> _entries = [];

        [RelayCommand]
        public async Task LoadAsync()
        {
            if (IsBusy) return; IsBusy = true; ClearMessages();
            try
            {
                var categories = await productCategoryService.SearchAsync(new QueryParameters<ProductCategoryModel> { PageSize = 500 });
                var data = await statisticsService.GetFavoriteProductsAsync(categories?.Items?.ToList() ?? []);
                Entries = data is not null
                    ? new ObservableCollection<StatisticEntryModel>(FlattenStatistics(data))
                    : [];
            }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        private static IEnumerable<StatisticEntryModel> FlattenStatistics(IEnumerable<Common.Models.StatisticModel> statistics)
        {
            foreach (var stat in statistics)
            {
                yield return new StatisticEntryModel { GroupTitle = stat.Title ?? string.Empty, IsGroupHeader = true };
                var maxVal = stat.Data.Values.Where(v => v.HasValue).DefaultIfEmpty(0).Max(v => v ?? 0);
                foreach (var (name, val) in stat.Data)
                {
                    var v = val ?? 0;
                    yield return new StatisticEntryModel
                    {
                        GroupTitle = stat.Title ?? string.Empty,
                        ItemName = name,
                        Value = v,
                        BarFraction = maxVal > 0 ? v / maxVal : 0
                    };
                }
            }
        }
    }
}
