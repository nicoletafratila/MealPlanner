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
        [ObservableProperty]
        private ObservableCollection<MealPlanModel> _mealPlans = [];

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private bool _hasNextPage;

        [RelayCommand]
        private async Task LoadAsync()
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
                var result = await mealPlanService.SearchAsync(new QueryParameters<MealPlanModel> { PageNumber = CurrentPage, Sorting = DefaultSorting });
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

        [RelayCommand]
        private Task AddAsync() => Shell.Current.GoToAsync($"MealPlanEdit?id={Guid.Empty}");

        [RelayCommand]
        private Task EditAsync(MealPlanModel mp) => Shell.Current.GoToAsync($"MealPlanEdit?id={mp.Id}");

        [RelayCommand]
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
