using System.Collections.ObjectModel;
using Common.Pagination;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    public partial class UnitsOverviewViewModel(UnitService unitService) : BaseViewModel
    {
        [ObservableProperty] private ObservableCollection<UnitModel> _units = [];

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (IsBusy)
            {
                return;
            }
            IsBusy = true;
            ClearMessages();
            try
            {
                var result = await unitService.SearchAsync(new QueryParameters<UnitModel> { PageSize = 200, Sorting = DefaultSorting });
                if (result is not null)
                {
                    Units = new ObservableCollection<UnitModel>(result.Items);
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

        [RelayCommand] private Task AddAsync() => Shell.Current.GoToAsync("UnitEdit?id=0");
        [RelayCommand] private Task EditAsync(UnitModel u) => Shell.Current.GoToAsync($"UnitEdit?id={u.Id}");

        [RelayCommand]
        private async Task DeleteAsync(UnitModel u)
        {
            var result = await unitService.DeleteAsync(u.Id);
            if (result?.Succeeded == true)
            {
                Units.Remove(u);
            }
            else
            {
                SetError(result?.Message);
            }
        }
    }
}
